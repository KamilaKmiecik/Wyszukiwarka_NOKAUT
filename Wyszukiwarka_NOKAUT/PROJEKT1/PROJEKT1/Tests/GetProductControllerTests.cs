using HtmlAgilityPack;
using PROJEKT1.Controllers;
using Xunit;

//before running tests comment main function in Program.cs and Install-Package Microsoft.NET.Test.Sdk 

public class GetProductControllerTests
{
    [Fact]
    public void FindNodesByClass_ShouldFindMatchingNodes()
    {
        // Arrange
        var html = @"<div class='ProductItem'>
                        <span class='Title'><a href='/product1'>Product Name 1</a></span>
                        <p class='Price'>100 zł</p>
                        <img src='image1.jpg' />
                      </div>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var parentNode = doc.DocumentNode.SelectSingleNode("//div[@class='ProductItem']");
        var targetClass = "Title";

        // Act
        var matchingNodes = GetProductController.FindNodesByClass(parentNode, targetClass);

        // Assert
        Assert.Equal(1, matchingNodes.Count);
        Assert.True(matchingNodes.All(n => n.Attributes["class"].Value == targetClass));
    }

    [Fact]
    public void ParseProducts_ShouldParseBasicProductInfo()
    {
        // Arrange
        var html = @"<div class='ProductItem'>
                        <span class='Title'><a href='/product1'>Product Name 1</a></span>
                        <p class='Price'>100 zł</p>
                        <img src='image1.jpg' />
                      </div>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Act
        var products = GetProductController.ParseProducts("https://www.nokaut.pl/produkt:autobusik.html");

        // Assert
        Assert.Equal("E-Edu pojazd miejski Autobusik", products[2].Name);
        Assert.Equal(32.62m, products[2].Price.Value);
    }
}