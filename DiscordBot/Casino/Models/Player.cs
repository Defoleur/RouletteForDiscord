using Discord;

namespace DiscordBot.Casino;

public class Player
{
    public Guid Id { get; set; }
    public ulong UserId { get; set; }
    public int Balance { get; set; }
    public int Profit { get; set; }
    public DateTime LastReceivedBonusTime { get; set; }

    public void Win(int bet, int coef)
    {
        Balance += bet * coef;
        Profit += bet * (coef - 1);
    }

    public void MakeBet(int bet, out int trueBet)
    {
        if (Balance - bet < 0)
        {
            trueBet = Balance;
            Balance = 0;
        }
        else
        {
            Balance -= bet;
            trueBet = bet;
        }
        
    }

    public double GetStack()
    {
        return Balance;
    }

    public void GetDailyBonus()
    {
        Balance += 500;
    }
    
}