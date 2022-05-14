using AutoMapper;
using Setre.DataAccess.Entities;
using Setre.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Setre.Business.Mapper
{
   public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductModel>();
            CreateMap<ProductModel, Product>();
        }
           
    }
    
}
