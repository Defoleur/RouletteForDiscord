using Microsoft.EntityFrameworkCore;
using DiscordBot.Casino;

namespace DiscordBot.DB.Contexts;

public interface IPlayersContext
{
    DbSet<Player> Players { get; set; }
    DbSet<Bet> Bets { get; set; }
    void SaveChanges();
}