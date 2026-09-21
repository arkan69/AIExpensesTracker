using ExpenseApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(category => new { category.Name, category.Type })
                .IsUnique();

            entity.Property(category => category.Name)
                .HasMaxLength(100);

            entity.Property(category => category.Type)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(transaction => transaction.Amount)
                .HasPrecision(18, 2);

            entity.Property(transaction => transaction.TransactionType)
                .HasMaxLength(50);

            entity.Property(transaction => transaction.Description)
                .HasMaxLength(500);

            entity.HasOne(transaction => transaction.Category)
                .WithMany()
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(transaction => transaction.User)
                .WithMany(user => user.Transactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.Property(budget => budget.LimitAmount)
                .HasPrecision(18, 2);

            entity.HasIndex(budget => new { budget.UserId, budget.CategoryId, budget.Month, budget.Year })
                .IsUnique();

            entity.HasOne(budget => budget.Category)
                .WithMany()
                .HasForeignKey(budget => budget.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(budget => budget.User)
                .WithMany(user => user.Budgets)
                .HasForeignKey(budget => budget.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.Name)
                .HasMaxLength(100);

            entity.Property(user => user.Email)
                .HasMaxLength(150);

            entity.Property(user => user.Role)
                .HasMaxLength(20);
        });
    }
}
