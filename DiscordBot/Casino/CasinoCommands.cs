using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Casino;
using DiscordBot.DB.Services;

namespace DiscordBot;

public class CasinoCommands: ModuleBase<SocketCommandContext>
{
    private readonly DiscordSocketClient _client;
    private readonly IDbService _dbService;
    public CasinoCommands(DiscordSocketClient client, IDbService dbService)
    {
        _client = client;
        _dbService = dbService;
    }
    [Command("casino")]
    public async Task Casino()
    {
        var casinoGame = new CasinoGame(_client, (ITextChannel)Context.Channel, Context.User, _dbService);
        await casinoGame.StartGame();
    }
}