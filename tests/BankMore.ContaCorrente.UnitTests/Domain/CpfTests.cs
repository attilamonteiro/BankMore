using BankMore.ContaCorrente.Domain.ValueObjects;
using FluentAssertions;

namespace BankMore.ContaCorrente.UnitTests.Domain;

public sealed class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    [InlineData("11144477735")]
    public void IsValid_ValidCpf_ReturnsTrue(string cpf)
        => Cpf.IsValid(cpf).Should().BeTrue();

    [Theory]
    [InlineData("000.000.000-00")]
    [InlineData("111.111.111-11")]
    [InlineData("123.456.789-00")]
    [InlineData("12345678900")]
    [InlineData("")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void IsValid_InvalidCpf_ReturnsFalse(string cpf)
        => Cpf.IsValid(cpf).Should().BeFalse();

    [Fact]
    public void Constructor_ValidCpf_StoresDigitsOnly()
    {
        var cpf = new Cpf("529.982.247-25");
        cpf.Value.Should().Be("52998224725");
    }

    [Fact]
    public void Constructor_InvalidCpf_ThrowsArgumentException()
    {
        var act = () => new Cpf("000.000.000-00");
        act.Should().Throw<ArgumentException>();
    }
}
