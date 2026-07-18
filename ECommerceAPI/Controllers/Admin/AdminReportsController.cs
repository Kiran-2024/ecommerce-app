using ECommerceAPI.Authorization;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize]
    public class AdminReportsController : ControllerBase
    {
        private readonly AdminReportsRepository _reportsRepo;

        public AdminReportsController(AdminReportsRepository reportsRepo)
        {
            _reportsRepo = reportsRepo;
        }

        // GET /api/admin/reports/sales?startDate=2026-06-01&endDate=2026-07-01
        [HttpGet("sales")]
        [HasRight("reports.view")]
        public IActionResult GetSalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = _reportsRepo.GetSalesReport(startDate, endDate);
            return Ok(result);
        }

        // GET /api/admin/reports/revenue-by-category?startDate=2026-06-01&endDate=2026-07-01
        [HttpGet("revenue-by-category")]
        [HasRight("reports.view")]
        public IActionResult GetRevenueByCategory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = _reportsRepo.GetRevenueByCategory(startDate, endDate);
            return Ok(result);
        }

        // GET /api/admin/reports/order-status?startDate=2026-06-01&endDate=2026-07-01
        [HttpGet("order-status")]
        [HasRight("reports.view")]
        public IActionResult GetOrderStatusSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = _reportsRepo.GetOrderStatusSummary(startDate, endDate);
            return Ok(result);
        }
    }
}