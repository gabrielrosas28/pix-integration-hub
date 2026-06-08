using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers.v1;

[ApiController]
[Route("api/v1/credentials")]
public class CredentialController : ControllerBase
{
    private readonly ICredentialService _credentialService;

    public CredentialController(ICredentialService credentialService)
    {
        _credentialService = credentialService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _credentialService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var credential = await _credentialService.GetByIdAsync(id);

        if (credential == null)
            return NotFound();

        return Ok(credential);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCredentialRequest request)
    {
        var credential = await _credentialService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = credential.Id }, credential);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCredentialRequest request)
    {
        var credential = await _credentialService.UpdateAsync(id, request);

        if (credential == null)
            return NotFound();

        return Ok(credential);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _credentialService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
