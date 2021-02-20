using Autofac;
using Autofac.Extras.DynamicProxy;
using Blog.Core.Common;
using Blog.Core.Extensions;
using Blog.Core.Repository;
using Blog.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core
{
    public class Startup
    {
        private readonly string DefaultPolicyName = "localhost";
        public IConfigurationRoot Configuration { get; }
        public Startup(IWebHostEnvironment env)
        {
            Configuration = HostingEnvironmentHelper.GetAppConfiguration(env);
        }

        public string ApiName { get; set; } = "Blog.Core";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var basePath = AppContext.BaseDirectory;
            BaseDBConfig.ConnectionString = Configuration["AppSettings:SqlServerConnection"];
            services.AddControllers();

            // 注入
            services.AddSingleton(Configuration);

            #region SWagger and JWT

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "V1",
                    Title = $"{ApiName} 接口文档--Net 5.0",
                    Description = $"{ApiName} HTTP API V1",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = ApiName,
                        Email = "Blog.Core@xxx.com",
                        Url = new Uri("https://github.com/itbetaw/Blog.Core-Vue.git")
                    },
                });
                c.OrderActionsBy(p => p.RelativePath);


                var xmlPath = Path.Combine(basePath, "Blog.Core.Web.Host.xml");
                c.IncludeXmlComments(xmlPath, true);


                //开启加权小锁

                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // 在header中添加token，传递到后台
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",// jwt默认的参数名称
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey

                });


            });

            #endregion

            #region 角色认证策略


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());

                options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));

            });



            #region 【第二步：配置认证服务】

            // 令牌验证参数
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = Configuration["Audience:Issuer"],//发行人
                ValidateAudience = true,
                ValidAudience = Configuration["Audience:Audience"],//订阅人
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RequireExpirationTime = true,
            };

            //2.1【认证】、core自带官方JWT认证
            // 开启Bearer认证
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             // 添加JwtBearer服务
             .AddJwtBearer(o =>
             {
                 o.TokenValidationParameters = tokenValidationParameters;
                 o.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         // 如果过期，则把<是否过期>添加到，返回头信息中
                         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                         {
                             context.Response.Headers.Add("Token-Expired", "true");
                         }
                         return Task.CompletedTask;
                     }
                 };
             });




            #endregion

            #endregion

            #region Redis缓存

            services.AddMemoryCache();
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddScoped<IRedisBasketRepository, RedisBasketRepository>();
            // 配置启动Redis服务，虽然可能影响项目启动速度，但是不能在运行的时候报错，所以是合理的
            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                //获取连接字符串
                string redisConfiguration = Configuration["AppSettings:RedisCaching:ConnectionString"];

                var configuration = ConfigurationOptions.Parse(redisConfiguration, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });
            #endregion

            #region CORS 跨域请求

            var origions = Configuration["App:CorsOrigins"]
                         .Split(",", StringSplitOptions.RemoveEmptyEntries);
            origions.ToList().ForEach(x =>
            {
                x.TrimEnd('/');
            });
            services.AddCors(c =>
            {

                c.AddPolicy(DefaultPolicyName, policy =>
                 {
                     policy.WithOrigins(origions)
                     .SetIsOriginAllowedToAllowWildcardSubdomains()
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
                 });
            });

            #endregion

            services.AddAutoMapper(typeof(CustomProfile));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<BlogLogAOP>();
            builder.RegisterType<BlogCacheAOP>();
            builder.RegisterType<BlogRedisAOP>();

            var assemblysServices = Assembly.GetAssembly(typeof(BaseServices<object>));
            //指定已扫描程序集中的类型注册为提供所有其实现的接口

            //通过反射加载repository
            var assemblyRepository = Assembly.GetAssembly(typeof(BaseRepository<object>));
            builder.RegisterAssemblyTypes(assemblyRepository, assemblysServices)
                      .AsImplementedInterfaces()
                      .InstancePerLifetimeScope()
                      .EnableInterfaceInterceptors()
                      .InterceptedBy(typeof(BlogLogAOP), typeof(BlogCacheAOP)
                      , typeof(BlogRedisAOP));


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"{ApiName} V1");
                c.RoutePrefix = "";

            });

            app.UseRouting();

            app.UseCors(DefaultPolicyName);

            // 跳转HTTPs
            app.UseHttpsRedirection();
            // 静态文件
            app.UseStaticFiles();

            // 缓存策略 使用Cookies
            app.UseCookiePolicy();
            // 返回错误码
            app.UseStatusCodePages();
            // 先开启认证
            app.UseAuthentication();

            // 授权中间件
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
