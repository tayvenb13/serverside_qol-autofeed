using ServersideQoL.AutoFeed;
using Xunit;

namespace AutoFeed.Core.Tests;

public class FeedingLogicTests
{
    static readonly DateTime Now = new(2026, 9, 13, 12, 0, 0);

    [Fact]
    public void SecondsUntilHungry_JustFed_ReturnsFullDuration()
        => Assert.Equal(600d, FeedingLogic.SecondsUntilHungry(Now, Now, 600f), 3);

    [Fact]
    public void SecondsUntilHungry_HalfElapsed_ReturnsRemainder()
        => Assert.Equal(300d, FeedingLogic.SecondsUntilHungry(Now, Now.AddSeconds(-300), 600f), 3);

    [Fact]
    public void SecondsUntilHungry_Elapsed_IsNonPositive()
        => Assert.True(FeedingLogic.SecondsUntilHungry(Now, Now.AddSeconds(-601), 600f) <= 0);

    [Fact]
    public void SecondsUntilHungry_NeverFed_DefaultTimestamp_IsHungry()
        => Assert.True(FeedingLogic.SecondsUntilHungry(Now, default, 600f) <= 0);

    static readonly (string Name, int Stack)[] Slots =
    [
        ("$item_carrot", 20),
        ("$item_turnip", 5),
        ("$item_carrot", 3),
        ("$item_carrot", 0),
    ];

    static int Select(string consumeName)
        => FeedingLogic.SelectSlot(Slots, static s => s.Name, static s => s.Stack, consumeName);

    [Fact]
    public void SelectSlot_PicksSmallestPositiveStack()
        => Assert.Equal(2, Select("$item_carrot"));

    [Fact]
    public void SelectSlot_MatchesExactNameOnly()
        => Assert.Equal(1, Select("$item_turnip"));

    [Fact]
    public void SelectSlot_NoMatch_ReturnsMinusOne()
        => Assert.Equal(-1, Select("$item_barley"));

    [Fact]
    public void SelectSlot_IgnoresEmptyStacks()
        => Assert.Equal(-1, FeedingLogic.SelectSlot(
            new[] { ("$item_carrot", 0) }, static s => s.Item1, static s => s.Item2, "$item_carrot"));
}
