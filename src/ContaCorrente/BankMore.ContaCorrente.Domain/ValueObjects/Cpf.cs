namespace BankMore.ContaCorrente.Domain.ValueObjects;

public readonly record struct Cpf
{
    public string Value { get; }

    public Cpf(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (!IsValid(digits))
            throw new ArgumentException("CPF inválido", nameof(value));
        Value = digits;
    }

    public static bool IsValid(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 11) return false;
        if (digits.Distinct().Count() == 1) return false;

        var d = digits.Select(c => c - '0').ToArray();

        var sum = 0;
        for (var i = 0; i < 9; i++) sum += d[i] * (10 - i);
        var remainder = sum % 11;
        var first = remainder < 2 ? 0 : 11 - remainder;
        if (d[9] != first) return false;

        sum = 0;
        for (var i = 0; i < 10; i++) sum += d[i] * (11 - i);
        remainder = sum % 11;
        var second = remainder < 2 ? 0 : 11 - remainder;
        return d[10] == second;
    }

    public override string ToString() => Value;
}
