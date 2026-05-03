using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Queries;
using BankMore.ContaCorrente.Api.Models;
using BankMore.Shared.Models;
using System.Security.Claims;

namespace BankMore.ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/contacorrente")]
public sealed class ContaCorrenteController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CadastrarContaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarContaRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CadastrarContaCommand(request.Cpf, request.Senha), ct);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(new CadastrarContaResponse(result.NumeroConta!));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.NumeroContaOuCpf, request.Senha), ct);
        if (!result.Success) return Unauthorized(result.Error);
        return Ok(new LoginResponse(result.Token!, result.NumeroConta!));
    }

    [HttpPatch("inativar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Inativar([FromBody] InativarContaRequest request, CancellationToken ct)
    {
        var accountId = GetAccountId();
        if (accountId is null) return Forbid();

        var result = await mediator.Send(new InativarContaCommand(accountId.Value, request.Senha), ct);
        if (!result.Success) return BadRequest(result.Error);
        return NoContent();
    }

    [HttpPost("movimento")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Movimento([FromBody] CriarMovimentoRequest request, CancellationToken ct)
    {
        var accountId = GetAccountId();
        if (accountId is null) return Forbid();

        var result = await mediator.Send(new CriarMovimentoCommand(
            accountId.Value,
            request.ChaveIdempotencia,
            request.NumeroContaCorrente,
            request.Valor,
            request.TipoMovimento), ct);

        if (!result.Success) return BadRequest(result.Error);
        return NoContent();
    }

    [HttpGet("saldo")]
    [Authorize]
    [ProducesResponseType(typeof(SaldoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Saldo(CancellationToken ct)
    {
        var accountId = GetAccountId();
        if (accountId is null) return Forbid();

        var result = await mediator.Send(new ConsultarSaldoQuery(accountId.Value), ct);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(new SaldoResponse(result.NumeroConta!, result.NomeTitular!, result.DataHoraConsulta!.Value, result.Saldo!.Value));
    }

    private Guid? GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
