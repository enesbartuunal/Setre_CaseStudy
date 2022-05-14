using AutoMapper;
using Setre.Business.Base;
using Setre.Business.Extensions;
using Setre.Common;
using Setre.DataAccess.Context;
using Setre.DataAccess.Entities;
using Setre.Models.Models;
using Setre.Models.Models.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
