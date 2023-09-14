using System.Collections;
using DiscordBot.Casino;
using DiscordBot.DB.Contexts;

namespace DiscordBot.DB.Repositories;

public class PlayerRepository: IPlayerRepository
{
    private readonly IPlayersContext _playersContext;

    public PlayerRepository(IPlayersContext playersContext)
    {
        _playersContext = playersContext;
    }
    public IEnumerable<Player> GetPlayers()
    {
        return _playersContext.Players.ToList();
    }

    public void SavePlayer(Player player)
    {
        _playersContext.Players.Add(player);
    }

    public void UpdatePlayer(Player player)
    {
        _playersContext.Players.Find(player);
    }
}