using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PROJEKT1.Models;

namespace PROJEKT1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetProductController : ControllerBase
    {

        private readonly ILogger<GetProductController> _logger;

        public GetProductController(ILogger<GetProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetProduct")]
        public Product Get()
        {
            return new Product(1, "test", "https://www.youtube.com/watch?v=dQw4w9WgXcQ", 123.4m);
        }

        static async void GetAndParseWebsite(string url, ILogger logger)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string htmlContent = await client.GetStringAsync(url);

                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);

                    var productItems = htmlDocument.DocumentNode.SelectNodes("//div[@class='ProductItem']");

                    if (productItems != null)
                    {
                        foreach (var productItem in productItems)
                        {
                            logger.LogInformation(productItem.InnerHtml);
                        }
                    }
                    else
                    {
                        logger.LogInformation("Nie znaleziono elementów o klasie 'ProductItem'.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"Wyst¹pi³ b³¹d: {ex.Message}");
                }
            }
        }

        public static List<Product> ParseProducts(string htmlContent)
        {
            var products = new List<Product>();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            var productNodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'ProductItem')]");

            if (productNodes != null)
            {
                int i = 1;
                foreach (var productNode in productNodes)
                {
                    string name = productNode.SelectSingleNode(".//span[@class='Title']/a")?.InnerText?.Trim();
                    string url = productNode.SelectSingleNode(".//span[@class='Title']/a")?.GetAttributeValue("href", "");
                    string priceString = productNode.SelectSingleNode(".//p[@class='Price']")?.InnerText?.Trim();

                    if (decimal.TryParse(priceString?.Replace(" z³", ""), out decimal price))
                    {
                        products.Add(new Product(i, name, url, price));
                        i++;
                    }
                }
            }

            return products;
        }
    }


}