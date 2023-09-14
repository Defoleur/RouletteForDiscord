using DiscordBot.Casino;

namespace DiscordBot.DB.Repositories;

public interface IPlayerRepository
{
    IEnumerable<Player> GetPlayers();
    void SavePlayer(Player player);
    void UpdatePlayer(Player player);
}