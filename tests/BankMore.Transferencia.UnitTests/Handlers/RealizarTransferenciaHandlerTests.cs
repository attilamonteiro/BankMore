using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.Handlers;
using BankMore.Transferencia.Application.HttpClients;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Shared.Idempotency;
using FluentAssertions;
using NSubstitute;
using TransferenciaEntity = global::BankMore.Transferencia.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.UnitTests.Handlers;

public sealed class RealizarTransferenciaHandlerTests
{
    private readonly IIdempotencyService _idempotency = Substitute.For<IIdempotencyService>();
    private readonly IContaCorrenteHttpClient _httpClient = Substitute.For<IContaCorrenteHttpClient>();
    private readonly ITransferenciaRepository _repository = Substitute.For<ITransferenciaRepository>();
    private readonly RealizarTransferenciaHandler _handler;

    private static readonly Guid AccountId = Guid.NewGuid();
    private const string Token = "test-token";

    public RealizarTransferenciaHandlerTests()
    {
        _idempotency.CheckAndClaimAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns((false, (string?)null));
        _handler = new RealizarTransferenciaHandler(_idempotency, _httpClient, _repository);
    }

    [Fact]
    public async Task Handle_SuccessfulTransfer_ReturnsSuccessAndSavesRecord()
    {
        _httpClient.MovimentoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var cmd = BuildCommand();
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _repository.Received(1).AddAsync(Arg.Is<TransferenciaEntity>(t => t.Status == "Concluida"));
        await _idempotency.Received(1).SaveResultAsync(cmd.ChaveIdempotencia, "concluida");
    }

    [Fact]
    public async Task Handle_DebitFails_ReturnsFailureWithoutCredit()
    {
        _httpClient.MovimentoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), "D", Arg.Any<CancellationToken>())
            .Returns(false);

        var cmd = BuildCommand();
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        await _httpClient.DidNotReceive().MovimentoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), "C", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreditFails_PerformsRollbackAndSavesRevertedRecord()
    {
        _httpClient.MovimentoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), "D", Arg.Any<CancellationToken>())
            .Returns(true);
        _httpClient.MovimentoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), "C", Arg.Any<CancellationToken>())
            .Returns(false);

        var cmd = BuildCommand();
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        await _repository.Received(1).AddAsync(Arg.Is<TransferenciaEntity>(t => t.Status == "Revertida"));
        // Rollback credit should be called
        await _httpClient.Received().MovimentoAsync(Token, $"{cmd.ChaveIdempotencia}-rollback", cmd.NumeroContaOrigem, cmd.Valor, "C", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateKey_ReturnsSuccessImmediately()
    {
        _idempotency.CheckAndClaimAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, "concluida"));

        var cmd = BuildCommand();
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _httpClient.DidNotReceive().MovimentoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static RealizarTransferenciaCommand BuildCommand()
        => new(AccountId, "idp-key-1", "12345678", "87654321", 200m, Token);
}
