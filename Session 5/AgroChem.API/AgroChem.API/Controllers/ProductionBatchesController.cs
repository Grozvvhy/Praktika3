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
    [RoutePrefix("api/production-batches")]
    public class ProductionBatchesController : ApiController
    {
        [HttpGet, Route]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.ProductionBatches
                    .Include(b => b.Recipe)
                    .Include(b => b.Card);
                var total = await query.CountAsync();
                var batches = await query.OrderBy(b => b.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var data = batches.Select(b => new
                {
                    b.Id,
                    RecipeVersion = b.Recipe.Version,
                    CardVersion = b.Card.Version,
                    b.StartDate,
                    b.Status,
                    b.PlannedQuantity
                });
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = data,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        [HttpGet, Route("{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var batch = await db.ProductionBatches
                    .Include(b => b.Recipe)
                    .Include(b => b.Card)
                    .FirstOrDefaultAsync(b => b.Id == id);
                if (batch == null) return NotFound();
                return Ok(new ApiResponse<object> { Success = true, Data = batch });
            }
        }

        [HttpPost, Route]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Create([FromBody] ProductionBatch batch)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            using (var db = new AppDbContext())
            {
                batch.Status = "Planned";
                db.ProductionBatches.Add(batch);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "production-batches", id = batch.Id },
                    new ApiResponse<ProductionBatch> { Success = true, Data = batch });
            }
        }

        [HttpPut, Route("{id:int}/start")]
        [Authorize(Roles = "Technologist")]
        public async Task<IHttpActionResult> Start(int id)
        {
            using (var db = new AppDbContext())
            {
                var batch = await db.ProductionBatches.FindAsync(id);
                if (batch == null) return NotFound();
                batch.Status = "InProgress";
                batch.StartDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<ProductionBatch> { Success = true, Data = batch });
            }
        }

        [HttpPut, Route("{id:int}/complete")]
        [Authorize(Roles = "Technologist")]
        public async Task<IHttpActionResult> Complete(int id)
        {
            using (var db = new AppDbContext())
            {
                var batch = await db.ProductionBatches.FindAsync(id);
                if (batch == null) return NotFound();
                batch.Status = "Completed";
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<ProductionBatch> { Success = true, Data = batch });
            }
        }

        // GET api/production-batches/5/program - технологическая программа для партии
        [HttpGet, Route("{id:int}/program")]
        public async Task<IHttpActionResult> GetProductionProgram(int id)
        {
            using (var db = new AppDbContext())
            {
                var batch = await db.ProductionBatches
                    .Include(b => b.Card.Steps)
                    .FirstOrDefaultAsync(b => b.Id == id);
                if (batch == null) return NotFound();
                var steps = batch.Card.Steps.OrderBy(s => s.StepNumber).Select(s => new
                {
                    s.StepNumber,
                    s.StepType,
                    Parameters = s.ParametersJson,
                    s.IsMandatory
                });
                return Ok(new ApiResponse<object> { Success = true, Data = steps });
            }
        }
    }
}