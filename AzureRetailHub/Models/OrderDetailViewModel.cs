/*
 * Jeron Okkers
 * ST10447759
 * PROG6221
 * MODIFIED FOR PART 3
 * Removed ParseItems() and old OrderItem class.
 * Added nested ItemDetail class.
 */
using System.Collections.Generic;

namespace AzureRetailHub.Models
{
    public class OrderDetailViewModel
    {
        // This 'OrderDto' is the new simple DTO class
        public OrderDto Order { get; set; }
        public CustomerDto Customer { get; set; }
        public List<ItemDetail> Items { get; set; } = new List<ItemDetail>();

        // Helper to calculate the total price of the order
        public decimal TotalPrice => Items.Sum(item => item.TotalPrice);

        // This is the nested class the Controller was looking for
        public class ItemDetail
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice => Quantity * Price;
        }

        // DELETED: ParseItems() method (it's obsolete)
        // DELETED: 'public class OrderItem' (it's a separate entity now)
    }
}
//================================================================================================================================================================//