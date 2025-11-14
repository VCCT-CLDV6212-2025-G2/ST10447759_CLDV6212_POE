/*
 * Jeron Okkers
 * ST10447759
 * PROG6221
 */ 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureRetailHub.Models
{
    public class Product
    {
        // Renamed RowKey to Id for EF Core convention
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
    }
}
//================================================================================================================================================================//
