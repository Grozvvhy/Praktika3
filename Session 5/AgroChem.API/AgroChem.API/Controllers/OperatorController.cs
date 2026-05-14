using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/operator")]
    public class OperatorController : ApiController
    {
        private readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AgroChemDB"].ConnectionString;

        [HttpGet]
        [Route("active-batches")]
        public IHttpActionResult GetActiveBatches()
        {
            var list = new List<object>();
            string query = @"
                SELECT b.id AS BatchId, b.batch_number AS BatchNumber, p.name AS ProductName,
                       (SELECT TOP 1 step_name FROM process_steps ps WHERE ps.recipe_id = b.recipe_id ORDER BY step_order) AS CurrentStepName,
                       'Линия 1' AS EquipmentLine, b.status AS Status
                FROM batches b
                JOIN production_orders po ON b.order_id = po.id
                JOIN products p ON po.product_id = p.id
                WHERE b.status = 'running'";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            BatchId = reader["BatchId"],
                            BatchNumber = reader["BatchNumber"],
                            ProductName = reader["ProductName"],
                            CurrentStepName = reader["CurrentStepName"],
                            EquipmentLine = reader["EquipmentLine"],
                            Status = reader["Status"]
                        });
                    }
                }
            }
            return Ok(list);
        }

        [HttpGet]
        [Route("batch/{batchId}/program")]
        public IHttpActionResult GetBatchProgram(int batchId)
        {
            var steps = new List<object>();
            // Получаем рецепт партии
            int recipeId = 0;
            string recipeQuery = "SELECT recipe_id FROM batches WHERE id = @id";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(recipeQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", batchId);
                    recipeId = (int)cmd.ExecuteScalar();
                }

                // Получаем шаги техкарты
                string stepsQuery = @"
                    SELECT step_order, step_name, planned_temp_c, planned_pressure_bar, planned_duration_min
                    FROM process_steps WHERE recipe_id = @recipeId ORDER BY step_order";
                using (var cmd = new SqlCommand(stepsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@recipeId", recipeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int stepOrder = (int)reader["step_order"];
                            string stepName = reader["step_name"].ToString();
                            decimal? plannedTemp = reader["planned_temp_c"] == DBNull.Value ? (decimal?)null : (decimal)reader["planned_temp_c"];
                            decimal? plannedPressure = reader["planned_pressure_bar"] == DBNull.Value ? (decimal?)null : (decimal)reader["planned_pressure_bar"];
                            int? plannedDuration = reader["planned_duration_min"] == DBNull.Value ? (int?)null : (int)reader["planned_duration_min"];

                            // Статус шага из таблицы batch_steps
                            string stepStatus = "not_started";
                            decimal? actualTemp = null;
                            decimal? actualPressure = null;
                            int? actualDuration = null;
                            string operatorComment = "";

                            string statusQuery = @"
                                SELECT actual_start_time, actual_end_time, actual_temp_c, actual_pressure_bar, actual_duration_min, operator_comment
                                FROM batch_steps WHERE batch_id = @batchId AND process_step_id = (SELECT id FROM process_steps WHERE recipe_id = @recipeId AND step_order = @stepOrder)";
                            using (var statusCmd = new SqlCommand(statusQuery, conn))
                            {
                                statusCmd.Parameters.AddWithValue("@batchId", batchId);
                                statusCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                statusCmd.Parameters.AddWithValue("@stepOrder", stepOrder);
                                using (var statusReader = statusCmd.ExecuteReader())
                                {
                                    if (statusReader.Read())
                                    {
                                        if (statusReader["actual_start_time"] != DBNull.Value && statusReader["actual_end_time"] == DBNull.Value)
                                            stepStatus = "in_progress";
                                        else if (statusReader["actual_end_time"] != DBNull.Value)
                                            stepStatus = "completed";
                                        actualTemp = statusReader["actual_temp_c"] == DBNull.Value ? (decimal?)null : (decimal)statusReader["actual_temp_c"];
                                        actualPressure = statusReader["actual_pressure_bar"] == DBNull.Value ? (decimal?)null : (decimal)statusReader["actual_pressure_bar"];
                                        actualDuration = statusReader["actual_duration_min"] == DBNull.Value ? (int?)null : (int)statusReader["actual_duration_min"];
                                        operatorComment = statusReader["operator_comment"]?.ToString() ?? "";
                                    }
                                }
                            }

                            steps.Add(new
                            {
                                StepOrder = stepOrder,
                                StepName = stepName,
                                Instruction = "Выполните операцию согласно регламенту",
                                PlannedTempC = plannedTemp,
                                PlannedPressureBar = plannedPressure,
                                PlannedDurationMin = plannedDuration,
                                Status = stepStatus,
                                ActualTempC = actualTemp,
                                ActualPressureBar = actualPressure,
                                ActualDurationMin = actualDuration,
                                OperatorComment = operatorComment
                            });
                        }
                    }
                }
            }
            return Ok(steps);
        }

        [HttpPost]
        [Route("batch/{batchId}/step/{stepOrder}/start")]
        public IHttpActionResult StartStep(int batchId, int stepOrder)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Получить process_step_id
                    int recipeId = 0;
                    string recipeQuery = "SELECT recipe_id FROM batches WHERE id = @id";
                    using (var cmd = new SqlCommand(recipeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", batchId);
                        recipeId = (int)cmd.ExecuteScalar();
                    }
                    int processStepId = 0;
                    string stepIdQuery = "SELECT id FROM process_steps WHERE recipe_id = @recipeId AND step_order = @stepOrder";
                    using (var cmd = new SqlCommand(stepIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@recipeId", recipeId);
                        cmd.Parameters.AddWithValue("@stepOrder", stepOrder);
                        processStepId = (int)cmd.ExecuteScalar();
                    }

                    string insert = @"
                        INSERT INTO batch_steps (batch_id, process_step_id, actual_start_time)
                        VALUES (@batchId, @processStepId, GETDATE())";
                    using (var cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        cmd.Parameters.AddWithValue("@processStepId", processStepId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("batch/{batchId}/step/{stepOrder}/complete")]
        public IHttpActionResult CompleteStep(int batchId, int stepOrder, [FromBody] dynamic data)
        {
            try
            {
                decimal? actualTemp = data.actualTemp != null ? (decimal?)data.actualTemp : null;
                decimal? actualPressure = data.actualPressure != null ? (decimal?)data.actualPressure : null;
                int? actualDuration = data.actualDuration != null ? (int?)data.actualDuration : null;
                string comment = data.comment ?? "";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    int recipeId = 0;
                    string recipeQuery = "SELECT recipe_id FROM batches WHERE id = @id";
                    using (var cmd = new SqlCommand(recipeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", batchId);
                        recipeId = (int)cmd.ExecuteScalar();
                    }
                    int processStepId = 0;
                    string stepIdQuery = "SELECT id FROM process_steps WHERE recipe_id = @recipeId AND step_order = @stepOrder";
                    using (var cmd = new SqlCommand(stepIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@recipeId", recipeId);
                        cmd.Parameters.AddWithValue("@stepOrder", stepOrder);
                        processStepId = (int)cmd.ExecuteScalar();
                    }

                    string update = @"
                        UPDATE batch_steps
                        SET actual_end_time = GETDATE(),
                            actual_temp_c = @temp,
                            actual_pressure_bar = @pressure,
                            actual_duration_min = @duration,
                            operator_comment = @comment
                        WHERE batch_id = @batchId AND process_step_id = @processStepId AND actual_end_time IS NULL";
                    using (var cmd = new SqlCommand(update, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        cmd.Parameters.AddWithValue("@processStepId", processStepId);
                        cmd.Parameters.AddWithValue("@temp", actualTemp ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@pressure", actualPressure ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@duration", actualDuration ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@comment", comment);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}