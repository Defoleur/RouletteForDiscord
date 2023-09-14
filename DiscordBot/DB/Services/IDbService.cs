using DiscordBot.DB.Repositories;

namespace DiscordBot.DB.Services;

public interface IDbService
{
    IPlayerRepository PlayerRepository { get; }
    IBetRepository BetRepository { get; }
    void SaveChanges();
}