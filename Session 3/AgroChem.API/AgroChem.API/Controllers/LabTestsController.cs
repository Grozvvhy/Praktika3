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
    [RoutePrefix("api/lab")]
    public class LabTestsController : ApiController
    {
        // GET api/lab/pending-raw-materials - партии сырья, ожидающие анализа
        [HttpGet, Route("pending-raw-materials")]
        [Authorize(Roles = "Laboratory,Admin")]
        public async Task<IHttpActionResult> GetPendingRawMaterialBatches()
        {
            using (var db = new AppDbContext())
            {
                var batches = await db.ProductionBatches
                    .Include(b => b.Recipe.Product)
                    .Where(b => b.Status == "AwaitingRawMaterialTest")
                    .ToListAsync();
                return Ok(new ApiResponse<List<ProductionBatch>> { Success = true, Data = batches });
            }
        }

        // GET api/lab/pending-products - партии готовой продукции на контроле
        [HttpGet, Route("pending-products")]
        [Authorize(Roles = "Laboratory,Admin")]
        public async Task<IHttpActionResult> GetPendingProductBatches()
        {
            using (var db = new AppDbContext())
            {
                var batches = await db.ProductionBatches
                    .Include(b => b.Recipe.Product)
                    .Where(b => b.Status == "QualityControl")
                    .ToListAsync();
                return Ok(new ApiResponse<List<ProductionBatch>> { Success = true, Data = batches });
            }
        }

        // POST api/lab/assign - создать задание на испытание
        [HttpPost, Route("assign")]
        [Authorize(Roles = "Laboratory,Admin")]
        public async Task<IHttpActionResult> AssignTest([FromBody] AssignTestRequest request)
        {
            using (var db = new AppDbContext())
            {
                var batch = await db.ProductionBatches.FindAsync(request.BatchId);
                if (batch == null) return NotFound();
                var test = new LabTest
                {
                    BatchId = request.BatchId,
                    AssignedAt = DateTime.UtcNow,
                    TestType = request.TestType
                };
                db.LabTests.Add(test);
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<LabTest> { Success = true, Data = test });
            }
        }

        // PUT api/lab/tests/{testId}/results - ввести результаты испытания
        [HttpPut, Route("tests/{testId:int}/results")]
        [Authorize(Roles = "Laboratory,Admin")]
        public async Task<IHttpActionResult> EnterResults(int testId, [FromBody] List<LabTestParameterResult> results)
        {
            using (var db = new AppDbContext())
            {
                var test = await db.LabTests.FindAsync(testId);
                if (test == null) return NotFound();

                foreach (var res in results)
                {
                    res.TestId = testId;
                    db.LabTestParameterResults.Add(res);
                }
                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }

        // PUT api/lab/tests/{testId}/decision - принять решение по испытанию
        [HttpPut, Route("tests/{testId:int}/decision")]
        [Authorize(Roles = "Laboratory,Admin")]
        public async Task<IHttpActionResult> MakeDecision(int testId, [FromBody] DecisionRequest request)
        {
            using (var db = new AppDbContext())
            {
                var test = await db.LabTests.Include(t => t.Results).FirstOrDefaultAsync(t => t.Id == testId);
                if (test == null) return NotFound();

                // Устанавливаем решения по каждому параметру, если переданы
                if (request.ParameterDecisions != null)
                {
                    foreach (var pd in request.ParameterDecisions)
                    {
                        var param = test.Results.FirstOrDefault(p => p.Id == pd.ResultId);
                        if (param != null) param.Decision = pd.Decision;
                    }
                }

                // Меняем статус партии в зависимости от общего решения
                var batch = await db.ProductionBatches.FindAsync(test.BatchId);
                if (batch != null)
                {
                    if (request.OverallDecision == "Pass")
                        batch.Status = "Released";
                    else if (request.OverallDecision == "Fail")
                        batch.Status = "Blocked";
                }

                await db.SaveChangesAsync();
                return Ok(new ApiResponse<object> { Success = true });
            }
        }

        // GET api/lab/archive?page=1&pageSize=20
        [HttpGet, Route("archive")]
        [Authorize(Roles = "Laboratory,Admin")]
        public async Task<IHttpActionResult> GetArchive(int page = 1, int pageSize = 20)
        {
            using (var db = new AppDbContext())
            {
                var query = db.LabTests.Include(t => t.Batch.Recipe.Product);
                var total = await query.CountAsync();
                var tests = await query.OrderByDescending(t => t.AssignedAt)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = tests,
                    Pagination = new PaginationInfo { TotalCount = total, Page = page, PageSize = pageSize }
                });
            }
        }
    }

    public class AssignTestRequest
    {
        public int BatchId { get; set; }
        public string TestType { get; set; } // "RawMaterial" или "FinishedProduct"
    }

    public class DecisionRequest
    {
        public string OverallDecision { get; set; } // Pass или Fail
        public List<ParameterDecision> ParameterDecisions { get; set; }
    }

    public class ParameterDecision
    {
        public int ResultId { get; set; }
        public string Decision { get; set; }
    }
}