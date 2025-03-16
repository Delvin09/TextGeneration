using System.Text;

namespace TextGeneration.Lib.Interfaces;

public interface IExpression
{
    void Parse(ReadOnlySpan<char> span);
    StringBuilder Print(StringBuilder builder);
}