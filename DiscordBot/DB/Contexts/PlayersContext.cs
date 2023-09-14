using System;
using System.IO;
using DiscordBot.Casino;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.DB.Contexts;

public class PlayersContext: DbContext, IPlayersContext
{
    private string _dbPath { get; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Bet> Bets { get; set; }
    public new void SaveChanges()
    {
        base.SaveChanges();
    }
    public PlayersContext()
    {
        _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "database.db3");
        SQLitePCL.Batteries_V2.Init();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename={_dbPath}");
    }

    /*protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bet>().HasNoKey();
        base.OnModelCreating(modelBuilder);
    }*/
}