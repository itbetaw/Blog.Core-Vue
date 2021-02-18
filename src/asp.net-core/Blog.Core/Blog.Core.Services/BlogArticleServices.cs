using Blog.Core.Common;
using Blog.Core.IRepository;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Blog.Core.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Services
{
    public class BlogArticleServices : BaseServices<BlogArticle>, IBlogArticleServices
    {
        IBaseRepository<BlogArticle> _dal;

        public BlogArticleServices(IBaseRepository<BlogArticle> dal)
        {
            this._dal = dal;
            this.baseDal = dal;
        }

        public Task<BlogViewModels> GetBlogDetails(int id)
        {
            throw new NotImplementedException();
        }

        [Caching(AbsoluteExpiration = 10)]
        public async Task<List<BlogArticle>> GetBlogs()
        {
            var blogList = await Query(x => x.bID > 0, x => x.bID);
            return blogList;
        }
    }
}
