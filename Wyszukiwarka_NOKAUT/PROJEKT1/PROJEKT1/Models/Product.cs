using Microsoft.EntityFrameworkCore;

namespace PROJEKT1.Models
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Product> Products { get; set; }

         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
         {
             // Skonfiguruj po³¹czenie do swojej bazy danych MSSQL
             string connectionString = GetConfig.ConnectionString;
             optionsBuilder.UseSqlServer(connectionString);
         }
    }

    public class Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string URL { get; set; }

        public PriceCy Price { get; set; }

        public Product(int _id, string _name, string _url, decimal _priceValue)
        {
            ID = _id;
            Name = _name;
            URL = _url;
            Price = new PriceCy(_priceValue);
        }

        public Product()
        {

        }
    }

    public class PriceCy
    {
        public decimal Value { get; set; }

        public string Symbol { get; set; }

        public PriceCy(decimal _value)
        {
            Value = _value;
            Symbol = "PLN";
        }

        public PriceCy(decimal _value, string _symbol)
        {
            Value = _value;
            Symbol = _symbol;
        }
    }
}