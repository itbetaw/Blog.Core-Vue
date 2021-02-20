using Blog.Core.Common;
using Blog.Core.IServices;
using Blog.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Blog.Core.Web.Host.Controllers
{
    /// <summary>
    /// 博客管理
    /// </summary>
    [Route("api/Blog")]
    [Authorize(Policy = "Admin")]
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
        /// <summary>
        /// 获取博客详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        //[Authorize(Policy = "Scope_BlogModule_Policy")]
        [Authorize]
        public async Task<MessageModel<BlogViewModels>> Get(int id)
        {
            var response = await _blogArticleServices.GetBlogDetails(id);
            return new MessageModel<BlogViewModels>()
            {
                msg = "获取成功",
                success = true,
                response = response

            };
        }


        [HttpGet]
        [Route("GetBlogs")]
        public async Task<MessageModel<PageModel<BlogArticle>>> Get(int id, int page = 1, string bcategory = "技术博文",
            string key = "")
        {
            int intPageSize = 6;
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            Expression<Func<BlogArticle, bool>> whereExpression = a => (a.bcategory == bcategory && a.IsDeleted == false) && ((a.btitle != null && a.btitle.Contains(key)) || (a.bcontent != null && a.bcontent.Contains(key)));

            var pageModelBlog = await _blogArticleServices.QueryPage(whereExpression, page, intPageSize, " bID desc ");
            return new MessageModel<PageModel<BlogArticle>>()
            {
                success = true,
                msg = "获取成功",
                response = new PageModel<BlogArticle>()
                {
                    page = page,
                    dataCount = pageModelBlog.dataCount,
                    data = pageModelBlog.data,
                    pageCount = pageModelBlog.pageCount,
                }
            };
        }
    }

}
