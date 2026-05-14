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
    [RoutePrefix("api/raw-materials")]
    public class RawMaterialsController : ApiController
    {
        // GET api/raw-materials?page=1&pageSize=20
        [HttpGet, Route]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.RawMaterials.Where(m => !m.IsArchived);
                var total = await query.CountAsync();
                var items = await query.OrderBy(m => m.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Ok(new ApiResponse<List<RawMaterial>>
                {
                    Success = true,
                    Data = items,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        // GET api/raw-materials/5
        [HttpGet, Route("{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var material = await db.RawMaterials.FindAsync(id);
                if (material == null) return NotFound();
                return Ok(new ApiResponse<RawMaterial> { Success = true, Data = material });
            }
        }

        // POST api/raw-materials
        [HttpPost, Route]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Create([FromBody] RawMaterial material)
        {
            using (var db = new AppDbContext())
            {
                db.RawMaterials.Add(material);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "raw-materials", id = material.Id },
                    new ApiResponse<RawMaterial> { Success = true, Data = material });
            }
        }

        // PUT api/raw-materials/5
        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] RawMaterial updated)
        {
            using (var db = new AppDbContext())
            {
                var material = await db.RawMaterials.FindAsync(id);
                if (material == null) return NotFound();
                material.Name = updated.Name;
                material.Code = updated.Code;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<RawMaterial> { Success = true, Data = material });
            }
        }

        // DELETE api/raw-materials/5 (архивирование)
        [HttpDelete, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Archive(int id)
        {
            using (var db = new AppDbContext())
            {
                var material = await db.RawMaterials.FindAsync(id);
                if (material == null) return NotFound();
                material.IsArchived = true;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<RawMaterial> { Success = true, Data = material });
            }
        }
    }
}