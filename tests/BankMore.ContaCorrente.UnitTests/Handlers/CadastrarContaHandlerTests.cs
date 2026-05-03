using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Models;
using BankMore.Shared.Security;
using FluentAssertions;
using NSubstitute;
using ContaCorrenteEntity = global::BankMore.ContaCorrente.Domain.Entities.ContaCorrente;

namespace BankMore.ContaCorrente.UnitTests.Handlers;

public sealed class CadastrarContaHandlerTests
{
    private readonly IContaCorrenteRepository _repository = Substitute.For<IContaCorrenteRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly CadastrarContaHandler _handler;

    public CadastrarContaHandlerTests()
    {
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(("hash", "salt"));
        _repository.GetByNumeroAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((ContaCorrenteEntity?)null);
        _handler = new CadastrarContaHandler(_repository, _passwordHasher);
    }

    [Fact]
    public async Task Handle_ValidCpf_ReturnsSuccessWithNumeroConta()
    {
        var cmd = new CadastrarContaCommand("529.982.247-25", "senha123");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.NumeroConta.Should().NotBeNullOrEmpty();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InvalidCpf_ReturnsInvalidDocument()
    {
        var cmd = new CadastrarContaCommand("000.000.000-00", "senha123");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Error!.TipoErro.Should().Be(ErrorTypes.InvalidDocument);
        await _repository.DidNotReceive().AddAsync(Arg.Any<ContaCorrenteEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCpf_HashesPasswordBeforeSaving()
    {
        var cmd = new CadastrarContaCommand("52998224725", "minhasenha");

        await _handler.Handle(cmd, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword("minhasenha");
    }
}
