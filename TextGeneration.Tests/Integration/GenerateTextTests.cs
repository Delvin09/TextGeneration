﻿using System.Text;
using TextGeneration.Lib;

namespace TextGeneration.Tests.Integration;

public class GenerateTextTests
{
    private string GenerateString(string input)
    {
        var factory = new ExpressionFactory();
        var expression = factory.Create(input);
        expression.Parse(input);
        
        StringBuilder builder = new StringBuilder();
        return expression.Print(builder).ToString();
    }
    
    [Theory]
    [InlineData("2[a]", "aa")]
    [InlineData("11[a]", "aaaaaaaaaaa")]
    public void Numbers_Test(string input, string expected)
    {
        Assert.Equal(expected, GenerateString(input));
    }
    
    [Fact]
    public void Multi_Test()
    {
        string input = "2[a]3[b]4[cc]";
        Assert.Equal("aabbbcccccccc", GenerateString(input));
    }
    
    [Fact]
    public void NumberWithMulti_Test()
    {
        string input = "2[3[b]4[c]]";
        Assert.Equal("bbbccccbbbcccc", GenerateString(input));
    }
    
    [Fact(Skip = "Not implemented yet")]
    // 2[a3[b]4[c]]              -> abbbccccabbbcccc
    public void Test4()
    {
        string input = "2[a3[b]4[c]]";
        Assert.Equal("abbbccccabbbcccc", GenerateString(input));
    }
}