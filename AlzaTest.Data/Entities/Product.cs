using System.ComponentModel.DataAnnotations;

namespace AlzaTest.Data.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public int Quantity { get; set; }
    }
}
