using System.Data;
using System.Text;
using TextGeneration.Lib.Expressions;
using TextGeneration.Lib.Interfaces;

namespace TextGeneration.Tests.Unit;

public class NumExpressionTests
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
    [InlineData("2[aaa]")]
    [InlineData("2[3[b]5[c]]")]
    public void IsExpression_Test(string pattern)
    {
        var actual = NumWordExpression.IsExpression(pattern);
        Assert.True(actual);
    }
    
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    [InlineData("[2[aaa]]")]
    [InlineData("2[aaa]3[bbb]")]
    [InlineData("a2")]
    [InlineData("a")]
    [InlineData("aaaa")]
    public void IsNotExpression_Test(string pattern)
    {
        var actual = NumWordExpression.IsExpression(pattern);
        Assert.False(actual);
    }
    
    [Theory]
    [InlineData("2[aaa]")]
    [InlineData("2[3[b]5[c]]")]
    public void ParseExpression_Success_Test(string pattern)
    {
        var actual = new NumWordExpression(_factory);
        actual.Parse(pattern);
    }
    
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    [InlineData("[2[aaa]]")]
    [InlineData("2[aaa]3[bbb]")]
    [InlineData("a2")]
    [InlineData("a")]
    [InlineData("aaaa")]
    public void ParseExpression_Failed_Test(string pattern)
    {
        var actual = new NumWordExpression(_factory);
        Assert.Throws<SyntaxErrorException>(() => actual.Parse(pattern));
    }
    
    [Theory]
    [InlineData("2[aaa]", "[aaa][aaa]")]
    [InlineData("2[3[b]5[c]]", "[3[b]5[c]][3[b]5[c]]")]
    public void PrintExpression_Success_Test(string pattern, string expected)
    {
        var actual = new NumWordExpression(_factory);
        actual.Parse(pattern);

        Assert.Equal(expected, actual.Print(new StringBuilder()).ToString());
    }
}