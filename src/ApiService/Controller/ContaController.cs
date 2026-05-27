using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaController : ControllerBase
{
    private readonly IContaService _contaService;

    public ContaController(IContaService contaService)
    {
        _contaService = contaService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_contaService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var conta = _contaService.GetByIdAsync(id);

        if (conta == null)
            return NotFound();

        return Ok(conta);
    }

    [HttpPost]
    public IActionResult Create(CreateContaRequest request)
    {
        var conta = _contaService.CreateAsync(request);

        return Ok(conta);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateContaRequest request)
    {
        var conta = _contaService.UpdateAsync(id, request);

        if (conta == null)
            return NotFound();

        return Ok(conta);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _contaService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}