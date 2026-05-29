using ECommerceAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // ✅ Anyone can view products (no auth needed)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProducts()
        {
            return Ok(new { message = "Product list - public endpoint" });
        }

        // ✅ Only users with product.create right can access
        [HttpPost]
        [HasRight("product.create")]
        public IActionResult CreateProduct()
        {
            return Ok(new { message = "Product created - you have product.create right!" });
        }

        // ✅ Only users with product.edit right can access
        [HttpPut("{id}")]
        [HasRight("product.edit")]
        public IActionResult UpdateProduct(int id)
        {
            return Ok(new { message = $"Product {id} updated - you have product.edit right!" });
        }

        // ✅ Only users with product.delete right can access
        [HttpDelete("{id}")]
        [HasRight("product.delete")]
        public IActionResult DeleteProduct(int id)
        {
            return Ok(new { message = $"Product {id} deleted - you have product.delete right!" });
        }
    }
}