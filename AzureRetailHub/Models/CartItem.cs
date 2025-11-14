/*
 * ST10447759
 * CLDV6212
 * Part 3
 */
namespace AzureRetailHub.Models
{
    public class CartItem
    {
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}