using Blog.Core.Common;
using Blog.Core.IServices;
using Blog.Core.Model;
using Blog.Core.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Core.Web.Host.Controllers
{
    /// <summary>
    /// 博客管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        readonly IBlogArticleServices _blogArticleServices;
        private readonly IConfigurationRoot _configurationRoot;

        public BlogController(IBlogArticleServices blogArticleServices,
               IConfigurationRoot configurationAccessor
            )
        {
            _blogArticleServices = blogArticleServices;
            _configurationRoot = configurationAccessor;
        }

        [HttpGet]
        [Authorize(Policy = "SystemOrAdmin")]
        public string GetValue()
        {
            return "V1";
        }
        [HttpGet("{id}", Name = "Get")]
        public async Task<List<BlogArticle>> Get(int id)
        {
            return await _blogArticleServices.Query(d => d.bID == id);
        }

        [HttpGet]
        public MessageModel<string> GetJWTToken(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            {
                return new MessageModel<string>()
                {
                    success = false,
                    msg = "用户名或密码不能为空"
                };
            }
            TokenModelJwt tokenModel = new TokenModelJwt();
            tokenModel.Uid = 1;
            tokenModel.Role = "Admin";
            jwtStr = JwtHelper.IssueJwt(tokenModel);
            suc = true;

            return new MessageModel<string>()
            {
                success = suc,
                msg = suc ? "获取成功" : "获取失败",
                response = jwtStr
            };
        }

        [HttpGet]
        [Route("GetBlogs")]
        public async Task<List<BlogArticle>> GetBlogs()

        {
            var ass = _configurationRoot["AppSettings:SqlServerConnection"];

            var ss = _configurationRoot["AppSettings:RedisCaching:ConnectionString"];
            var connect = Appsettings.app(new string[] { "AppSettings", "RedisCaching",
                "ConnectionString" });

            return await _blogArticleServices.GetBlogs();
        }
    }

}
