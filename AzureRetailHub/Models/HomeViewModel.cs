namespace AzureRetailHub.Models
{
    public class HomeViewModel
    {
        public List<Product> NewArrivals { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
    }
}