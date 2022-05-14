using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Setre.Business.Implementaion;
using Setre.Common;
using Setre.Models.Models;
using Setre.Models.Models.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Setre.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;
        private readonly IMemoryCache _memoryCache;

        public ProductsController(ProductService service)
        {
            _service = service;
        }

        // GET: api/<ProductsController>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.Get();
            return Ok(list.Data);
        }

        // GET api/<ProductsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id is null)
                return BadRequest(new Result<IActionResult>(false, ResultConstant.IdNotNull));
            var data = await _service.GetById((int)id);
            if (data != null)
                return Ok(data.Data);
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordNotFound));
        }

        // GET api/<ProductsController>?<id>=2
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] FilterQueryParams queryParams)
        {
            var data =  _service.GetByFilters(queryParams);
            if (data != null)
                return Ok(data);
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordNotFound));
        }

        // POST api/<ProductsControllerController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductModel model)
        {
            var data = await _service.Add(model);
            if (data.IsSuccess)
                return StatusCode(201);
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordCreateNotSuccessfully));
        }

        // PUT api/<ProductsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductModel model)
        {

            model.ProductID = id;
            var data = await _service.Update(model);
            if (data.IsSuccess)
                return Ok(new Result<IActionResult>(true, ResultConstant.RecordUpdateSuccessfully));
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordUpdateNotSuccessfully));


        }

        // DELETE api/<ProductsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return BadRequest(new Result<IActionResult>(false, ResultConstant.IdNotNull));
            var data = await _service.Delete((int)id);
            if (data.IsSuccess)
                return Ok(new Result<IActionResult>(true, ResultConstant.RecordRemoveSuccessfully));
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordRemoveNotSuccessfully));
        }
    }
}
