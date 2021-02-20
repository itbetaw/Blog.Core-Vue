using Blog.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core.Web.Host.Controllers
{
    [Route("api/Login")]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        /// <summary>
        /// 获取JWT的方法
        /// </summary>
        /// <param name="name">id</param>
        /// <param name="pass">角色</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Token")]
        public MessageModel<string> GetJWTStr(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            //这里就是用户登录以后，通过数据库去调取数据，分配权限的操作
            //这里直接写死了
            if (name == "admins" && pass == "123")
            {
                TokenModelJwt tokenModel = new TokenModelJwt();
                tokenModel.Uid = 1;
                tokenModel.Role = "Admin";

                jwtStr = JwtHelper.IssueJwt(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }
            return new MessageModel<string>()
            {
                success = suc,
                msg = suc ? "获取成功" : "获取失败",
                response = jwtStr
            };
        }
    }
}
