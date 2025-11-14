/*
 * Jeron Okkers
 * ST10447759
 * PROG6221
 * RENAMED TO ORDER.CS FOR PART 3
 */
using System.ComponentModel.DataAnnotations;

namespace AzureRetailHub.Models
{
    // This is the EF Core Order ENTITY
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; } = "New";

        [Required]
        public string ApplicationUserId { get; set; } = "";
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}