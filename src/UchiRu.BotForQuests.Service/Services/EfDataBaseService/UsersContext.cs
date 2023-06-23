using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UchiRu.BotForQuests.Service.Services.EfDataBaseService;

public sealed class UsersContext : DbContext {
    
    public DbSet<User>? User { get; set; }
    
    private string _connectionString = String.Empty; 
    
    public UsersContext(IConfiguration configuration) {
        _connectionString = String.Format("Host={0};Port={1};Database={2};Username={3};Password={4}",
            configuration.GetSection("DataBase:Host").Value,
            configuration.GetSection("DataBase:Port").Value, 
            configuration.GetSection("DataBase:Database").Value, 
            configuration.GetSection("DataBase:Username").Value,
            configuration.GetSection("DataBase:Password").Value);
        
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql(_connectionString);
    }
}

public class User {
    [Key] public int Id { get; set; }

    public long UserId { get; set; }

    public int Level { get; set; }
}