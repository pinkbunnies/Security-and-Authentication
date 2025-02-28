using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // Actualizado para usar Microsoft.Data.SqlClient
using SafeVault.Helpers;
using SafeVault.Models;

namespace SafeVault.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString = "YourConnectionStringHere";

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            string allowedSpecialCharacters = "!@#$%^&*?";

            // Validar entradas utilizando helpers
            if (!ValidationHelpers.IsValidInput(model.Username) ||
                !ValidationHelpers.IsValidInput(model.Password, allowedSpecialCharacters))
            {
                return BadRequest("Entradas inválidas.");
            }

            // Ejemplo de consulta parametrizada para prevenir SQL Injection
            string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", model.Username);
                    command.Parameters.AddWithValue("@Password", model.Password); // En producción, comparar contraseñas hasheadas

                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    if (count > 0)
                    {
                        // Aquí se podría emitir un token JWT
                        return Ok("Login exitoso.");
                    }
                    else
                    {
                        return Unauthorized("Credenciales incorrectas.");
                    }
                }
            }
        }
    }
}
