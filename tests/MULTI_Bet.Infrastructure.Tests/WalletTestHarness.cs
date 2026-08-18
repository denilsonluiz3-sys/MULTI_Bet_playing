using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MULTI_Bet.Infrastructure.Wallet;

namespace MULTI_Bet.Infrastructure.Tests;

public sealed class WalletTestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    public WalletDbContext Db { get; }

    public WalletService Service { get; }

    public WalletTestHarness()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new WalletDbContext(options);
        Db.Database.Migrate();

        Service = new WalletService(new WalletRepository(Db));
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}