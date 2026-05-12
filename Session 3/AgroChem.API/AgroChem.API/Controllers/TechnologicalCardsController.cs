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
    [RoutePrefix("api/technological-cards")]
    public class TechnologicalCardsController : ApiController
    {
        // GET api/technological-cards?page=1&pageSize=20
        [HttpGet, Route]
        [Authorize]
        public async Task<IHttpActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.TechnologicalCards.Include(c => c.Product);
                var total = await query.CountAsync();
                var cards = await query.OrderBy(c => c.Id)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var data = cards.Select(c => new
                {
                    c.Id,
                    ProductName = c.Product.Name,
                    c.Version,
                    c.Status
                });
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = data,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }

        // GET api/technological-cards/5
        [HttpGet, Route("{id:int}")]
        [Authorize]
        public async Task<IHttpActionResult> GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var card = await db.TechnologicalCards
                    .Include(c => c.Product)
                    .Include(c => c.Steps)
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (card == null) return NotFound();
                return Ok(new ApiResponse<object> { Success = true, Data = card });
            }
        }

        // POST api/technological-cards
        [HttpPost, Route]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Create([FromBody] TechnologicalCard card)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            using (var db = new AppDbContext())
            {
                card.Status = "Draft";
                db.TechnologicalCards.Add(card);
                await db.SaveChangesAsync();
                return CreatedAtRoute("DefaultApi", new { controller = "technological-cards", id = card.Id },
                    new ApiResponse<TechnologicalCard> { Success = true, Data = card });
            }
        }

        // PUT api/technological-cards/5
        [HttpPut, Route("{id:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Update(int id, [FromBody] TechnologicalCard updated)
        {
            using (var db = new AppDbContext())
            {
                var card = await db.TechnologicalCards.FindAsync(id);
                if (card == null) return NotFound();
                card.Version = updated.Version;
                // при необходимости можно расширить
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<TechnologicalCard> { Success = true, Data = card });
            }
        }

        // POST api/technological-cards/5/steps
        [HttpPost, Route("{cardId:int}/steps")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> AddStep(int cardId, [FromBody] TechnologicalStep step)
        {
            using (var db = new AppDbContext())
            {
                var card = await db.TechnologicalCards.FindAsync(cardId);
                if (card == null) return NotFound();
                step.CardId = cardId;
                // Установим номер шага автоматически на максимум + 1, если не задан
                if (step.StepNumber == 0)
                {
                    var maxStep = await db.TechnologicalSteps
                        .Where(s => s.CardId == cardId)
                        .MaxAsync(s => (int?)s.StepNumber) ?? 0;
                    step.StepNumber = maxStep + 1;
                }
                db.TechnologicalSteps.Add(step);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<TechnologicalStep> { Success = true, Data = step });
            }
        }

        // PUT api/technological-cards/5/steps/10
        [HttpPut, Route("{cardId:int}/steps/{stepId:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> UpdateStep(int cardId, int stepId, [FromBody] TechnologicalStep updated)
        {
            using (var db = new AppDbContext())
            {
                var step = await db.TechnologicalSteps
                    .FirstOrDefaultAsync(s => s.Id == stepId && s.CardId == cardId);
                if (step == null) return NotFound();
                step.StepType = updated.StepType;
                step.ParametersJson = updated.ParametersJson;
                step.IsMandatory = updated.IsMandatory;
                step.StepNumber = updated.StepNumber;
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<TechnologicalStep> { Success = true, Data = step });
            }
        }

        // PUT api/technological-cards/5/steps/order
        [HttpPut, Route("{cardId:int}/steps/order")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> ReorderSteps(int cardId, [FromBody] List<StepOrderDto> order)
        {
            using (var db = new AppDbContext())
            {
                var steps = await db.TechnologicalSteps.Where(s => s.CardId == cardId).ToListAsync();
                foreach (var step in steps)
                {
                    var newOrder = order.FirstOrDefault(o => o.StepId == step.Id);
                    if (newOrder != null)
                        step.StepNumber = newOrder.NewNumber;
                }
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }

        // DELETE api/technological-cards/5/steps/10
        [HttpDelete, Route("{cardId:int}/steps/{stepId:int}")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> RemoveStep(int cardId, int stepId)
        {
            using (var db = new AppDbContext())
            {
                var step = await db.TechnologicalSteps
                    .FirstOrDefaultAsync(s => s.Id == stepId && s.CardId == cardId);
                if (step == null) return NotFound();
                db.TechnologicalSteps.Remove(step);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }

        // PUT api/technological-cards/5/approve
        [HttpPut, Route("{id:int}/approve")]
        [Authorize(Roles = "Technologist")]
        public async Task<IHttpActionResult> Approve(int id)
        {
            using (var db = new AppDbContext())
            {
                var card = await db.TechnologicalCards.FindAsync(id);
                if (card == null) return NotFound();
                card.Status = "Active";
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<TechnologicalCard> { Success = true, Data = card });
            }
        }

        // PUT api/technological-cards/5/archive
        [HttpPut, Route("{id:int}/archive")]
        [Authorize(Roles = "Technologist,Admin")]
        public async Task<IHttpActionResult> Archive(int id)
        {
            using (var db = new AppDbContext())
            {
                var card = await db.TechnologicalCards.FindAsync(id);
                if (card == null) return NotFound();
                card.Status = "Archived";
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<TechnologicalCard> { Success = true, Data = card });
            }
        }
    }

    public class StepOrderDto
    {
        public int StepId { get; set; }
        public int NewNumber { get; set; }
    }
}