using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models.DataModel;
using SQLitePCL;

namespace MyMvcApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Accounts> Accounts { get; set; }
    public DbSet<Deparment> Deparment { get; set; }
    public DbSet<Resources> Resources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Deparment)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DeparmentId);
        
        
    }
}
