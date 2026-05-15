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
                       'Экструдер Линия 1' AS EquipmentLine, b.status AS Status
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

                string stepsQuery = @"
                    SELECT step_order, step_name, planned_temp_c, planned_pressure_bar, planned_duration_min, instruction
                    FROM process_steps WHERE recipe_id = @recipeId ORDER BY step_order";
                var stepList = new List<(int stepOrder, string stepName, decimal? plannedTemp, decimal? plannedPressure, int? plannedDuration, string instruction)>();
                using (var cmd = new SqlCommand(stepsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@recipeId", recipeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stepList.Add((
                                stepOrder: (int)reader["step_order"],
                                stepName: reader["step_name"].ToString(),
                                plannedTemp: reader["planned_temp_c"] == DBNull.Value ? (decimal?)null : (decimal)reader["planned_temp_c"],
                                plannedPressure: reader["planned_pressure_bar"] == DBNull.Value ? (decimal?)null : (decimal)reader["planned_pressure_bar"],
                                plannedDuration: reader["planned_duration_min"] == DBNull.Value ? (int?)null : (int)reader["planned_duration_min"],
                                instruction: reader["instruction"]?.ToString() ?? "Выполните операцию согласно регламенту"
                            ));
                        }
                    }
                }

                foreach (var step in stepList)
                {
                    string stepStatus = "not_started";
                    decimal? actualTemp = null;
                    decimal? actualPressure = null;
                    int? actualDuration = null;
                    string operatorComment = "";

                    int processStepId = 0;
                    string psQuery = "SELECT id FROM process_steps WHERE recipe_id = @recipeId AND step_order = @stepOrder";
                    using (var cmd = new SqlCommand(psQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@recipeId", recipeId);
                        cmd.Parameters.AddWithValue("@stepOrder", step.stepOrder);
                        processStepId = (int)cmd.ExecuteScalar();
                    }

                    string statusQuery = @"
                        SELECT actual_start_time, actual_end_time, actual_temp_c, actual_pressure_bar, actual_duration_min, operator_comment
                        FROM batch_steps 
                        WHERE batch_id = @batchId AND process_step_id = @processStepId";
                    using (var cmd = new SqlCommand(statusQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        cmd.Parameters.AddWithValue("@processStepId", processStepId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["actual_start_time"] != DBNull.Value && reader["actual_end_time"] == DBNull.Value)
                                    stepStatus = "in_progress";
                                else if (reader["actual_end_time"] != DBNull.Value)
                                    stepStatus = "completed";

                                actualTemp = reader["actual_temp_c"] == DBNull.Value ? (decimal?)null : (decimal)reader["actual_temp_c"];
                                actualPressure = reader["actual_pressure_bar"] == DBNull.Value ? (decimal?)null : (decimal)reader["actual_pressure_bar"];
                                actualDuration = reader["actual_duration_min"] == DBNull.Value ? (int?)null : (int)reader["actual_duration_min"];
                                operatorComment = reader["operator_comment"]?.ToString() ?? "";
                            }
                        }
                    }

                    steps.Add(new
                    {
                        StepOrder = step.stepOrder,
                        StepName = step.stepName,
                        Instruction = step.instruction,
                        PlannedTempC = step.plannedTemp,
                        PlannedPressureBar = step.plannedPressure,
                        PlannedDurationMin = step.plannedDuration,
                        Status = stepStatus,
                        ActualTempC = actualTemp,
                        ActualPressureBar = actualPressure,
                        ActualDurationMin = actualDuration,
                        OperatorComment = operatorComment
                    });
                }
            }
            return Ok(steps);
        }

        [HttpGet]
        [Route("telemetry/{equipmentName}")]
        public IHttpActionResult GetTelemetry(string equipmentName)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT temperature_c, pressure_bar, screw_speed_rpm, last_update FROM equipment_telemetry WHERE equipment_name = @name";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", equipmentName);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    Temperature = reader["temperature_c"] == DBNull.Value ? (decimal?)null : (decimal)reader["temperature_c"],
                                    Pressure = reader["pressure_bar"] == DBNull.Value ? (decimal?)null : (decimal)reader["pressure_bar"],
                                    ScrewSpeed = reader["screw_speed_rpm"] == DBNull.Value ? (int?)null : (int)reader["screw_speed_rpm"],
                                    LastUpdate = reader["last_update"]
                                });
                            }
                        }
                    }
                }
                return Ok(new { Temperature = (decimal?)null, Pressure = (decimal?)null, ScrewSpeed = (int?)null });
            }
            catch
            {
                return Ok(new { Temperature = (decimal?)null, Pressure = (decimal?)null, ScrewSpeed = (int?)null });
            }
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

                    string checkQuery = "SELECT COUNT(*) FROM batch_steps WHERE batch_id = @batchId AND process_step_id = @processStepId";
                    using (var cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        cmd.Parameters.AddWithValue("@processStepId", processStepId);
                        int exists = (int)cmd.ExecuteScalar();
                        if (exists == 0)
                        {
                            string insert = @"
                                INSERT INTO batch_steps (batch_id, process_step_id, actual_start_time)
                                VALUES (@batchId, @processStepId, GETDATE())";
                            using (var insertCmd = new SqlCommand(insert, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@batchId", batchId);
                                insertCmd.Parameters.AddWithValue("@processStepId", processStepId);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
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

                    // Получить плановые значения для проверки отклонений
                    decimal? plannedTemp = null, plannedPressure = null;
                    int? plannedDuration = null;
                    string planQuery = "SELECT planned_temp_c, planned_pressure_bar, planned_duration_min FROM process_steps WHERE id = @id";
                    using (var cmd = new SqlCommand(planQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", processStepId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                plannedTemp = reader["planned_temp_c"] == DBNull.Value ? (decimal?)null : (decimal)reader["planned_temp_c"];
                                plannedPressure = reader["planned_pressure_bar"] == DBNull.Value ? (decimal?)null : (decimal)reader["planned_pressure_bar"];
                                plannedDuration = reader["planned_duration_min"] == DBNull.Value ? (int?)null : (int)reader["planned_duration_min"];
                            }
                        }
                    }

                    // Обновить шаг
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

                    // Логирование отклонений
                    if (actualTemp.HasValue && plannedTemp.HasValue && Math.Abs(actualTemp.Value - plannedTemp.Value) > 2.0m)
                        InsertDeviationEvent(conn, batchId, processStepId, "temperature_deviation", $"Температура {actualTemp.Value}°C (план {plannedTemp.Value}°C)", "medium");
                    if (actualPressure.HasValue && plannedPressure.HasValue && Math.Abs(actualPressure.Value - plannedPressure.Value) > 0.5m)
                        InsertDeviationEvent(conn, batchId, processStepId, "pressure_deviation", $"Давление {actualPressure.Value} бар (план {plannedPressure.Value} бар)", "medium");
                    if (actualDuration.HasValue && plannedDuration.HasValue && Math.Abs(actualDuration.Value - plannedDuration.Value) > 10)
                        InsertDeviationEvent(conn, batchId, processStepId, "duration_deviation", $"Длительность {actualDuration.Value} мин (план {plannedDuration.Value} мин)", "low");

                    // Проверка, все ли шаги завершены
                    int totalSteps = 0, completedSteps = 0;
                    string countQuery = "SELECT COUNT(*) FROM process_steps WHERE recipe_id = @recipeId";
                    using (var cmd = new SqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@recipeId", recipeId);
                        totalSteps = (int)cmd.ExecuteScalar();
                    }
                    string completedQuery = @"
                        SELECT COUNT(*) FROM batch_steps bs
                        JOIN process_steps ps ON bs.process_step_id = ps.id
                        WHERE bs.batch_id = @batchId AND ps.recipe_id = @recipeId AND bs.actual_end_time IS NOT NULL";
                    using (var cmd = new SqlCommand(completedQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        cmd.Parameters.AddWithValue("@recipeId", recipeId);
                        completedSteps = (int)cmd.ExecuteScalar();
                    }

                    if (totalSteps == completedSteps)
                    {
                        string completeBatch = "UPDATE batches SET status = 'completed', end_time = GETDATE() WHERE id = @id";
                        using (var cmd = new SqlCommand(completeBatch, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", batchId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private void InsertDeviationEvent(SqlConnection conn, int batchId, int batchStepId, string eventType, string description, string severity)
        {
            string insert = @"
                INSERT INTO deviation_events (batch_id, batch_step_id, event_time, event_type, description, severity)
                VALUES (@batchId, @batchStepId, GETDATE(), @eventType, @description, @severity)";
            using (var cmd = new SqlCommand(insert, conn))
            {
                cmd.Parameters.AddWithValue("@batchId", batchId);
                cmd.Parameters.AddWithValue("@batchStepId", batchStepId);
                cmd.Parameters.AddWithValue("@eventType", eventType);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@severity", severity);
                cmd.ExecuteNonQuery();
            }
        }
    }
}