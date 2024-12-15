using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace kursah_5semestr;

public partial class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     => optionsBuilder.UseNpgsql(_configuration.GetConnectionString("AppDbContext"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Transactions_pkey");

            entity.HasOne(e => e.User).WithMany()
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("Transactions_UserId_fkey");

            entity.HasOne(e => e.Order).WithMany()
                .HasForeignKey(e => e.OrderId)
                .HasConstraintName("Transactions_OrderId_fkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Login).HasMaxLength(128);
            entity.Property(e => e.PasswordHash).HasMaxLength(128);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Orders_pkey");

            entity.HasOne(e => e.User).WithMany()
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("Orders_UserId_fkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
