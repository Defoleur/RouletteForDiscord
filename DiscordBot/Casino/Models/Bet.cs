using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace DiscordBot.Casino;

public record Bet
{
    public Guid Id { get; set; }
    [NotMapped] public List<int> Numbers { get; set; } = new List<int>();
    public int Sum { get; set; }
    public BetType BetType { get; set; }
    public BetState BetState { get; set; }
    public Player Player { get; set; }
}