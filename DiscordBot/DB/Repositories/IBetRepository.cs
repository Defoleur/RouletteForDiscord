using DiscordBot.Casino;

namespace DiscordBot.DB.Repositories;

public interface IBetRepository
{
    IEnumerable<Bet> GetPlayersBets(Player player);
    void SaveBet(Bet bet);
}