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
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        // GET api/users?page=1&pageSize=20
        [HttpGet, Route]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Users.Include(u => u.Role).Where(u => !u.IsArchived);
                var total = await query.CountAsync();
                var users = await query.OrderBy(u => u.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                // Возвращаем пользователей без пароля
                var result = users.Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Login,
                    u.RoleId,
                    RoleName = u.Role.Name,
                    u.IsArchived
                });
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        // GET api/users/5
        [HttpGet, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null) return NotFound();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        user.Id,
                        user.FullName,
                        user.Login,
                        user.RoleId,
                        RoleName = user.Role.Name,
                        user.IsArchived
                    }
                });
            }
        }

        // POST api/users
        [HttpPost, Route]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            using (var db = new AppDbContext())
            {
                var user = new User
                {
                    FullName = request.FullName,
                    Login = request.Login,
                    PasswordHash = PasswordHasher.Hash(request.Password),
                    RoleId = request.RoleId,
                    IsArchived = false
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "users", id = user.Id },
                    new ApiResponse<object> { Success = true, Data = new { user.Id, user.FullName, user.Login } });
            }
        }

        // PUT api/users/5
        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            using (var db = new AppDbContext())
            {
                var user = await db.Users.FindAsync(id);
                if (user == null) return NotFound();
                user.FullName = request.FullName;
                user.Login = request.Login;
                user.RoleId = request.RoleId;
                if (!string.IsNullOrEmpty(request.NewPassword))
                    user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true, Data = new { user.Id, user.FullName, user.Login } });
            }
        }

        // DELETE api/users/5 (архивирование)
        [HttpDelete, Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Archive(int id)
        {
            using (var db = new AppDbContext())
            {
                var user = await db.Users.FindAsync(id);
                if (user == null) return NotFound();
                user.IsArchived = true;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }
    }

    // Вспомогательные модели запросов
    public class CreateUserRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string FullName { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Login { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string FullName { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Login { get; set; }
        public int RoleId { get; set; }
        public string NewPassword { get; set; } // если не задан, пароль не меняется
    }
}