namespace TextGeneration.Lib;

readonly ref struct ParseResult()
{
    public ReadOnlySpan<char> Result { get; init; } = default;

    public ReadOnlySpan<char> Tail { get; init; } = default;
}