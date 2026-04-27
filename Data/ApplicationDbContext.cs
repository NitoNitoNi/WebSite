using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models; 

namespace MyMvcApp.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser> 
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Accounts> Accounts { get; set; }
    public DbSet<Deparment> Deparment { get; set; }
    public DbSet<Resources> Resources { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Deparment)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DeparmentId);
        
        
    }
}
