using System.Data;

namespace BankMore.Shared.Database;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
