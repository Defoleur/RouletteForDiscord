using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DB.Services;
using Color = Discord.Color;

namespace DiscordBot.Casino;

public class CasinoGame
{
    private readonly IDbService _dbService;
    private readonly ITextChannel _textChannel;
    private Dictionary<Player, List<Bet>> Bets { get; set; } = new Dictionary<Player, List<Bet>>();
    private List<Player> Players { get; set; }
    private readonly DiscordSocketClient _client;
    private readonly IUser _owner;
    private const int MinBet = 50;

    public CasinoGame(DiscordSocketClient client, ITextChannel textChannel, IUser owner, IDbService dbService)
    {
        _client = client;
        _textChannel = textChannel;
        _owner = owner;
        _dbService = dbService;
        Players = _dbService.PlayerRepository.GetPlayers().ToList();
        foreach (var pl in Players)
        {
            Bets.Add(pl, new List<Bet>());
        }
        _client.MessageReceived += OnMessageReceived;
    }

    private async Task OnMessageReceived(SocketMessage arg)
    {
        int argPos = 0;
        if (arg is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;
        if (!(message.HasCharPrefix('!', ref argPos) || 
              message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;
        Player currentPlayer = Players.FirstOrDefault(player => player.UserId == arg.Author.Id);

        switch (arg.Content)
        {
            case "!connect":
                if (currentPlayer == null) await AddPlayer(arg);
                return;
            
            case "!top":
                await GetTopProfitList();
                return;
        }
        
        if (await CheckIfConnected(arg, currentPlayer))
        {
            switch (arg.Content)
            {
                case "!roll":
                    if (arg.Author == _owner)
                    {
                        await SpinRoulette();
                    }
                    return;
            
                case "!balance":
                    await CheckBalance(currentPlayer);
                    return;
                
                case "!stop":
                    await StopGame();
                    return;
                
                case "!help":
                    await GetHelp();
                    return;
                
                case "!board":
                    await GetBoard();
                    return;
                
                case "!bets":
                    await GetPlayerBets(currentPlayer);
                    return;
            
                case "!daily":
                    await GetDailyBonus(currentPlayer);
                    return;
            }
        }

        var request = arg.Content.Split(' ');
        var command = request[0][1..];
        if(!int.TryParse(request.Last(), out var sum)) return;
        var numbers = GetNumbers(request);
        await arg.DeleteAsync();
        if (await CheckIfConnected(arg, currentPlayer))
        {
            switch (command.ToLower())
            {
                case "straight":
                    if (numbers.Count == 1)
                    {
                        await MakeBet(currentPlayer, numbers, sum, BetType.StraightUp);
                    }
                    break;
                
                case "red":
                    await MakeBet(currentPlayer, NumberCombinations.RedNumbers, sum, BetType.Red);
                    break;
                
                case "black":
                    await MakeBet(currentPlayer, NumberCombinations.BlackNumbers, sum, BetType.Black);
                    break;
                
                case "even":
                    await MakeBet(currentPlayer, NumberCombinations.EvenNumbers, sum, BetType.Even);
                    break;
                
                case "odd":
                    await MakeBet(currentPlayer, NumberCombinations.OddNumbers, sum, BetType.Odd);
                    break;
                
                case "dozen":
                    if (numbers.Count == 1)
                    {
                        await MakeBet(currentPlayer, GetDozenNumbers(numbers[0]), sum, BetType.Dozen);
                    }
                    break;
                
                case "column":
                    if (numbers.Count == 1)
                    {
                        await MakeBet(currentPlayer, GetColumnNumbers(numbers[0]), sum, BetType.Column);
                    }
                    break;
                
                case "small":
                    await MakeBet(currentPlayer, NumberCombinations.SmallNumbers, sum, BetType.SmallNumbers);
                    break;
                
                case "big":
                    await MakeBet(currentPlayer, NumberCombinations.BigNumbers, sum, BetType.BigNumbers);
                    break;
                
                case "firstfour":
                    await MakeBet(currentPlayer, NumberCombinations.FirstFourNumbers, sum, BetType.FirstFour);
                    break;
                
                case "split":
                    if (numbers.Count == 2 && !numbers.Contains(0))
                    {
                        if ((numbers[1] - numbers[0] == 1 && !NumberCombinations.ThirdColumn.Contains(numbers[0])) || numbers[1] - numbers[0] == 3)
                        {
                            await MakeBet(currentPlayer, numbers, sum, BetType.Split);
                        }
                    }
                    break;
                
                case "street":
                    await MakeBet(currentPlayer, GetStreetNumbers(numbers), sum, BetType.Street);
                    break;
                
                case "streetzero":
                    await MakeBet(currentPlayer, new List<int>() { 0, 2, numbers[0] }, sum, BetType.StreetAZero);
                    break;
                
                case "sixline":
                    await MakeBet(currentPlayer, GetSixlineNumbers(numbers), sum, BetType.Sixline);
                    break;
                
                case "corner":
                    await MakeBet(currentPlayer, GetCornerNumbers(numbers), sum, BetType.Corner);
                    break;
                
                default:
                    await _textChannel.SendMessageAsync($"There is no such command as {arg.Content}");
                    break;
            }
        }
    }

    private async Task GetDailyBonus(Player currentPlayer)
    {
        if (CheckDailyBonusAvailable(currentPlayer))
        {
            currentPlayer.GetDailyBonus();
            _dbService.SaveChanges();
            await _textChannel.SendMessageAsync("You have successfully gained your daily bonus!");
            return;
        }

        var day = currentPlayer.LastReceivedBonusTime.AddDays(1);
        await _textChannel.SendMessageAsync($"You can gain your daily bonus from {day}!");
    }

    private bool CheckDailyBonusAvailable(Player currentPlayer)
    {
        var currentTime = DateTime.Now;
        if (currentPlayer.LastReceivedBonusTime == DateTime.MinValue || (currentPlayer.LastReceivedBonusTime - currentTime).Days > 0)
        {
            currentPlayer.LastReceivedBonusTime = currentTime;
            _dbService.SaveChanges();
            return true;
        }

        return false;
    }

    private async Task StopGame()
    {
        await _textChannel.SendMessageAsync($"Game is stopped!");
        _client.MessageReceived -= OnMessageReceived;
    }

    private List<int> GetCornerNumbers(List<int> numbers)
    {
        if (!NumberCombinations.ThirdColumn.Contains(numbers[0]) && numbers[0] is <= 33 and not 0)
        {
            var corner = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) continue;
                corner.Add(numbers[0] + i);
            }

            return corner;
        }

        return new List<int>();
    }

    private List<int> GetSixlineNumbers(List<int> numbers)
    {
        if (NumberCombinations.FirstColumn.Contains(numbers[0]) && numbers[0] != 34)
        {
            var sixline = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                sixline.Add(numbers[0] + i);
            }

            return sixline;
        }

        return new List<int>();
    }

