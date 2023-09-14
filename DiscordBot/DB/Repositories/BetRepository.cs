using System.Collections.Generic;
using System.Linq;
using DiscordBot.Casino;
using DiscordBot.DB.Contexts;

namespace DiscordBot.DB.Repositories;

public class BetRepository: IBetRepository
{
    private readonly IPlayersContext _playersContext;

    public BetRepository(IPlayersContext playersContext)
    {
        _playersContext = playersContext;
    }
    
    public IEnumerable<Bet> GetPlayersBets(Player player)
    {
        return _playersContext.Bets.Where(bet => bet.Player == player);
    }

    public void SaveBet(Bet bet)
    {
        _playersContext.Bets.Add(bet);
    }
}