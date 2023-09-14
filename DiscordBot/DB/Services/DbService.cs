using DiscordBot.DB.Contexts;
using DiscordBot.DB.Repositories;

namespace DiscordBot.DB.Services;

public class DbService: IDbService
{
    private readonly IPlayersContext _playersContext;

    public DbService(IPlayersContext playersContext)
    {
        _playersContext = playersContext;
        PlayerRepository = new PlayerRepository(playersContext);
        BetRepository = new BetRepository(playersContext);
    }

    public IPlayerRepository PlayerRepository { get; set; }
    public IBetRepository BetRepository { get; set; }

    public void SaveChanges()
    {
        _playersContext.SaveChanges();
    }
}