    private List<int> GetStreetNumbers(List<int> numbers)
    {
        return numbers[0] switch
        {
            var n when NumberCombinations.FirstColumn.Contains(n) => new List<int>() { n, n + 1, n + 2 },
            var n when NumberCombinations.SecondColumn.Contains(n) => new List<int>() { n - 1, n, n + 1 },
            var n when NumberCombinations.ThirdColumn.Contains(n) => new List<int>() { n - 2, n - 1, n },
            _ => new List<int>()
        };
    }

    private List<int> GetDozenNumbers(int dozen)
    {
        return dozen switch
        {
            1 => NumberCombinations.FirstDozen,
            2 => NumberCombinations.SecondDozen,
            3 => NumberCombinations.ThirdDozen,
            _ => new List<int>()
        };
    }

    private List<int> GetColumnNumbers(int column)
    {
        return column switch
        {
            1 => NumberCombinations.FirstColumn,
            2 => NumberCombinations.SecondColumn,
            3 => NumberCombinations.ThirdColumn,
            _ => new List<int>()
        };
    }
    
    private async Task CheckBalance(Player player)
    {
        await _textChannel.SendMessageAsync(
            $"The balance of player {GetUserById(player).Username} is {player.GetStack()} dollars.");
    }

    private List<int> GetNumbers(string[] request)
    {
        var numbers = new List<int>();

        for (int i = 0; i < request.Length; i++)
        {
            if (i == 0 || i == request.Length - 1) continue;

            if (int.TryParse(request[i], out var number) && number is >= 0 and <= 36)
            {
                numbers.Add(number);
            }
            else
            {
                return new List<int>();
            }
        }

        return numbers;
    }


    private async Task MakeBet(Player currentPlayer, List<int> numbers, int sum, BetType betType)
    {
        if (numbers.Count == 0 || sum < MinBet)
        {
            await _textChannel.SendMessageAsync("Some error occured. Check your request and try again!");
            return;
        }
        currentPlayer.MakeBet(sum, out sum);
        var bet = new Bet() {Numbers = numbers, Sum = sum, BetType = betType, Player = currentPlayer, BetState = BetState.NotPlayed};
        Bets[currentPlayer].Add(bet);
        _dbService.BetRepository.SaveBet(bet);
        _dbService.SaveChanges();
        await ConfirmingBetMessage(GetUserById(currentPlayer).Username, bet);
    }

    private async Task ConfirmingBetMessage(string username, Bet bet)
    {
        await _textChannel.SendMessageAsync($"Player {username} - {bet.Sum} dollars to {string.Join(", ", bet.Numbers)}. Bet type - {bet.BetType}.");
    }

