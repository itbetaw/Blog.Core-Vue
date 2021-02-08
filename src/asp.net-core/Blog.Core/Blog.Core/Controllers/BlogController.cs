using Blog.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [HttpGet]
        [Authorize(Policy = "SystemOrAdmin")]
        public string GetValue()
        {
            return "V1";
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
    }

}
