using System;
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
    [RoutePrefix("api/recipes")]
    public class RecipesController : ApiController
    {
        // GET api/recipes?page=1&pageSize=20
        [HttpGet, Route]
        [Authorize]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Recipes.Include(r => r.Product);
                var total = await query.CountAsync();
                var recipes = await query.OrderBy(r => r.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var data = recipes.Select(r => new
                {
                    r.Id,
                    ProductName = r.Product.Name,
                    r.Version,
                    r.Status,
                    r.CreatedAt
                });
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = data,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        // GET api/recipes/5
        [HttpGet, Route("{id:int}")]
        [Authorize]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var recipe = await db.Recipes
                    .Include(r => r.Product)
                    .Include(r => r.Components.Select(c => c.RawMaterial))
                    .FirstOrDefaultAsync(r => r.Id == id);
                if (recipe == null) return NotFound();
                return Ok(new ApiResponse<object> { Success = true, Data = recipe });
            }
        }

        // POST api/recipes
        [HttpPost, Route]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Create([FromBody] Recipe recipe)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            using (var db = new AppDbContext())
            {
                recipe.Status = "Draft";
                recipe.CreatedAt = DateTime.UtcNow;
                db.Recipes.Add(recipe);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "recipes", id = recipe.Id },
                    new ApiResponse<Recipe> { Success = true, Data = recipe });
            }
        }

        // PUT api/recipes/5
        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] Recipe updated)
        {
            using (var db = new AppDbContext())
            {
                var recipe = await db.Recipes.FindAsync(id);
                if (recipe == null) return NotFound();
                recipe.Version = updated.Version;
                // Другие поля можно обновлять при необходимости
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Recipe> { Success = true, Data = recipe });
            }
        }

        // POST api/recipes/5/components
        [HttpPost, Route("{recipeId:int}/components")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> AddComponent(int recipeId, [FromBody] RecipeComponent component)
        {
            using (var db = new AppDbContext())
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null) return NotFound();
                component.RecipeId = recipeId;
                db.RecipeComponents.Add(component);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<RecipeComponent> { Success = true, Data = component });
            }
        }

        // PUT api/recipes/5/components/10
        [HttpPut, Route("{recipeId:int}/components/{componentId:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> UpdateComponent(int recipeId, int componentId, [FromBody] RecipeComponent updated)
        {
            using (var db = new AppDbContext())
            {
                var comp = await db.RecipeComponents
                    .FirstOrDefaultAsync(c => c.Id == componentId && c.RecipeId == recipeId);
                if (comp == null) return NotFound();
                comp.RawMaterialId = updated.RawMaterialId;
                comp.Percentage = updated.Percentage;
                comp.LoadOrder = updated.LoadOrder;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<RecipeComponent> { Success = true, Data = comp });
            }
        }

        // DELETE api/recipes/5/components/10
        [HttpDelete, Route("{recipeId:int}/components/{componentId:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> RemoveComponent(int recipeId, int componentId)
        {
            using (var db = new AppDbContext())
            {
                var comp = await db.RecipeComponents
                    .FirstOrDefaultAsync(c => c.Id == componentId && c.RecipeId == recipeId);
                if (comp == null) return NotFound();
                db.RecipeComponents.Remove(comp);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }

        // PUT api/recipes/5/approve
        [HttpPut, Route("{id:int}/approve")]
        [Authorize(Roles = "Technologist")]
        public async Task<IHttpActionResult> Approve(int id)
        {
            using (var db = new AppDbContext())
            {
                var recipe = await db.Recipes.Include(r => r.Components).FirstOrDefaultAsync(r => r.Id == id);
                if (recipe == null) return NotFound();

                var total = recipe.Components.Sum(c => c.Percentage);
                if (total != 100m)
                    return BadRequest("Сумма долей компонентов должна быть равна 100%");

                recipe.Status = "Approved";
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Recipe> { Success = true, Data = recipe });
            }
        }

        // PUT api/recipes/5/archive
        [HttpPut, Route("{id:int}/archive")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Archive(int id)
        {
            using (var db = new AppDbContext())
            {
                var recipe = await db.Recipes.FindAsync(id);
                if (recipe == null) return NotFound();
                recipe.Status = "Archived";
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<Recipe> { Success = true, Data = recipe });
            }
        }
    }
}