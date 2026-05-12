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
    [RoutePrefix("api/equipment")]
    public class EquipmentController : ApiController
    {
        [HttpGet, Route]
        [Authorize]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Equipment.Where(e => !e.IsArchived);
                var total = await query.CountAsync();
                var items = await query.OrderBy(e => e.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Ok(new ApiResponse<List<Equipment>>
                {
                    Success = true,
                    Data = items,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        [HttpGet, Route("{id:int}")]
        [Authorize]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var equip = await db.Equipment.FindAsync(id);
                if (equip == null) return NotFound();
                return Ok(new ApiResponse<Equipment> { Success = true, Data = equip });
            }
        }

        [HttpPost, Route]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Create([FromBody] Equipment equip)
        {
            using (var db = new AppDbContext())
            {
                db.Equipment.Add(equip);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "equipment", id = equip.Id },
                    new ApiResponse<Equipment> { Success = true, Data = equip });
            }
        }

        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] Equipment updated)
        {
            using (var db = new AppDbContext())
            {
                var equip = await db.Equipment.FindAsync(id);
                if (equip == null) return NotFound();
                equip.Name = updated.Name;
                equip.Type = updated.Type;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Equipment> { Success = true, Data = equip });
            }
        }

        [HttpDelete, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Archive(int id)
        {
            using (var db = new AppDbContext())
            {
                var equip = await db.Equipment.FindAsync(id);
                if (equip == null) return NotFound();
                equip.IsArchived = true;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Equipment> { Success = true, Data = equip });
            }
        }
    }
}