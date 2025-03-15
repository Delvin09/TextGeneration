using System.Data;
using System.Text;

namespace TextGeneration.Lib;

// 2[a]             -> aa 
// 4[a]             -> aaaa
// 2[a]3[b]4[cc]    -> aabbbcccccccc
// 2[a3[b]4[c]]     -> abbbccccabbbcccc

// 1. все буквы только в паттерне.
// 2. все цифры только в каунте паттерна.

public interface IExpression
{
    void Parse(ReadOnlySpan<char> span);
    StringBuilder Print(StringBuilder builder);
}

public interface IExpressionFactory
{
    IExpression Create(ReadOnlySpan<char> span);
}

public class ExpressionFactory : IExpressionFactory
{
    public IExpression Create(ReadOnlySpan<char> span)
    {
        IExpression result;

        if (WordExpression.IsExpression(span))
        {
            result = new WordExpression();
        }
        else if (NumWordExpression.IsExpression(span))
        {
            result = new NumWordExpression(this);
        }
        else if (MultiExpression.IsExpression(span))
        {
            result = new MultiExpression(this);
        }
        else
        {
            throw new InvalidOperationException();
        }
        
        return result;
    }
}

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

class NumWordExpression : IExpression // 2[3[b]4[c]]
{
    private readonly IExpressionFactory _factory;
    
    private int _number = 0;
    private IExpression? _innerExpression;

    public NumWordExpression(IExpressionFactory factory)
    {
        _factory = factory;
    }

    public static bool IsExpression(ReadOnlySpan<char> span)
    {
        var index = span.IndexOf('['); // 213[ 123[adasd[ ] ]
        if (!int.TryParse(span[..index], out var number)) return false;

        span = span[index..];
        int bc = 1;
        for (int i = 1; i < span.Length; i++)
        {
            if (bc == 0) return i == span.Length - 1;

            if (span[i] == '[') bc++;
            else if (span[i] == ']') bc--;
        }
        return bc == 0;
    }
    
    public void Parse(ReadOnlySpan<char> span)
    {
        if (!IsExpression(span))
            throw new SyntaxErrorException();

        var index = span.IndexOf('[');
        _number = int.Parse(span[..index]);
        
        span = span[index..];
        _innerExpression = _factory.Create(span);
        _innerExpression.Parse(span);
    }

    public StringBuilder Print(StringBuilder builder)
    {
        for (int i = 0; i < _number; i++)
            _innerExpression.Print(builder);
        
        return builder;
    }
}

class MultiExpression : IExpression // 2[a]3[b]4[cc] || 2[a3[b]4[c]]
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

public readonly ref struct ParseResult()
{
    public ReadOnlySpan<char> Result { get; init; } = default;

    public ReadOnlySpan<char> Tail { get; init; } = default;
}
