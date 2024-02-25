using Microsoft.AspNetCore.Mvc;
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
    }
}