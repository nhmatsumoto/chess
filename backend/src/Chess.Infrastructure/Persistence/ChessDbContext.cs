using Chess.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        });

        modelBuilder.Entity<ChessPiece>(builder =>
        {
            builder.HasKey(p => p.Id);
        });
    }
}
