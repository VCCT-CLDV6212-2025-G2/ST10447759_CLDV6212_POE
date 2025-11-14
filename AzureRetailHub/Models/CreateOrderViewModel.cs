/*
 * Jeron Okkers
 * ST10447759
 * PROG6221
 */ 
namespace AzureRetailHub.Models
{
    public class CreateOrderViewModel
    {
        public Order Order { get; set; } = new Order();
        public IEnumerable<CustomerDto> AvailableCustomers { get; set; } = new List<CustomerDto>();
        public IEnumerable<Product> AvailableProducts { get; set; } = new List<Product>();
    }
}
//================================================================================================================================================================//
