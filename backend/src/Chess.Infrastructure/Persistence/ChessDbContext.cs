using Chess.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;

namespace Chess.Infrastructure.Persistence;

public class ChessDbContext : DbContext
{
    public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options) { }

    public DbSet<ChessGame> Games => Set<ChessGame>();
    public DbSet<ChessPiece> Pieces => Set<ChessPiece>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChessGame>(builder =>
        {
            builder.HasKey(g => g.Id);
            builder.HasMany<ChessPiece>().WithOne().HasForeignKey(p => p.GameId);
            builder.Ignore(g => g.Board);
            builder.Ignore(g => g.EnPassantSquare);
            
            // Auto-convert List<string> to JSON for storage
            builder.Property(g => g.MoveHistory)
                   .HasColumnType("jsonb")
                   .HasConversion(
                       v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                       v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());
        });

        modelBuilder.Entity<ChessPiece>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Type).HasConversion<string>();
            builder.Property(p => p.Color).HasConversion<string>();
        });
    }
}
