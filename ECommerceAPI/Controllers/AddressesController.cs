using ECommerceAPI.DTO_s;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/users/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressRepository _addressRepo;

    public AddressesController(IAddressRepository addressRepo)
    {
        _addressRepo = addressRepo;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // GET /api/users/addresses
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var addresses = await _addressRepo.GetByUserIdAsync(GetUserId());
        return Ok(addresses);
    }

    // GET /api/users/addresses/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var address = await _addressRepo.GetByIdAsync(id, GetUserId());
        if (address == null) return NotFound();
        return Ok(address);
    }

    // POST /api/users/addresses
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var id = await _addressRepo.CreateAsync(GetUserId(), dto);
        return Ok(new { addressId = id, message = "Address created successfully" });
    }

    // PUT /api/users/addresses/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _addressRepo.UpdateAsync(id, GetUserId(), dto);
        if (!success) return NotFound();
        return Ok(new { message = "Address updated successfully" });
    }

    // DELETE /api/users/addresses/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _addressRepo.DeleteAsync(id, GetUserId());
        if (!success) return NotFound();
        return Ok(new { message = "Address deleted successfully" });
    }

    // PUT /api/users/addresses/{id}/set-default
    [HttpPut("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var success = await _addressRepo.SetDefaultAsync(id, GetUserId());
        if (!success) return NotFound();
        return Ok(new { message = "Default address updated" });
    }
}