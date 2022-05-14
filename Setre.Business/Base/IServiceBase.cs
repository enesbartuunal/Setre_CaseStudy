using Setre.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Setre.Business.Base
{
    public interface IServiceBase<TEntity,TModel> where TEntity:class,new() where TModel: class ,new()
    {
        Task<Result<IEnumerable<TModel>>>Get(Expression<Func<TModel, bool>> predicate = null);


        Task<Result<TModel>> GetById(int id);


        Task<Result<TModel>> Add(TModel model);


        Task<Result<TModel>> Update(TModel model);


        Task<Result<TModel>> Delete(int id);
    }
}
