using System.Data;
using System.Text;
using TextGeneration.Lib.Interfaces;

namespace TextGeneration.Lib.Expressions;

public class NumWordExpression : IExpression // 2[3[b]4[c]]
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
        if (index <= 0 || !int.TryParse(span[..index], out var number)) return false;

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
            _innerExpression?.Print(builder);
        
        return builder;
    }
}