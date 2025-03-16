namespace TextGeneration.Lib.Interfaces;

public interface IExpressionFactory
{
    IExpression Create(ReadOnlySpan<char> span);
}