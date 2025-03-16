using System.Data;
using System.Text;
using TextGeneration.Lib.Interfaces;

namespace TextGeneration.Lib.Expressions;

class WordExpression : IExpression // ..[asdad]
{
    private string _word = string.Empty;

    public static bool IsExpression(ReadOnlySpan<char> span)
    {
        if (span[0] != '[' || span[^1] != ']') return false;

        span = span[1..^1];
        foreach (var letter in span)
        {
            if (!char.IsLetter(letter))
                return false;
        }

        return true;
    }
    
    public void Parse(ReadOnlySpan<char> span)
    {
        if (!IsExpression(span))
            throw new SyntaxErrorException();
        
        _word = span[1..^1].ToString();
    }

    public StringBuilder Print(StringBuilder builder) => builder.Append(_word);
}