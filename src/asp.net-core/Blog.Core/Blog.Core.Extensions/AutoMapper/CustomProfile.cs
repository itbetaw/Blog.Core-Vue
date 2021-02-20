using AutoMapper;
using Blog.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Extensions
{
    public class CustomProfile : Profile
    {
        public CustomProfile()
        {

            CreateMap<BlogArticle, BlogViewModels>();
            CreateMap<BlogViewModels, BlogArticle>();
        }
    }
}
