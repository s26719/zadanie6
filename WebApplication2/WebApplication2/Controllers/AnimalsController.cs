using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Cryptography;
using WebApplication2;
using Microsoft.Extensions.Configuration;

namespace AnimalsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalsController : ControllerBase
    {
        private readonly string connectionString;

        public AnimalsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetAnimals(string orderBy = "name")
        {
          var query = $"SELECT IdAnimal, Name, Description, Category, Area FROM Animal ORDER BY {orderBy}";


            var animals = new List<Animal>();
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        animals.Add(new Animal
                        {
                            IdAnimal = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Category = reader.GetString(3),
                            Area = reader.GetString(4)
                        });
                    }
                }
            }

            return Ok(animals);
        }

        [HttpPost]
        public IActionResult AddAnimal(Animal animal)
        {
            if (animal == null || string.IsNullOrEmpty(animal.Name) || string.IsNullOrEmpty(animal.Category) ||
                string.IsNullOrEmpty(animal.Area))
            {
                return BadRequest("Invalid animal data! ");
            }

            string query =
                "INSERT INTO Animal (Name, Description, Category, Area) VALUES (@Name, @Description, @Category, @Area)";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", animal.Name);
                command.Parameters.AddWithValue("@Description", animal.Description);
                command.Parameters.AddWithValue("@Category", animal.Category);
                command.Parameters.AddWithValue("@Area", animal.Area);
                connection.Open();
                int result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    return StatusCode(201, "Animal added successfully.");
                }
                else
                {
                    return StatusCode(500, "Failed to add animal.");
                }
            }

        }

        [HttpPut("{idAnimal}")]
        public IActionResult ModifyAnimal(int idAnimal, Animal animal)
        {
            if (animal == null || string.IsNullOrEmpty(animal.Name) || string.IsNullOrEmpty(animal.Category) ||
                string.IsNullOrEmpty(animal.Area))
            {
                return BadRequest("Invalid animal data! ");
            }


            string query =  "UPDATE Animal SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IdAnimal", idAnimal);
                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Description", animal.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Category", animal.Category);
                command.Parameters.AddWithValue("@Area", animal.Area);
                connection.Open();
                int result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    return Ok("Animal uppdated Successfully.");
                }
                else
                {
                    return NotFound("Animal not found.");
                }
            }
        }

        [HttpDelete("{idAnimal}")]
        public IActionResult DeleteAnimal(int idAnimal)
        {

            string query = "Delete from Animal where IdAnimal = @IdAnimal";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IdAnimal", idAnimal);
                connection.Open();
                int result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    return Ok("Animal deleted Successfully.");
                }
                else
                {
                    return NotFound("Animal not found.");
                }
            }
        }
    }
}