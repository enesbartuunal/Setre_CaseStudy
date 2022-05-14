using Newtonsoft.Json;
using Setre.Common;
using Setre.Models.Models;
using Setre.UI.Services.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Setre.UI.Services.Implementation
{
    public class ProductHttpService : IProductHttpService
    {
        private readonly HttpClient _client;

        public ProductHttpService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Result<ProductModel>> CreateProduct(ProductModel model)
        {
            var content = JsonConvert.SerializeObject(model);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("products/", bodyContent);
            string res = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ProductModel>(data);
                return new Result<ProductModel>(true, ResultConstant.RecordCreateSuccessfully, result);
            }
            else
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                return new Result<ProductModel>(false, ResultConstant.RecordCreateNotSuccessfully);
            }
        }

        public async Task<Result<ProductModel>> DeleteProduct(string id)
        {
            var url = Path.Combine("products", id.ToString());
            var deleteResult = await _client.DeleteAsync(url);
            var deleteContent = await deleteResult.Content.ReadAsStringAsync();
            if (deleteResult.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ProductModel>(deleteContent);
                return new Result<ProductModel>(true, ResultConstant.RecordRemoveSuccessfully, result);
            }
            else
            {
                return new Result<ProductModel>(false, ResultConstant.RecordRemoveNotSuccessfully);
            }
        }

        public async Task<Result<ProductModel>> GetProductById(string id)
        {
            var url = Path.Combine("products/", id);
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(content);
            }
            var category = JsonConvert.DeserializeObject<ProductModel>(content);
            return new Result<ProductModel>(true, ResultConstant.RecordFound, category);
        }

        public async Task<Result<IEnumerable<ProductModel>>> GetProductsByCategory()
        {
            var response = await _client.GetAsync("products/getall");
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<IEnumerable<ProductModel>>(content);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(content);
            }

            return new Result<IEnumerable<ProductModel>>(true, ResultConstant.RecordFound, products);
        }

        public async Task<Result<ProductModel>> UpdateProduct(ProductModel model)
        {
            var content = JsonConvert.SerializeObject(model);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var url = Path.Combine("products", model.ProductID.ToString());
            var putResult = await _client.PutAsync(url, bodyContent);
            var putContent = await putResult.Content.ReadAsStringAsync();
            if (putResult.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ProductModel>(putContent);
                return new Result<ProductModel>(true, ResultConstant.RecordUpdateSuccessfully, result);
            }
            else
            {
                return new Result<ProductModel>(false, ResultConstant.RecordUpdateNotSuccessfully);
            }
        }

        public async Task<string> UploadProductImage(MultipartFormDataContent content)
        {
            var postResult = await _client.PostAsync("https://localhost:44341/api/upload", content);
            var postContent = await postResult.Content.ReadAsStringAsync();
            if (!postResult.IsSuccessStatusCode)
            {
                throw new ApplicationException(postContent);
            }
            else
            {
                var imgUrl = Path.Combine("https://localhost:5011/", postContent);
                return imgUrl;
            }
        }
    }
}
