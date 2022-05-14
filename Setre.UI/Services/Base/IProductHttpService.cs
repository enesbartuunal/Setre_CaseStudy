using Setre.Common;
using Setre.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Setre.UI.Services.Base
{
    public interface IProductHttpService
    {
        Task<Result<IEnumerable<ProductModel>>> GetProductsByCategory();
        Task<Result<ProductModel>> GetProductById(string id);
        Task<Result<ProductModel>> CreateProduct(ProductModel model);
        Task<Result<ProductModel>> UpdateProduct(ProductModel model);
        Task<Result<ProductModel>> DeleteProduct(string id);
        Task<string> UploadProductImage(MultipartFormDataContent content);


    }
}
