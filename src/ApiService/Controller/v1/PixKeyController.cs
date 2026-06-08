using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers.v1;

[ApiController]
[Route("api/v1/pix-keys")]
public class PixKeyController : ControllerBase
{
    private readonly IPixKeyService _pixKeyService;

    public PixKeyController(IPixKeyService pixKeyService)
    {
        _pixKeyService = pixKeyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _pixKeyService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pixKey = await _pixKeyService.GetByIdAsync(id);
        if (pixKey == null) return NotFound();
        return Ok(pixKey);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePixKeyRequest request)
    {
        var pixKey = await _pixKeyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = pixKey.Id }, pixKey);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePixKeyRequest request)
    {
        var pixKey = await _pixKeyService.UpdateAsync(id, request);
        if (pixKey == null) return NotFound();
        return Ok(pixKey);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _pixKeyService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
