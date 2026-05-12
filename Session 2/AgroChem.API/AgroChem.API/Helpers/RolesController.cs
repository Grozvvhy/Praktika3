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
    [RoutePrefix("api/roles")]
    public class RolesController : ApiController
    {
        [HttpGet, Route]
        [Authorize]
        public async Task<IHttpActionResult> GetAll()
        {
            using (var db = new AppDbContext())
            {
                var roles = await db.Roles.ToListAsync();
                return Ok(new ApiResponse<List<Role>> { Success = true, Data = roles });
            }
        }

        [HttpGet, Route("{id:int}")]
        [Authorize]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var role = await db.Roles.FindAsync(id);
                if (role == null) return NotFound();
                return Ok(new ApiResponse<Role> { Success = true, Data = role });
            }
        }

        [HttpPost, Route]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Create([FromBody] Role role)
        {
            using (var db = new AppDbContext())
            {
                db.Roles.Add(role);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "roles", id = role.Id },
                    new ApiResponse<Role> { Success = true, Data = role });
            }
        }

        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] Role updated)
        {
            using (var db = new AppDbContext())
            {
                var role = await db.Roles.FindAsync(id);
                if (role == null) return NotFound();
                role.Name = updated.Name;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Role> { Success = true, Data = role });
            }
        }

        [HttpDelete, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            using (var db = new AppDbContext())
            {
                var role = await db.Roles.FindAsync(id);
                if (role == null) return NotFound();
                // Проверка, что нет связанных пользователей, иначе ошибка внешнего ключа
                db.Roles.Remove(role);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }
    }
}