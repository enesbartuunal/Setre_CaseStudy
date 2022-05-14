using Setre.DataAccess.Attributes;
using Setre.Models.Models.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Setre.Business.Extensions
{
    public static class FilterDataExtension 
    {
        public static FilterResponseModel<T> GetDataAndPaggingInfo<T> (this IQueryable<T> dbEntities,FilterQueryParams queryParams) where T : class
        {
            FilterResponseModel<T> filterResponse = new FilterResponseModel<T>();
            var entity = typeof(T);
            PropertyInfo[] properties = entity.GetProperties();
            var searchProps = new List<PropertyInfo>();

            foreach (var prop in properties)
            {
                var attribute = prop.GetCustomAttribute(typeof(CustomSearchPropertyAttribute),false);
                if (attribute != null)
                {
                    searchProps.Add(prop);
                }
            }

            //Search

            if (!string.IsNullOrEmpty(queryParams.SearchValue) && searchProps.Count > 0)
            {
                ConstantExpression constant = Expression.Constant(queryParams.SearchValue.ToLower());
                ParameterExpression parameter = Expression.Parameter(typeof(T), "e");
                MemberExpression[] members = new MemberExpression[searchProps.Count];
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);

                for (int i = 0; i < searchProps.Count(); i++)
                {
                    members[i] = Expression.Property(parameter, searchProps[i]);
                }

                Expression predicate = null;
                foreach (var member in members)
                {
                    //e => e.Member != null
                    BinaryExpression notNullExp = Expression.NotEqual(member, Expression.Constant(null));
                    //e => e.Member.ToLower() 
                    MethodCallExpression toLowerExp = Expression.Call(member, toLowerMethod);
                    //e => e.Member.Contains(value)
                    MethodCallExpression containsExp = Expression.Call(toLowerExp, containsMethod, constant);
                    //e => e.Member != null && e.Member.Contains(value)
                    BinaryExpression filterExpression = Expression.AndAlso(notNullExp, containsExp);
                    predicate = predicate == null ? (Expression)filterExpression : Expression.OrElse(predicate, filterExpression);

                }
                var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);

                filterResponse.DataList = dbEntities.Where(lambda).Skip((queryParams.Page - 1) * queryParams.PageSize).Take(queryParams.PageSize).ToList();
            }

            //Pagging

            if (filterResponse.DataList == null)
            {
                filterResponse.DataList = dbEntities.Skip((queryParams.Page - 1) * queryParams.PageSize).Take(queryParams.PageSize).ToList();
            }

            FilterPaggingInfo paggingInfo = new FilterPaggingInfo();
            paggingInfo.TotalItemCount = dbEntities.Count();
            paggingInfo.CurrentPage = queryParams.Page;
            paggingInfo.TotalPageCount = (int)Math.Ceiling((double)(dbEntities.Count() / queryParams.PageSize));

            filterResponse.PaggingInfo = paggingInfo;

            //Sort
            if (queryParams.SortOptions != null && queryParams.SortOptions.Length > 0)
            {
                
                foreach (var item in queryParams.SortOptions)
                {
                    var property = entity.GetProperty(item.Trim());

                    if (!queryParams.SortingDirection)
                    {
                        filterResponse.DataList = filterResponse.DataList.OrderBy(x => property.GetValue(x, null)).ToList();
                    }
                    else
                    {
                        filterResponse.DataList = filterResponse.DataList.OrderByDescending(x => property.GetValue(x, null)).ToList();
                    }
                }
            }

            return filterResponse;
        }
    }
}
