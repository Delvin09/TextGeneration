using TextGeneration.Lib.Expressions;
using TextGeneration.Lib.Interfaces;

namespace TextGeneration.Lib;

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