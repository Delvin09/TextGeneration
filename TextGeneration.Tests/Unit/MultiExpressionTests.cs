using System.Data;
using System.Text;
using TextGeneration.Lib.Expressions;
using TextGeneration.Lib.Interfaces;

namespace TextGeneration.Tests.Unit;

public class MultiExpressionTests
{
    class FakeExpressionFactory : IExpressionFactory
    {
        public IExpression Create(ReadOnlySpan<char> span)
        {
            return new FakeExpression();
        }
    }

    class FakeExpression : IExpression
    {
        private string _word = string.Empty;
        
        public void Parse(ReadOnlySpan<char> span)
        {
            _word = span.ToString();
        }

        public StringBuilder Print(StringBuilder builder)
        {
            return builder.Append(_word);
        }
    }
    
    private readonly IExpressionFactory _factory = new FakeExpressionFactory();
    
    [Theory]
    [InlineData("2[a]3[b]4[cc]")]
    [InlineData("[2[aaa]]")]
    [InlineData("zz2[a]3[b]4[cc]", Skip = "Not implemented yet")]
    [InlineData("2[a]3[b]4[cc]zz", Skip = "Not implemented yet")]
    [InlineData("2[a]3[b]zz4[cc]zz", Skip = "Not implemented yet")]
    public void IsExpression_Test(string pattern)
    {
        var actual = MultiExpression.IsExpression(pattern);
        Assert.True(actual);
    }
    
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    [InlineData("2[aaa]")]
    [InlineData("a2")]
    [InlineData("a")]
    [InlineData("aaaa")]
    [InlineData("2[3[b]5[c]]")]
    public void IsNotExpression_Test(string pattern)
    {
        var actual = MultiExpression.IsExpression(pattern);
        Assert.False(actual);
    }
    
    [Theory]
    [InlineData("2[a]3[b]4[cc]")]
    [InlineData("zz2[a]3[b]4[cc]", Skip = "Not implemented yet")]
    [InlineData("2[a]3[b]4[cc]zz", Skip = "Not implemented yet")]
    [InlineData("2[a]3[b]zz4[cc]zz", Skip = "Not implemented yet")]
    public void ParseExpression_Success_Test(string pattern)
    {
        var actual = new MultiExpression(_factory);
        actual.Parse(pattern);
    }
    
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    [InlineData("a2")]
    [InlineData("a")]
    [InlineData("aaaa")]
    [InlineData("2[aaa]")]
    [InlineData("2[3[b]5[c]]")]
    public void ParseExpression_Failed_Test(string pattern)
    {
        var actual = new MultiExpression(_factory);
        Assert.Throws<SyntaxErrorException>(() => actual.Parse(pattern));
    }

    [Theory]
    [InlineData("2[a]3[b]4[cc]")]
    [InlineData("[2[aaa]]", "2[aaa]")]
    [InlineData("zz2[a]3[b]4[cc]", Skip = "Not implemented yet")]
    [InlineData("2[a]3[b]4[cc]zz", Skip = "Not implemented yet")]
    [InlineData("2[a]3[b]zz4[cc]zz", Skip = "Not implemented yet")]
    public void PrintExpression_Success_Test(string pattern, string? expected = null)
    {
        var actual = new MultiExpression(_factory);
        actual.Parse(pattern);
        
        Assert.Equal(expected ?? pattern, actual.Print(new StringBuilder()).ToString());
    }
}