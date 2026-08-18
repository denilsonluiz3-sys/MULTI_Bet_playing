using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MULTI_Bet.Infrastructure.Wallet;

public sealed class WalletDbContextFactory : IDesignTimeDbContextFactory<WalletDbContext>
{
    public WalletDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseSqlite("Data Source=multibet.db")
            .Options;
        return new WalletDbContext(options);
    }
}