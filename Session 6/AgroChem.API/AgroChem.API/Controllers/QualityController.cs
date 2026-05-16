using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/quality")]
    public class QualityController : ApiController
    {
        private readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AgroChemDB"].ConnectionString;

        public class TestParameterDto
        {
            public string ParameterName { get; set; }
            public string MeasuredValue { get; set; }
            public string StandardValue { get; set; }
            public string Unit { get; set; }
            public string Result { get; set; }
        }

        public class SaveTestResultRequest
        {
            public int? BatchId { get; set; }
            public int? RawMaterialBatchId { get; set; }
            public string SampleType { get; set; }
            public List<TestParameterDto> Parameters { get; set; }
            public string Decision { get; set; }
            public string AnalystComment { get; set; }
            public string AnalystName { get; set; }   // добавляем имя лаборанта
        }

        // GET: api/quality/batches?type=raw
        [HttpGet]
        [Route("batches")]
        public IHttpActionResult GetBatchesForQC(string type)
        {
            try
            {
                if (type == "raw")
                {
                    string query = @"
                        SELECT rmb.id, rmb.batch_number, rm.name AS material_name, rmb.quantity_kg, rmb.arrival_date,
                               CASE WHEN qc.id IS NULL THEN 'pending' ELSE 'completed' END AS qc_status
                        FROM raw_material_batches rmb
                        JOIN raw_materials rm ON rmb.raw_material_id = rm.id
                        LEFT JOIN quality_controls qc ON qc.raw_material_batch_id = rmb.id AND qc.sample_type = 'raw_material'
                        WHERE rmb.status = 'available'
                        ORDER BY rmb.arrival_date DESC";

                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(query, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            var result = new List<object>();
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    Id = reader["id"],
                                    BatchNumber = reader["batch_number"],
                                    MaterialName = reader["material_name"],
                                    QuantityKg = reader["quantity_kg"],
                                    ArrivalDate = reader["arrival_date"],
                                    QcStatus = reader["qc_status"]
                                });
                            }
                            return Ok(result);
                        }
                    }
                }
                else if (type == "final")
                {
                    string query = @"
                        SELECT b.id, b.batch_number, p.name AS product_name, b.actual_quantity_kg, b.start_time, b.status,
                               CASE WHEN qc.id IS NULL THEN 'pending' ELSE 'completed' END AS qc_status
                        FROM batches b
                        JOIN production_orders po ON b.order_id = po.id
                        JOIN products p ON po.product_id = p.id
                        LEFT JOIN quality_controls qc ON qc.batch_id = b.id AND qc.sample_type = 'final_product'
                        WHERE b.status IN ('running', 'completed')
                        ORDER BY b.start_time DESC";

                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(query, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            var result = new List<object>();
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    Id = reader["id"],
                                    BatchNumber = reader["batch_number"],
                                    ProductName = reader["product_name"],
                                    ActualQuantityKg = reader["actual_quantity_kg"],
                                    StartTime = reader["start_time"],
                                    Status = reader["status"],
                                    QcStatus = reader["qc_status"]
                                });
                            }
                            return Ok(result);
                        }
                    }
                }
                else return BadRequest("Invalid type parameter. Use 'raw' or 'final'.");
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }

        // GET: api/quality/standards?productId=1   or rawMaterialId=1
        [HttpGet]
        [Route("standards")]
        public IHttpActionResult GetStandards(int? productId = null, int? rawMaterialId = null)
        {
            try
            {
                if (productId.HasValue)
                {
                    string query = "SELECT standard_params FROM products WHERE id = @id";
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", productId.Value);
                            var json = cmd.ExecuteScalar() as string;
                            if (string.IsNullOrEmpty(json)) return Ok(new List<object>());
                            var standards = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            var result = new List<object>();
                            foreach (var kv in standards)
                                result.Add(new { ParameterName = kv.Key, StandardValue = kv.Value, Unit = "" });
                            return Ok(result);
                        }
                    }
                }
                else if (rawMaterialId.HasValue)
                {
                    string query = "SELECT standard_params FROM raw_materials WHERE id = @id";
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", rawMaterialId.Value);
                            var json = cmd.ExecuteScalar() as string;
                            if (string.IsNullOrEmpty(json)) return Ok(new List<object>());
                            var standards = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            var result = new List<object>();
                            foreach (var kv in standards)
                                result.Add(new { ParameterName = kv.Key, StandardValue = kv.Value, Unit = "" });
                            return Ok(result);
                        }
                    }
                }
                else return BadRequest("Specify either productId or rawMaterialId");
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }

        // POST: api/quality/save
        [HttpPost]
        [Route("save")]
        // GET: api/quality/history?batchId=123
        [HttpGet]
        [Route("history")]
        public IHttpActionResult GetTestHistory(int? batchId = null, int? rawMaterialBatchId = null)
        {
            try
            {
                if (!batchId.HasValue && !rawMaterialBatchId.HasValue)
                    return BadRequest("Укажите batchId или rawMaterialBatchId");

                string query = @"
            SELECT id, analysis_date, sample_type, parameter_name, measured_value, standard_value, unit, 
                   result, decision, analyst_comment, analyst_name, test_results_json, 
                   CASE WHEN batch_id IS NOT NULL THEN batch_id ELSE raw_material_batch_id END AS entity_id
            FROM quality_controls
            WHERE (batch_id = @batchId OR raw_material_batch_id = @rawMaterialBatchId)
            ORDER BY analysis_date DESC";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId.HasValue ? (object)batchId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@rawMaterialBatchId", rawMaterialBatchId.HasValue ? (object)rawMaterialBatchId.Value : DBNull.Value);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var list = new List<object>();
                            while (reader.Read())
                            {
                                list.Add(new
                                {
                                    Id = reader["id"],
                                    AnalysisDate = reader["analysis_date"],
                                    SampleType = reader["sample_type"],
                                    ParameterName = reader["parameter_name"],
                                    MeasuredValue = reader["measured_value"],
                                    StandardValue = reader["standard_value"],
                                    Unit = reader["unit"],
                                    Result = reader["result"],
                                    Decision = reader["decision"],
                                    AnalystComment = reader["analyst_comment"],
                                    AnalystName = reader["analyst_name"],
                                    TestResultsJson = reader["test_results_json"]
                                });
                            }
                            return Ok(list);
                        }
                    }
                }
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }

        // PUT: api/quality/update/{id}
        [HttpPut]
        [Route("update/{id}")]
        public IHttpActionResult UpdateTestResult(int id, [FromBody] SaveTestResultRequest request)
        {
            try
            {
                // Получаем существующую запись, чтобы знать batch_id/raw_material_batch_id
                string selectQuery = "SELECT batch_id, raw_material_batch_id, sample_type FROM quality_controls WHERE id = @id";
                int? batchId = null, rawMaterialBatchId = null;
                string sampleType = "";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                batchId = reader["batch_id"] as int?;
                                rawMaterialBatchId = reader["raw_material_batch_id"] as int?;
                                sampleType = reader["sample_type"].ToString();
                            }
                            else return NotFound();
                        }
                    }
                }

                // Удаляем старые записи для этой партии/сырья
                string deleteQuery = sampleType == "final_product"
                    ? "DELETE FROM quality_controls WHERE batch_id = @id AND sample_type = @sample_type"
                    : "DELETE FROM quality_controls WHERE raw_material_batch_id = @id AND sample_type = @sample_type";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var delCmd = new SqlCommand(deleteQuery, conn, trans))
                            {
                                delCmd.Parameters.AddWithValue("@id", batchId ?? rawMaterialBatchId);
                                delCmd.Parameters.AddWithValue("@sample_type", sampleType);
                                delCmd.ExecuteNonQuery();
                            }

                            string insertQuery = @"
                        INSERT INTO quality_controls 
                        (batch_id, raw_material_batch_id, analysis_date, sample_type, parameter_name, 
                         measured_value, standard_value, unit, result, decision, analyst_comment, analyst_name, test_results_json)
                        VALUES 
                        (@batch_id, @raw_material_batch_id, @analysis_date, @sample_type, @parameter_name,
                         @measured_value, @standard_value, @unit, @result, @decision, @analyst_comment, @analyst_name, @test_results_json)";

                            string allParamsJson = JsonConvert.SerializeObject(request.Parameters);
                            foreach (var param in request.Parameters)
                            {
                                using (var insCmd = new SqlCommand(insertQuery, conn, trans))
                                {
                                    insCmd.Parameters.AddWithValue("@batch_id", batchId ?? (object)DBNull.Value);
                                    insCmd.Parameters.AddWithValue("@raw_material_batch_id", rawMaterialBatchId ?? (object)DBNull.Value);
                                    insCmd.Parameters.AddWithValue("@analysis_date", DateTime.Now);
                                    insCmd.Parameters.AddWithValue("@sample_type", sampleType);
                                    insCmd.Parameters.AddWithValue("@parameter_name", param.ParameterName);
                                    insCmd.Parameters.AddWithValue("@measured_value", param.MeasuredValue);
                                    insCmd.Parameters.AddWithValue("@standard_value", param.StandardValue);
                                    insCmd.Parameters.AddWithValue("@unit", param.Unit ?? "");
                                    insCmd.Parameters.AddWithValue("@result", param.Result);
                                    insCmd.Parameters.AddWithValue("@decision", request.Decision);
                                    insCmd.Parameters.AddWithValue("@analyst_comment", request.AnalystComment ?? "");
                                    insCmd.Parameters.AddWithValue("@analyst_name", request.AnalystName ?? "");
                                    insCmd.Parameters.AddWithValue("@test_results_json", allParamsJson);
                                    insCmd.ExecuteNonQuery();
                                }
                            }

                            // Аудит
                            string auditQuery = @"INSERT INTO audit_log (user_id, action_type, entity_type, entity_id, new_value, timestamp)
                                          VALUES (NULL, 'UPDATE_QUALITY', @entity_type, @entity_id, @new_value, GETDATE())";
                            using (var auditCmd = new SqlCommand(auditQuery, conn, trans))
                            {
                                auditCmd.Parameters.AddWithValue("@entity_type", sampleType);
                                auditCmd.Parameters.AddWithValue("@entity_id", batchId ?? rawMaterialBatchId);
                                auditCmd.Parameters.AddWithValue("@new_value", JsonConvert.SerializeObject(request));
                                auditCmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                        catch { trans.Rollback(); throw; }
                    }
                }
                return Ok(new { success = true, message = "Обновлено" });
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }
        public IHttpActionResult SaveTestResults(SaveTestResultRequest request)
        {
            try
            {
                if (request == null || request.Parameters == null)
                    return BadRequest("Invalid data");

                if (request.SampleType != "raw_material" && request.SampleType != "final_product")
                    return BadRequest("Invalid sample_type");

                int? targetId = null;
                if (request.SampleType == "final_product" && request.BatchId.HasValue)
                    targetId = request.BatchId.Value;
                else if (request.SampleType == "raw_material" && request.RawMaterialBatchId.HasValue)
                    targetId = request.RawMaterialBatchId.Value;
                else
                    return BadRequest("Missing batch identifier");

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Удаляем старые результаты по этой партии
                            string deleteQuery = (request.SampleType == "final_product")
                                ? "DELETE FROM quality_controls WHERE batch_id = @id AND sample_type = @sample_type"
                                : "DELETE FROM quality_controls WHERE raw_material_batch_id = @id AND sample_type = @sample_type";
                            using (var delCmd = new SqlCommand(deleteQuery, conn, transaction))
                            {
                                delCmd.Parameters.AddWithValue("@id", targetId.Value);
                                delCmd.Parameters.AddWithValue("@sample_type", request.SampleType);
                                delCmd.ExecuteNonQuery();
                            }

                            // Сохраняем JSON всех параметров для истории
                            string allParamsJson = JsonConvert.SerializeObject(request.Parameters);

                            // Вставляем новые параметры
                            string insertQuery = @"
                                INSERT INTO quality_controls 
                                (batch_id, raw_material_batch_id, analysis_date, sample_type, parameter_name, 
                                 measured_value, standard_value, unit, result, decision, analyst_comment, analyst_name, test_results_json)
                                VALUES 
                                (@batch_id, @raw_material_batch_id, @analysis_date, @sample_type, @parameter_name,
                                 @measured_value, @standard_value, @unit, @result, @decision, @analyst_comment, @analyst_name, @test_results_json)";

                            foreach (var param in request.Parameters)
                            {
                                using (var insCmd = new SqlCommand(insertQuery, conn, transaction))
                                {
                                    insCmd.Parameters.AddWithValue("@batch_id", (request.SampleType == "final_product") ? (object)targetId.Value : DBNull.Value);
                                    insCmd.Parameters.AddWithValue("@raw_material_batch_id", (request.SampleType == "raw_material") ? (object)targetId.Value : DBNull.Value);
                                    insCmd.Parameters.AddWithValue("@analysis_date", DateTime.Now);
                                    insCmd.Parameters.AddWithValue("@sample_type", request.SampleType);
                                    insCmd.Parameters.AddWithValue("@parameter_name", param.ParameterName);
                                    insCmd.Parameters.AddWithValue("@measured_value", param.MeasuredValue);
                                    insCmd.Parameters.AddWithValue("@standard_value", param.StandardValue);
                                    insCmd.Parameters.AddWithValue("@unit", param.Unit ?? "");
                                    insCmd.Parameters.AddWithValue("@result", param.Result);
                                    insCmd.Parameters.AddWithValue("@decision", request.Decision);
                                    insCmd.Parameters.AddWithValue("@analyst_comment", request.AnalystComment ?? "");
                                    insCmd.Parameters.AddWithValue("@analyst_name", request.AnalystName ?? "");
                                    insCmd.Parameters.AddWithValue("@test_results_json", allParamsJson);
                                    insCmd.ExecuteNonQuery();
                                }
                            }

                            // Если решение blocked и это готовая продукция, блокируем партию
                            if (request.SampleType == "final_product" && request.Decision == "blocked")
                            {
                                string blockQuery = "UPDATE batches SET status = 'blocked', end_time = GETDATE() WHERE id = @batch_id";
                                using (var blockCmd = new SqlCommand(blockQuery, conn, transaction))
                                {
                                    blockCmd.Parameters.AddWithValue("@batch_id", targetId.Value);
                                    blockCmd.ExecuteNonQuery();
                                }
                            }

                            // Аудит
                            string auditQuery = @"
                                INSERT INTO audit_log (user_id, action_type, entity_type, entity_id, new_value, timestamp)
                                VALUES (NULL, 'SAVE_QUALITY', @entity_type, @entity_id, @new_value, GETDATE())";
                            using (var auditCmd = new SqlCommand(auditQuery, conn, transaction))
                            {
                                auditCmd.Parameters.AddWithValue("@entity_type", request.SampleType);
                                auditCmd.Parameters.AddWithValue("@entity_id", targetId.Value);
                                auditCmd.Parameters.AddWithValue("@new_value", JsonConvert.SerializeObject(request));
                                auditCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch { transaction.Rollback(); throw; }
                    }
                }
                return Ok(new { success = true, message = "Test results saved" });
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }
    }
}