using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UchiRu.BotForQuests.Service.Services.EfDataBaseService;

public sealed class UsersContext : DbContext {
    public DbSet<User> User { get; set; }

    private readonly string _connectionString;
    private readonly ILogger<UsersContext> _logger;

    public UsersContext(IConfiguration configuration, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<UsersContext>();
        var dataBaseOptions = configuration.GetSection(DataBaseOptions.SectionName).Get<DataBaseOptions>();

        if (dataBaseOptions == null) {
            throw new ArgumentNullException(nameof(dataBaseOptions));
        }
        
        _connectionString = String.Format(dataBaseOptions.GetConnectionString());
        try {
            Database.EnsureCreated();
        }
        catch (Exception e) {
            _logger.LogError(e.Message);
            throw;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql(_connectionString);
    }
}

public class DataBaseOptions {
    public static readonly string SectionName = "DataBase";
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string GetConnectionString() {
        return String.Format("Host={0};Port={1};Database={2};Username={3};Password={4};",
            Host, Port, Database, Username, Password);
    }
}

public class User {
    [Key] public int Id { get; set; }
    public long UserId { get; set; }
    public int Level { get; set; }
}