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

            // ע��
            services.AddSingleton(Configuration);

            #region SWagger and JWT

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "V1",
                    Title = $"{ApiName} �ӿ��ĵ�--Net 5.0",
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


                //������ȨС��

                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // ��header�����token�����ݵ���̨
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
                    Name = "Authorization",// jwtĬ�ϵĲ�������
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey

                });


            });

            #endregion

            #region ��ɫ��֤����


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());

                options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));

            });



            #region ���ڶ�����������֤����

            // ������֤����
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = Configuration["Audience:Issuer"],//������
                ValidateAudience = true,
                ValidAudience = Configuration["Audience:Audience"],//������
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RequireExpirationTime = true,
            };

            //2.1����֤����core�Դ��ٷ�JWT��֤
            // ����Bearer��֤
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             // ���JwtBearer����
             .AddJwtBearer(o =>
             {
                 o.TokenValidationParameters = tokenValidationParameters;
                 o.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         // ������ڣ����<�Ƿ����>��ӵ�������ͷ��Ϣ��
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

            #region Redis����

            services.AddMemoryCache();
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddScoped<IRedisBasketRepository, RedisBasketRepository>();
            // ��������Redis������Ȼ����Ӱ����Ŀ�����ٶȣ����ǲ��������е�ʱ�򱨴������Ǻ����
            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                //��ȡ�����ַ���
                string redisConfiguration = Configuration["AppSettings:RedisCaching:ConnectionString"];

                var configuration = ConfigurationOptions.Parse(redisConfiguration, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });
            #endregion

            #region CORS ��������

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
            //ָ����ɨ������е�����ע��Ϊ�ṩ������ʵ�ֵĽӿ�

            //ͨ���������repository
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

            // ��תHTTPs
            app.UseHttpsRedirection();
            // ��̬�ļ�
            app.UseStaticFiles();

            // ������� ʹ��Cookies
            app.UseCookiePolicy();
            // ���ش�����
            app.UseStatusCodePages();
            // �ȿ�����֤
            app.UseAuthentication();

            // ��Ȩ�м��
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
