using Microsoft.EntityFrameworkCore;
using MULTI_Bet.Infrastructure.Wallet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=multibet.db";
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<WalletService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();