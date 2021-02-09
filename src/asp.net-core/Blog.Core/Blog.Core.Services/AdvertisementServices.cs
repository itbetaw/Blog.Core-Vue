using Blog.Core.IRepository;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Blog.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Blog.Core.Services
{
    public class AdvertisementServices : IAdvertisementServices
    {
        public IAdvertisementRepository dal = new AdvertisementRepository();
        public int Add(Advertisement model)
        {
            return dal.Add(model);
        }

        public bool Delete(Advertisement model)
        {
            return dal.Delete(model);
        }

        public List<Advertisement> Query(Expression<Func<Advertisement, bool>> whereExpression)
        {
            return dal.Query(whereExpression);
        }

        public bool Update(Advertisement model)
        {
            return dal.Update(model);
        }
    }
}
