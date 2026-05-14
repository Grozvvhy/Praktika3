using System.Collections.Generic;
using System.Web.Http;
using AgroChem.API.Models;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        // Временное хранилище в памяти (для демонстрации)
        private static List<Product> _products = new List<Product>
        {
            new Product { Id = 1, Name = "Гербицид А", Type = "Гербицид", Form = "Жидкость", IsActive = true },
            new Product { Id = 2, Name = "Инсектицид Б", Type = "Инсектицид", Form = "Концентрат", IsActive = true },
            new Product { Id = 3, Name = "Фунгицид В", Type = "Фунгицид", Form = "Порошок", IsActive = false }
        };
        private static int _nextId = 4;

        // GET /api/products
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProducts()
        {
            return Ok(_products);
        }

        // GET /api/products/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetProduct(int id)
        {
            var product = _products.Find(p => p.Id == id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // POST /api/products
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateProduct([FromBody] Product product)
        {
            if (product == null)
                return BadRequest("Product is null");

            // Простейшая валидация
            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest("Name is required");

            product.Id = _nextId++;
            _products.Add(product);
            return Ok(product); // Возвращаем созданный объект с присвоенным Id
        }

        // PUT /api/products/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            if (product == null || product.Id != id)
                return BadRequest("Product ID mismatch");

            var existing = _products.Find(p => p.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = product.Name;
            existing.Type = product.Type;
            existing.Form = product.Form;
            existing.IsActive = product.IsActive;

            return Ok(existing);
        }

        // DELETE /api/products/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteProduct(int id)
        {
            var product = _products.Find(p => p.Id == id);
            if (product == null)
                return NotFound();

            _products.Remove(product);
            return Ok();
        }
    }
}