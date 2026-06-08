using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers.v1;

[ApiController]
[Route("api/v1/accounts")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _accountService.GetAllAsync();
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
    {
        var account = await _accountService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountRequest request)
    {
        var account = await _accountService.UpdateAsync(id, request);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _accountService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
