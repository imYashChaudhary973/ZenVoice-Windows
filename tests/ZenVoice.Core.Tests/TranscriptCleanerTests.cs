using ZenVoice.Core;

namespace ZenVoice.Core.Tests;

public class TranscriptCleanerTests
{
    private readonly TranscriptCleaner _cleaner = new();

    [Theory]
    [InlineData("   hello    from\nZenVoice   ", "Hello from ZenVoice")]
    [InlineData("[BLANK_AUDIO]", "")]
    [InlineData("um, this is, um, still meaningful.", "This is, um, still meaningful.")]
    [InlineData("(static)", "")]
    [InlineData("(wind blowing) (computer speaking)", "")]
    [InlineData("the total (before tax) is fifty", "The total (before tax) is fifty")]
    [InlineData("you", "")]
    [InlineData("You.", "")]
    [InlineData("can you review this", "Can you review this")]
    [InlineData("Thank you.", "Thank you.")]
    public void MatchesMacCleaner(string input, string expected)
    {
        Assert.Equal(expected, _cleaner.Clean(input));
    }
}
