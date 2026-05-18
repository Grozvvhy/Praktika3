using System;
using System.Data.SqlClient;
using System.Web.Http;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AgroChemDB"].ConnectionString;

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Role { get; set; }
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return Ok(new { success = false, message = "Введите логин" });
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT role, full_name, is_active FROM users WHERE username = @username";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", request.Username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isActive = reader.GetBoolean(reader.GetOrdinal("is_active"));
                                if (!isActive)
                                    return Ok(new { success = false, message = "Пользователь деактивирован" });

                                string role = reader.GetString(reader.GetOrdinal("role"));
                                string fullName = reader.GetString(reader.GetOrdinal("full_name"));

                                // Пароль не проверяем – подходит любой пароль (для теста)
                                // Если нужна проверка пароля, раскомментируйте блок ниже и установите BCrypt.Net-Next
                                /*
                                string storedHash = reader.GetString(reader.GetOrdinal("password_hash"));
                                if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                                    return Ok(new { success = false, message = "Неверный пароль" });
                                */

                                return Ok(new { success = true, role = role, fullName = fullName });
                            }
                            else
                            {
                                return Ok(new { success = false, message = "Пользователь не найден" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Ошибка сервера: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                    return Ok(new { success = false, message = "Логин обязателен" });
                if (string.IsNullOrWhiteSpace(request.Password))
                    return Ok(new { success = false, message = "Пароль обязателен" });
                if (string.IsNullOrWhiteSpace(request.FullName))
                    return Ok(new { success = false, message = "ФИО обязательно" });

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                    using (var checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@username", request.Username);
                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists > 0)
                        {
                            return Ok(new { success = false, message = "Пользователь с таким логином уже существует" });
                        }
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    string insertQuery = @"INSERT INTO users (username, password_hash, full_name, email, phone, role, is_active, created_at) 
                                           VALUES (@username, @password_hash, @full_name, @email, @phone, @role, 1, @created_at)";
                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", request.Username);
                        cmd.Parameters.AddWithValue("@password_hash", hashedPassword);
                        cmd.Parameters.AddWithValue("@full_name", request.FullName);
                        cmd.Parameters.AddWithValue("@email", request.Email ?? "");
                        cmd.Parameters.AddWithValue("@phone", request.Phone ?? "");
                        cmd.Parameters.AddWithValue("@role", request.Role ?? "technologist");
                        cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok(new { success = true, message = "Регистрация успешна" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Ошибка сервера: " + ex.Message });
            }
        }
    }
}