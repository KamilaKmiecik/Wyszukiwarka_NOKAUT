//using Microsoft.AspNetCore.Mvc;
//using PROJEKT1.Models;

//private bool ShouldScrapeAgain(string url)
//{
//    if (!LastTimeScraped.ContainsKey(url))
//    {
//        LastScrapeTimes.Add(url, DateTime.MinValue);
//        return true;
//    }

//    TimeSpan elapsed = DateTime.Now - LastScrapeTimes[url];
//    return elapsed.TotalHours >= 1;
//}

//private async Task ScrapeAndSaveProducts(string url)
//{
//    // Perform scraping and saving products here
//    await GetAndParseWebsite(url, _logger);
//    ParseProducts(url);

//    // Update last scrape time
//    LastTimeScraped[url] = DateTime.Now;
//}

//public async Task<ActionResult<List<Product>>> Get(string productType)
//{
//    string url = $"https://www.nokaut.pl/produkt:{productType}.html";

//    if (ShouldScrapeAgain(productType))
//    {
//        await ScrapeAndSaveProducts(url);
//    }

//    return 
//}
//private static Dictionary<string, DateTime> LastTimeScraped = new Dictionary<string, DateTime>();
