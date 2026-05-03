namespace BankMore.ContaCorrente.Api.Models;

public sealed record CadastrarContaResponse(string NumeroConta);
public sealed record LoginResponse(string Token, string NumeroConta);
public sealed record SaldoResponse(string NumeroConta, string NomeTitular, DateTime DataHoraConsulta, decimal Saldo);
