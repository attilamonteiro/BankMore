namespace BankMore.ContaCorrente.Domain.Entities;

public sealed class Movimento
{
    public Guid IdMovimento { get; set; }
    public Guid IdContaCorrente { get; set; }
    public DateTime DataMovimento { get; set; }
    public string TipoMovimento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
