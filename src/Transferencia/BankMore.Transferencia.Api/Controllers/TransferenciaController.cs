using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.Events;
using BankMore.Transferencia.Api.Models;
using BankMore.Shared.Events;
using BankMore.Shared.Models;
using System.Security.Claims;

namespace BankMore.Transferencia.Api.Controllers;

[ApiController]
[Route("api/transferencia")]
public sealed class TransferenciaController(IMediator mediator, ITransferenciaEventPublisher eventPublisher) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Transferir([FromBody] RealizarTransferenciaRequest request, CancellationToken ct)
    {
        var accountId = GetAccountId();
        if (accountId is null) return Forbid();

        var token = GetRawToken();

        var result = await mediator.Send(new RealizarTransferenciaCommand(
            accountId.Value,
            request.ChaveIdempotencia,
            request.NumeroContaOrigem,
            request.NumeroContaDestino,
            request.Valor,
            token), ct);

        if (!result.Success) return BadRequest(new ErrorResponse(result.Error!, "TRANSFER_FAILED"));

        await eventPublisher.PublishAsync(new TransferenciaRealizadaEvent(
            Guid.NewGuid(),
            request.NumeroContaOrigem,
            request.NumeroContaDestino,
            request.Valor,
            DateTime.UtcNow), ct);

        return NoContent();
    }

    private Guid? GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private string GetRawToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : string.Empty;
    }
}
