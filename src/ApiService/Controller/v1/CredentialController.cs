using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/credentials")] // Alterado implicitamente para garantir a rota /api/credentials
public class CredentialController : ControllerBase // Alterado de SecretController para CredentialController
{
    private readonly ICredentialService _credentialService; // Alterado de ISecretService

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
        var credential = await _credentialService.GetByIdAsync(id); // Alterado nome da variável de escopo

        if (credential == null)
            return NotFound();

        return Ok(credential);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCredentialRequest request) // Alterado tipo do parâmetro
    {
        var credential = await _credentialService.CreateAsync(request);

        return Ok(credential);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCredentialRequest request) // Alterado tipo do parâmetro
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