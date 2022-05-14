## Ürünlerin verimodeli,tasarım detayları ve dokümanı analiz edildi ve gerekli olan veriler yalın bir sekilde tasarlanıp, codefirst yaklaşımı ile veri tabanı oluşturuldu.
- Setre.DataAccess Katmanında bu işlemler incelenebilir.
- Sql ile ilgili script projeye eklendi.

## İstenilen mimari,teknoloji ve paternler kullanıldı.
- Çok katmanlı mimari,Generic Repository pattern, Entity Framework Core

## Crud işlemleri için katmanlı yapı mimariye uygun generic bir abstract class oluşturuldu.
```ruby
namespace Setre.Business.Base
{
    public class ServiceAbstractBase<TEntity, TModel> : IServiceBase<TEntity, TModel> where TEntity : class, new() where TModel : class, new()
    {
        private readonly SetreDbContext _db;
        private readonly IMapper _mapper;

        public ServiceAbstractBase(SetreDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<TModel>> Add(TModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            var addedRecord = await _db.Set<TEntity>().AddAsync(entity);
            await _db.SaveChangesAsync();
            var returndata = _mapper.Map<TModel>(addedRecord.Entity);
            return new Result<TModel>(true, ResultConstant.RecordCreateSuccessfully, returndata);
        }

        public async Task<Result<TModel>> Delete(int id)
        {
            var entity = await _db.Set<TEntity>().FindAsync(id);
            if (entity != null)
            {
                _db.Set<TEntity>().Remove(entity);
                await _db.SaveChangesAsync();
                return new Result<TModel>(true, ResultConstant.RecordRemoveSuccessfully);
            }
            return new Result<TModel>(false, ResultConstant.RecordRemoveNotSuccessfully);
        }

        public async Task<Result<IEnumerable<TModel>>> Get(Expression<Func<TModel, bool>> predicate = null)
        {
            try
            {
                if (predicate != null)
                {
                    var visitor = new ParameterTypeVisitor<TModel, TEntity>(predicate);
                    var entityLambda = visitor.Convert();
                    var listquery = _db.Set<TEntity>().Where(entityLambda);
                    var modellistquery = _mapper.Map<IEnumerable<TEntity>, IEnumerable<TModel>>(listquery);
                    return  new Result<IEnumerable<TModel>>(true, ResultConstant.RecordFound, modellistquery, modellistquery.ToList().Count());
                }
                else
                {
                    var listquery = _db.Set<TEntity>();
                    var modellistquery = _mapper.Map<IEnumerable<TEntity>, IEnumerable<TModel>>(listquery);
                    return new Result<IEnumerable<TModel>>(true, ResultConstant.RecordFound, modellistquery, modellistquery.ToList().Count());
                }

            }
            catch (Exception)
            {
                return new Result<IEnumerable<TModel>>(false, ResultConstant.RecordNotFound);
            }
        }

        public async Task<Result<TModel>> GetById(int id)
        {
            try
            {
                var entity = await _db.Set<TEntity>().FindAsync(id);
                var returndata = _mapper.Map<TModel>(entity);
                return new Result<TModel>(true, ResultConstant.RecordFound, returndata);
            }
            catch (Exception)
            {
                return new Result<TModel>(false, ResultConstant.RecordNotFound);
            }
        }

        public async Task<Result<TModel>> Update(TModel model)
        {
            try
            {
                var entity = _mapper.Map<TEntity>(model);
                _db.Entry(entity).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                var returndata = _mapper.Map<TModel>(entity);
                return new Result<TModel>(true, ResultConstant.RecordUpdateSuccessfully, returndata);
            }
            catch (Exception)
            {
                return new Result<TModel>(false, ResultConstant.RecordUpdateNotSuccessfully);
            }
        }

        public class ParameterTypeVisitor<TFrom, TTo> : ExpressionVisitor
        {

            private Dictionary<string, ParameterExpression> convertedParameters;
            private Expression<Func<TFrom, bool>> expression;

            public ParameterTypeVisitor(Expression<Func<TFrom, bool>> expresionToConvert)
            {

                //for each parameter in the original expression creates a new parameter with the same name but with changed type 
                convertedParameters = expresionToConvert.Parameters
                    .ToDictionary(
                        x => x.Name,
                        x => Expression.Parameter(typeof(TTo), x.Name)
                    );

                expression = expresionToConvert;
            }

            public Expression<Func<TTo, bool>> Convert()
            {
                return (Expression<Func<TTo, bool>>)Visit(expression);
            }

            //handles Properties and Fields accessors 
            protected override Expression VisitMember(MemberExpression node)
            {
                //we want to replace only the nodes of type TFrom
                //so we can handle expressions of the form x=> x.Property.SubProperty
                //in the expression x=> x.Property1 == 6 && x.Property2 == 3
                //this replaces         ^^^^^^^^^^^         ^^^^^^^^^^^            
                if (node.Member.DeclaringType == typeof(TFrom))
                {
                    //gets the memberinfo from type TTo that matches the member of type TFrom
                    var memeberInfo = typeof(TTo).GetMember(node.Member.Name).First();

                    //this will actually call the VisitParameter method in this class
                    var newExp = Visit(node.Expression);
                    return Expression.MakeMemberAccess(newExp, memeberInfo);
                }
                else
                {
                    return base.VisitMember(node);
                }
            }

            // this will be called where ever we have a reference to a parameter in the expression
            // for ex. in the expression x=> x.Property1 == 6 && x.Property2 == 3
            // this will be called twice     ^                   ^
            protected override Expression VisitParameter(ParameterExpression node)
            {
                var newParameter = convertedParameters[node.Name];
                return newParameter;
            }

            //this will be the first Visit method to be called
            //since we're converting LamdaExpressions
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                //visit the body of the lambda, this will Traverse the ExpressionTree 
                //and recursively replace parts of the expression we for which we have matching Visit methods 
                var newExp = Visit(node.Body);

                //this will create the new expression            
                return Expression.Lambda(newExp, convertedParameters.Select(x => x.Value));
            }
        }
    }
}
```


