namespace BankMore.Shared.Models;

public sealed record ErrorResponse(string Mensagem, string TipoErro);

public static class ErrorTypes
{
    public const string InvalidDocument = "INVALID_DOCUMENT";
    public const string UserUnauthorized = "USER_UNAUTHORIZED";
    public const string InvalidAccount = "INVALID_ACCOUNT";
    public const string InactiveAccount = "INACTIVE_ACCOUNT";
    public const string InvalidValue = "INVALID_VALUE";
    public const string InvalidType = "INVALID_TYPE";
}
