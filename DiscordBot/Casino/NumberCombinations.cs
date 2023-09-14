namespace DiscordBot.Casino;

public static class NumberCombinations
{
    public static readonly List<int> RedNumbers = new()
        { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
    public static readonly List<int> EvenNumbers = Enumerable.Range(1, 36).Where(num => num % 2 == 0).ToList();
    public static readonly List<int> BlackNumbers = Enumerable.Range(1, 36).Except(RedNumbers).ToList();
    public static readonly List<int> OddNumbers = Enumerable.Range(1, 36).Where(num => num % 2 != 0).ToList();
    public static readonly List<int> FirstDozen = Enumerable.Range(1, 12).ToList();
    public static readonly List<int> SecondDozen = Enumerable.Range(13, 12).ToList();
    public static readonly List<int> ThirdDozen = Enumerable.Range(25, 12).ToList();
    public static readonly List<int> FirstColumn = Enumerable.Range(1, 36).Where(num => num % 3 == 1).ToList();
    public static readonly List<int> SecondColumn = Enumerable.Range(1, 36).Where(num => num % 3 == 2).ToList();
    public static readonly List<int> ThirdColumn = Enumerable.Range(1, 36).Where(num => num % 3 == 0).ToList();
    public static readonly List<int> SmallNumbers = Enumerable.Range(1, 18).ToList();
    public static readonly List<int> BigNumbers = Enumerable.Range(19, 18).ToList();
    public static readonly List<int> FirstFourNumbers = Enumerable.Range(0, 4).ToList();

}