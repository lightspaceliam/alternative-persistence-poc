using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Stub.Models;

namespace Stub;

public class SqLiteDbContext : DbContext
{
	public DbSet<Patient> Patients { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlite("Data Source=sqlite.db", options =>
		{
			options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
		});
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Patient>().ToTable(name: "Patients", schema:"dbo");
		modelBuilder.Entity<Patient>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.HasIndex(e => e.Given);
			entity.HasIndex(e => e.Family);
		});
	}
}