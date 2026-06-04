// File: Data/Db.cs
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CrmAdmin.Web.Data;

public interface IDbFactory
{
    IDbConnection AppDb();
    IDbConnection MaximizerDb(); // in cloud dev: same as AppDb (MockMaximizerUsers lives there)
}

public sealed class DbFactory : IDbFactory
{
    private readonly IConfiguration _cfg;
    public DbFactory(IConfiguration cfg) => _cfg = cfg;

    public IDbConnection AppDb()
    {
        var cs = _cfg.GetConnectionString("AppDb")
                 ?? throw new InvalidOperationException("Missing AppDb connection string");
        return new SqlConnection(cs);
    }

    public IDbConnection MaximizerDb()
    {
        var cs = _cfg.GetConnectionString("MaximizerDb")
                 ?? throw new InvalidOperationException("Missing MaximizerDb connection string");
        return new SqlConnection(cs);
    }
}
