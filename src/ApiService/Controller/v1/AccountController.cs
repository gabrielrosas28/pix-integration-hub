using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaController : ControllerBase
{
    private readonly IAccountService _contaService;

    public ContaController(IAccountService contaService)
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
    public IActionResult Create(CreateAccountRequest request)
    {
        var conta = _contaService.CreateAsync(request);

        return Ok(conta);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateAccountRequest request)
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
