namespace BankMore.ContaCorrente.Api.Models;

public sealed record CadastrarContaRequest(string Cpf, string Senha);
public sealed record LoginRequest(string NumeroContaOuCpf, string Senha);
public sealed record InativarContaRequest(string Senha);
public sealed record CriarMovimentoRequest(string ChaveIdempotencia, string? NumeroContaCorrente, decimal Valor, string TipoMovimento);