    private async Task GetTopProfitList()
    {
        var players = _dbService.PlayerRepository.GetPlayers().OrderByDescending(player => player.Profit);
        var stringBuilder = new StringBuilder();
        int count = 1;
        foreach (var player in players)
        {
            stringBuilder.AppendLine($"{count}. {GetUserById(player).Username} - {player.Profit}.");
            count++;
        }

        await _textChannel.SendMessageAsync(stringBuilder.ToString());
    }
    
    private async Task GetPlayerBets(Player player)
    {
        var bets = _dbService.BetRepository.GetPlayersBets(player);
        var stringBuilder = new StringBuilder();
        foreach (var bet in bets)
        {
            stringBuilder.AppendLine($"Bet type - {bet.BetType}, {bet.Sum} dollars. Status - {bet.BetState}");
        }
        await _textChannel.SendMessageAsync(stringBuilder.ToString());
    }

    private async Task SpinRoulette()
    {
        var random = new Random();
        var res = random.Next(37);
        Color color = res switch
        {
            0 => Color.Green,
            var n when NumberCombinations.RedNumbers.Contains(n) => Color.Red,
            _ => Color.Default
        };

        string path = $"Resources/Gifs/{res}.gif";
        var image = Path.GetFileName(path);
        var embed = new EmbedBuilder()
            .WithImageUrl($"attachment://{image}")
            .WithCurrentTimestamp()
            .Build();
        var message = await _textChannel.SendFileAsync(path, embed: embed);
        await Task.Delay(11000);
        var eb = new EmbedBuilder()
            .WithImageUrl($"attachment://{image}")
            .WithTitle($"The winning number is {res}!")
            .WithColor(color)
            .WithCurrentTimestamp();
        await message.ModifyAsync(m => m.Embeds = new Embed[] { eb.Build() });
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var player in Players)
        {
            if (Bets.TryGetValue(player, out var bets))
            {
                foreach (var bet in bets)
                {
                    if (bet.Numbers.Contains(res))
                    {
                        var coef = GetCoef(bet);
                        player.Win(bet.Sum, coef);
                        bet.BetState = BetState.Win;
                        stringBuilder.AppendLine($"{GetUserById(player).Username} wins {bet.BetType} bet and {bet.Sum * (coef - 1)} dollars.");
                    }
                    else
                    {
                        bet.BetState = BetState.Lose;
                        stringBuilder.AppendLine($"{GetUserById(player).Username} loses his {bet.BetType} bet and {bet.Sum} dollars.");
                    }
                }
                _dbService.SaveChanges();
                Bets[player].Clear();
            }
        }
        await _textChannel.SendMessageAsync(stringBuilder.ToString());
    }

    private int GetCoef(Bet bet)
    {
        switch (bet.BetType)
        {
            case BetType.StraightUp:
                return 36;
            
            case BetType.Split:
                return 18;
            
            case BetType.Street:
            case BetType.StreetAZero:
                return 12;
            
            case BetType.Corner:
            case BetType.FirstFour:
                return 8;
            
            case BetType.Sixline:
                return 6;
            
            case BetType.Dozen:
            case BetType.Column:
                return 3;
            
            case BetType.Red:
            case BetType.Black: 
            case BetType.Even:
            case BetType.Odd: 
            case BetType.SmallNumbers: 
            case BetType.BigNumbers: 
                return 2;
            
            default:
                return 0;
        }
    }
    
    public async Task StartGame()
    {
        await _textChannel.SendMessageAsync("Welcome to casino! Make your stakes ladies and gentlemen!");
        await GetBoard();
        await GetHelp();
    }

    private async Task GetBoard()
    {
        await _textChannel.SendFileAsync($"Resources/board.png");
    }
    
    private async Task GetHelp()
    {
        await _textChannel.SendFileAsync($"Resources/rules.txt");

    }
    

    private async Task AddPlayer(SocketMessage arg)
    {
        var player = new Player { UserId = arg.Author.Id, Balance = 1000, Profit = 0};
        Players.Add(player);
        _dbService.PlayerRepository.SavePlayer(player);
        _dbService.SaveChanges();
        Bets.Add(player, new List<Bet>());
        await _textChannel.SendMessageAsync($"{arg.Author} is connected to the game!");
    }

    private IUser GetUserById(Player player)
    {
        return _client.GetUser(player.UserId);
    }

    private async Task<bool> CheckIfConnected(SocketMessage arg, Player currentPlayer)
    {
        if (currentPlayer == null)
        {
            await _textChannel.SendMessageAsync($"{arg.Author} is not connected to the game! Type 'connect' to play!");
            return false;
        }

        return true;
    }
}