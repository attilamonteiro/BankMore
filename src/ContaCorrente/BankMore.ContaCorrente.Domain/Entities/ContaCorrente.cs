namespace BankMore.ContaCorrente.Domain.Entities;

public sealed class ContaCorrente
{
    public Guid IdContaCorrente { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string Senha { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
}
