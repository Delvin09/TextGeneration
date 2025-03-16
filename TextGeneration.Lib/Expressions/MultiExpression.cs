using System.Data;
using System.Text;
using TextGeneration.Lib.Interfaces;

namespace TextGeneration.Lib.Expressions;

// 2[a]             -> aa 
// 4[a]             -> aaaa
// 2[a]3[b]4[cc]    -> aabbbcccccccc
// 2[a3[b]4[c]]     -> abbbccccabbbcccc

// 1. все буквы только в паттерне.
// 2. все цифры только в каунте паттерна.

public class MultiExpression : IExpression // 2[a]3[b]4[cc] || 2[a3[b]4[c]]
{
    private readonly IExpressionFactory _factory;
    private readonly List<IExpression> _expressions = new();

    public MultiExpression(IExpressionFactory factory)
    {
        _factory = factory;
    }
    
    public static bool IsExpression(ReadOnlySpan<char> span)
    {
        var hasLetters = false;
        var hasNumbers = false;

        foreach (var symbol in span)
        {
            if (hasLetters && hasNumbers)
                return true;
            
            if (char.IsLetter(symbol))
                hasLetters = true;
            else if(char.IsDigit(symbol))
                hasNumbers = true;
        }

        return false;
    }

    public void Parse(ReadOnlySpan<char> span)
    {
        if (!IsExpression(span))
            throw new SyntaxErrorException();
        
        if (span[0] == '[' && span[^1] == ']')
            span = span[1..^1];

        while (!span.IsEmpty)
        {
            ParseResult result;
            if (span[0] == '[' || char.IsLetter(span[0]))
            {
                // parse letters
                result = ParseLetters(span);
            }
            else if (char.IsNumber(span[0]))
            {
                // parse number
                result = ParseNumber(span);
            }
            else
            {
                throw new SyntaxErrorException();
            }
            
            var expression = _factory.Create(result.Result);
            _expressions.Add(expression);
            expression.Parse(result.Result);
            
            span = result.Tail;
        }
    }

    private ParseResult ParseNumber(ReadOnlySpan<char> span)
    {
        // 112312[.[]..[].]3123524[...]
        int index = span.IndexOf('[');
        int bracketCount = 1;
        int i;
        for (i = index + 1; i < span.Length; i++)
        {
            if (bracketCount == 0)
            {
                return new ParseResult { Result = span[..i], Tail = span[i..] };
            }
            
            if (span[i] == '[')
                bracketCount++;
            else if (span[i] == ']')
                bracketCount--;
        }

        return bracketCount == 0 && i == span.Length
            ? new ParseResult { Result = span[..i], Tail = span[i..] }
            : throw new SyntaxErrorException();
    }
    
    private ParseResult ParseLetters(ReadOnlySpan<char> span)
    {
        int i;
        for (i = 1; i < span.Length; i++)
        {
            if (!char.IsLetter(span[i]))
                break;
        }
        
        return new ParseResult { Result = span[..(i + 1)], Tail = span[(i+1)..] };
    }

    public StringBuilder Print(StringBuilder builder)
    {
        foreach (var expression in _expressions)
        {
            expression.Print(builder);
        }

        return builder;
    }
}