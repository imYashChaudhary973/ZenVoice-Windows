using System.Text.RegularExpressions;

namespace ZenVoice.Core;

public sealed class TranscriptCleaner
{
    private static readonly HashSet<string> NonSpeechFragments =
    [
        "you",
        "thanks for watching",
        "thank you for watching",
        "thanks for watching!",
        "subtitles by the amara.org community"
    ];

    private static readonly Regex Brackets = new(@"\s*\[[^\]]+\]\s*", RegexOptions.Compiled);
    private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex Filler = new(
        @"(^|(?<=[.!?]\s))(?:(?:um+|uh+|erm+)[,.\s]+)+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Annotation = new(@"\([^)]*\)", RegexOptions.Compiled);

    public string Clean(string transcript)
    {
        var result = Spaces.Replace(Brackets.Replace(transcript, " "), " ").Trim();
        result = Filler.Replace(result, "$1");
        if (IsNonSpeech(result) || result.Length == 0)
        {
            return "";
        }

        return char.ToUpperInvariant(result[0]) + result[1..];
    }

    private static bool IsNonSpeech(string transcript)
    {
        var withoutAnnotations = Annotation.Replace(transcript, "").Trim();
        if (withoutAnnotations.Length == 0 && transcript.Length > 0)
        {
            return true;
        }

        var normalized = transcript.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            return false;
        }

        if (NonSpeechFragments.Contains(normalized))
        {
            return true;
        }

        return NonSpeechFragments.Contains(normalized.TrimEnd('.', '!', '?', ','));
    }
}
