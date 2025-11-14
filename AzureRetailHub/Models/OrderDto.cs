/*
 * ST10447759
 * CLDV6212
 * Part 3
 * This is a simple DTO for the OrderDetailViewModel.
 */
namespace AzureRetailHub.Models
{
    public class OrderDto
    {
        public string Id { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
    }
}