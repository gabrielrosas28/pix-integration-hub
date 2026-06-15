using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChavePixController : ControllerBase
{
    private readonly IChavePixService _chavePixService;

    public ChavePixController(IChavePixService chavePixService)
    {
        _chavePixService = chavePixService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _chavePixService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var chavePix = await _chavePixService.GetByIdAsync(id);
        if (chavePix == null) return NotFound();
        return Ok(chavePix);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePixKeyRequest request)
    {
        var chavePix = await _chavePixService.CreateAsync(request);
        return Ok(chavePix);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePixKeyRequest request)
    {
        var chavePix = await _chavePixService.UpdateAsync(id, request);
        if (chavePix == null) return NotFound();
        return Ok(chavePix);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _chavePixService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
