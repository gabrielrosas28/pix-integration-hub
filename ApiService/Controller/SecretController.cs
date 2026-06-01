using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecretController : ControllerBase
{
    private readonly ISecretService _secretService;

    public SecretController(ISecretService secretService)
    {
        _secretService = secretService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _secretService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var secret = await _secretService.GetByIdAsync(id);

        if (secret == null)
            return NotFound();

        return Ok(secret);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSecretRequest request)
    {
        var secret = await _secretService.CreateAsync(request);

        return Ok(secret);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSecretRequest request)
    {
        var secret = await _secretService.UpdateAsync(id, request);

        if (secret == null)
            return NotFound();

        return Ok(secret);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _secretService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
