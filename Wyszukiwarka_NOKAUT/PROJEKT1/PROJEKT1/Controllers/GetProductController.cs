using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PROJEKT1.Models;
using System;
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
        public List<Product> Get()
        {
            GetAndParseWebsite("https://www.nokaut.pl/produkt:rower.html", _logger);
            return ParseProducts("https://www.nokaut.pl/produkt:rower.html");
        }
        static List<HtmlNode> FindNodesByClass(HtmlNode parentNode, string targetClass)
        {
            List<HtmlNode> matchingNodes = new List<HtmlNode>();

            if (parentNode.Attributes["class"]?.Value == targetClass)
            {
                matchingNodes.Add(parentNode);
            }

            foreach (var childNode in parentNode.ChildNodes)
            {
                matchingNodes.AddRange(FindNodesByClass(childNode, targetClass));
            }

            return matchingNodes;
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

                var productItems = FindNodesByClass(htmlDocument.DocumentNode, "//div[@class='ProductItem']");
                    productItems = FindNodesByClass(htmlDocument.DocumentNode, "ProductItem");

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

        public static List<Product> ParseProducts(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            List<Product> products = new List<Product>();
            int i = 1;

            HtmlNodeCollection productNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ProductItem')]");
            if (productNodes != null)
            {
                foreach (HtmlNode productNode in productNodes)
                {
                    string name = productNode.SelectSingleNode(".//span[@class='Title']/a")?.InnerText?.Trim();
                    string productUrl = productNode.SelectSingleNode(".//span[@class='Title']/a")?.GetAttributeValue("href", "");
                    string priceString = productNode.SelectSingleNode(".//p[@class='Price']")?.InnerText?.Trim();
                    string imageUrl = productNode.SelectSingleNode(".//img")?.GetAttributeValue("src", "");
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        imageUrl = productNode.SelectSingleNode(".//img")?.GetAttributeValue("data-src", "");
                    }

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(productUrl) && !string.IsNullOrEmpty(priceString))
                    {
                        if (decimal.TryParse(priceString.Replace(" z³", "").Trim(), out decimal price))
                        {
                            products.Add(new Product(i, name, productUrl, price, imageUrl));
                            i++;
                        }
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        string html = productNode.SelectSingleNode(".//span[@class='Title Multi']/a")?.GetAttributeValue("href", "");
                        HtmlDocument document = web.Load(html);

                        HtmlNode offersNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'ProductInfo')]").FirstOrDefault();
                        string offerName = offersNode.SelectSingleNode(".//h2[@class='Title']")?.InnerText?.Trim();

                        HtmlNodeCollection offersNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'PromoOffer')]");
                        if (offersNodes != null)
                        {
                            foreach (HtmlNode offerNode in offersNodes)
                            {
                                string offerPrice = offerNode.SelectSingleNode(".//p[@class='Price']")?.InnerText?.Trim();

                                string offerURL = offerNode.SelectSingleNode("//a[@class='Button']")?.GetAttributeValue("href", "");
                                if (string.IsNullOrEmpty(offerURL))
                                {
                                    offerURL = offerNode.SelectSingleNode(".//img")?.GetAttributeValue("data-src", "");
                                }

                                if (!string.IsNullOrEmpty(offerName) && !string.IsNullOrEmpty(offerURL) && !string.IsNullOrEmpty(offerPrice))
                                {
                                    if (decimal.TryParse(offerPrice.Replace(" z³", "").Trim(), out decimal price))
                                    {
                                        products.Add(new Product(i, offerName, offerURL, price, imageUrl));
                                        i++;
                                    }
                                }
                            }
                        }
                    }


                }

            }
            SaveToDatabase(products, GetConfig.ConnectionString);
            return products;
        }

        static void SaveToDatabase(List<Product> products, string connectionString)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (Product product in products)
                {
                    string insertQuery = "INSERT INTO Products (Name, URL, Price, ImageURL) VALUES (@Name, @URL, @Price, @ImageURL)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@URL", product.URL);
                        command.Parameters.AddWithValue("@Price", product.Price.Value);
                        command.Parameters.AddWithValue("@ImageURL", product.imageUrl);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}