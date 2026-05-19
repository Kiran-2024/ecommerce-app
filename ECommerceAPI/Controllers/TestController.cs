using ECommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public TestController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpGet("connection")]
        public IActionResult TestConnection()
        {
            try
            {
                using var conn = _dbHelper.GetConnection();
                conn.Open();
                return Ok("✅ Database Connected Successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ Connection Failed: {ex.Message}");
            }
        }
    }
}
