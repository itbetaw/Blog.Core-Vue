using Blog.Core.IRepository;
using Blog.Core.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Repository
{
    public class BlogArticleRepository : BaseRepository<BlogArticle>, IBlogArticleRepository
    {
    }
}
