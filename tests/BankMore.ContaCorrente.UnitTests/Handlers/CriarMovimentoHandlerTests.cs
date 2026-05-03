using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Idempotency;
using BankMore.Shared.Models;
using FluentAssertions;
using NSubstitute;
using ContaCorrenteEntity = global::BankMore.ContaCorrente.Domain.Entities.ContaCorrente;
using MovimentoEntity = global::BankMore.ContaCorrente.Domain.Entities.Movimento;

namespace BankMore.ContaCorrente.UnitTests.Handlers;

public sealed class CriarMovimentoHandlerTests
{
    private readonly IContaCorrenteRepository _contaRepo = Substitute.For<IContaCorrenteRepository>();
    private readonly IMovimentoRepository _movimentoRepo = Substitute.For<IMovimentoRepository>();
    private readonly IIdempotencyService _idempotency = Substitute.For<IIdempotencyService>();
    private readonly CriarMovimentoHandler _handler;

    private static readonly Guid AccountId = Guid.NewGuid();

    private readonly ContaCorrenteEntity _contaAtiva = new()
    {
        IdContaCorrente = AccountId,
        Numero = "12345678",
        Nome = "52998224725",
        Ativo = true,
        Senha = "hash",
        Salt = "salt"
    };

    public CriarMovimentoHandlerTests()
    {
        _idempotency.CheckAndClaimAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns((false, (string?)null));
        _handler = new CriarMovimentoHandler(_contaRepo, _movimentoRepo, _idempotency);
    }

    [Fact]
    public async Task Handle_ValidCreditToOwnAccount_ReturnsSuccess()
    {
        _contaRepo.GetByIdAsync(AccountId, Arg.Any<CancellationToken>()).Returns(_contaAtiva);

        var cmd = new CriarMovimentoCommand(AccountId, "key-1", null, 100m, "C");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _movimentoRepo.Received(1).AddAsync(Arg.Any<MovimentoEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DebitToThirdPartyAccount_ReturnsInvalidType()
    {
        var otherAccountId = Guid.NewGuid();
        var contaTerceiro = new ContaCorrenteEntity
        {
            IdContaCorrente = otherAccountId,
            Numero = "99887766",
            Nome = "11144477735",
            Ativo = true,
            Senha = "hash",
            Salt = "salt"
        };
        _contaRepo.GetByNumeroAsync("99887766", Arg.Any<CancellationToken>()).Returns(contaTerceiro);

        var cmd = new CriarMovimentoCommand(AccountId, "key-2", "99887766", 50m, "D");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Error!.TipoErro.Should().Be(ErrorTypes.InvalidType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_NonPositiveValue_ReturnsInvalidValue(decimal valor)
    {
        _contaRepo.GetByIdAsync(AccountId, Arg.Any<CancellationToken>()).Returns(_contaAtiva);

        var cmd = new CriarMovimentoCommand(AccountId, "key-3", null, valor, "C");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Error!.TipoErro.Should().Be(ErrorTypes.InvalidValue);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("c")]
    [InlineData("d")]
    [InlineData("CREDITO")]
    public async Task Handle_InvalidTipoMovimento_ReturnsInvalidType(string tipo)
    {
        _contaRepo.GetByIdAsync(AccountId, Arg.Any<CancellationToken>()).Returns(_contaAtiva);

        var cmd = new CriarMovimentoCommand(AccountId, "key-4", null, 100m, tipo);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Error!.TipoErro.Should().Be(ErrorTypes.InvalidType);
    }

    [Fact]
    public async Task Handle_InactiveAccount_ReturnsInactiveAccount()
    {
        var contaInativa = new ContaCorrenteEntity
        {
            IdContaCorrente = AccountId,
            Numero = "12345678",
            Nome = "52998224725",
            Ativo = false,
            Senha = "hash",
            Salt = "salt"
        };
        _contaRepo.GetByIdAsync(AccountId, Arg.Any<CancellationToken>()).Returns(contaInativa);

        var cmd = new CriarMovimentoCommand(AccountId, "key-5", null, 100m, "C");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Error!.TipoErro.Should().Be(ErrorTypes.InactiveAccount);
    }

    [Fact]
    public async Task Handle_DuplicateIdempotencyKey_ReturnsSuccessWithoutProcessing()
    {
        _idempotency.CheckAndClaimAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, "ok"));

        var cmd = new CriarMovimentoCommand(AccountId, "key-dup", null, 100m, "C");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _movimentoRepo.DidNotReceive().AddAsync(Arg.Any<MovimentoEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AccountNotFound_ReturnsInvalidAccount()
    {
        _contaRepo.GetByIdAsync(AccountId, Arg.Any<CancellationToken>()).Returns((ContaCorrenteEntity?)null);

        var cmd = new CriarMovimentoCommand(AccountId, "key-6", null, 100m, "C");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Error!.TipoErro.Should().Be(ErrorTypes.InvalidAccount);
    }
}
