using System.Data;
using System.Text;
using TextGeneration.Lib.Expressions;

namespace TextGeneration.Tests.Unit;

public class WordExpressionTests
{
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    public void IsExpression_Test(string pattern)
    {
        var actual = WordExpression.IsExpression(pattern);
        Assert.True(actual);
    }
    
    [Theory]
    [InlineData("2[a]")]
    [InlineData("[2[aaa]]")]
    [InlineData("a2")]
    [InlineData("a")]
    [InlineData("aaaa")]
    public void IsNotExpression_Test(string pattern)
    {
        var actual = WordExpression.IsExpression(pattern);
        Assert.False(actual);
    }
    
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    public void ParseExpression_Success_Test(string pattern)
    {
        var actual = new WordExpression();
        actual.Parse(pattern);
    }
    
    [Theory]
    [InlineData("2[a]")]
    [InlineData("[2[aaa]]")]
    [InlineData("a2")]
    [InlineData("a")]
    [InlineData("aaaa")]
    public void ParseExpression_Failed_Test(string pattern)
    {
        var actual = new WordExpression();
        Assert.Throws<SyntaxErrorException>(() => actual.Parse(pattern));
    }
    
    [Theory]
    [InlineData("[a]")]
    [InlineData("[aaaa]")]
    public void PrintExpression_Success_Test(string pattern)
    {
        var actual = new WordExpression();
        actual.Parse(pattern);

        Assert.Equal(pattern[1..^1], actual.Print(new StringBuilder()).ToString());
    }
}