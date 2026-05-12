using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AgroChem.Data;
using AgroChem.Data.Models;
using AgroChem.API.Helpers;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        // GET api/products?page=1&pageSize=20
        [HttpGet, Route]
        [Authorize]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Products.Where(p => !p.IsArchived);
                var total = await query.CountAsync();
                var items = await query.OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Ok(new ApiResponse<List<Product>>
                {
                    Success = true,
                    Data = items,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        // GET api/products/5
        [HttpGet, Route("{id:int}")]
        [Authorize]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var product = await db.Products.FindAsync(id);
                if (product == null) return NotFound();
                return Ok(new ApiResponse<Product> { Success = true, Data = product });
            }
        }

        // POST api/products
        [HttpPost, Route]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Create([FromBody] Product product)
        {
            using (var db = new AppDbContext())
            {
                db.Products.Add(product);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "products", id = product.Id },
                    new ApiResponse<Product> { Success = true, Data = product });
            }
        }

        // PUT api/products/5
        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] Product updated)
        {
            using (var db = new AppDbContext())
            {
                var product = await db.Products.FindAsync(id);
                if (product == null) return NotFound();
                product.Name = updated.Name;
                product.Code = updated.Code;
                // IsArchived меняется только через метод Archive
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Product> { Success = true, Data = product });
            }
        }

        // DELETE api/products/5 (архивирование)
        [HttpDelete, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Archive(int id)
        {
            using (var db = new AppDbContext())
            {
                var product = await db.Products.FindAsync(id);
                if (product == null) return NotFound();
                product.IsArchived = true;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Product> { Success = true, Data = product });
            }
        }
    }
}