## Crud işlemlerin hepsi yapılmaktadır.Ayrıca  Tek bir endpoint ten arama, filtreleme ve sıralama işlemlerini yapılan generic method yazıldı.
- FilterDataExtension.cs=> Bu method db'ye yapılan her sorgu için kullanılabilir. 

* FilterResponseModel, FilterQueryParams, FilterPaggingInfo classları oluşturuldu.

```ruby
    public class FilterPaggingInfo
    {
        
        public int TotalItemCount { get; set; } = 0;
        public int TotalPageCount { get; set; } = 1;

        public int CurrentPage { get; set; } = 1;

        private int _nextPage;
        public int NextPage
        {
            get
            {
                _nextPage = CurrentPage == TotalPageCount ? CurrentPage : CurrentPage + 1;
                return _nextPage;
            }
        }

        private int _previousPage;

        public int PreviousPage
        {
            get
            {
                _previousPage = CurrentPage == 1 ? CurrentPage : CurrentPage - 1;
                return _previousPage;
            }
        }
    }
  ``` 

  ```ruby
    public class FilterQueryParams
    {
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;
        public string[] SortOptions { get; set; } = null; 
        public bool SortingDirection { get; set; } = false; //false = asc, true = desc
        public string SearchValue { get; set; } = null;
    }
    
    public class FilterResponseModel<T> where T: class
    {
        public List<T> DataList { get; set; }

        public FilterPaggingInfo PaggingInfo { get; set; }
    }
   ```
    
   ## FilterDataExtension adında IQueryable extend eden method yazıldı.
    
   
   ```ruby
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
    ```
    
    ## Urun listelemede kullanıldı.
    
    ```ruby
    namespace Setre.Business.Implementaion
    {
    public class ProductService : ServiceAbstractBase<Product, ProductModel>
    {
        private readonly SetreDbContext _db;
        private readonly IMapper _mapper;

        public ProductService(SetreDbContext db, IMapper mapper) : base(db, mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        //Bonus//
        public FilterResponseModel<ProductModel> GetByFilters(FilterQueryParams pr)
        {

            FilterResponseModel<Product> productReponse = _db.Products.GetDataAndPaggingInfo<Product>(pr);
            var responseModels = _mapper.Map<List<ProductModel>>(productReponse.DataList);

            FilterResponseModel<ProductModel> vmResponse = new FilterResponseModel<ProductModel>();
            vmResponse.DataList = responseModels;
            vmResponse.PaggingInfo = productReponse.PaggingInfo;

            return vmResponse;
        }
               
        }
    }
   ```
    
   ## Frontend teknolojisi olarak Blazor kullanıldı.Normalde Mvc Teknolojisi ile bu geliştirme yapılabilirdi.Fakat component yapısına daha uygun oldugunu düşündüğüm için blazor kullandım.
   
    -basit olarak sayfaya login ve registir işlemleri yapılıyor.
    -Basit token kullanımı yapıldı.(Jwt Bear)
    -Sayfalama işlemleri için hazır kütüphane kullanıldı.(Radzen)
    -InMemory cacheleme işlemi yapıldı.(GetById metodu)
    
   ## Kullanıcı uyarıları için Setre.Common katmanı yapıldı.
      -UI tarafında Blazor.Toaster kutuphanesi kullanılarak uyarılar kullanıcıya aktarıldı.
   
   ## Api katmanı ilk ayaga kalkısında default olarak bir kullanıcı start.up dosyasında tanımlamdı.Bilgileri appsetting.json dosyasına eklendı.
   
   ## Api ilk ayaga kalkısında  appsetting.json dosyası(defaultconnection) kullanıcı bilgisayarına gore duzenlenmelidir.
   
   
