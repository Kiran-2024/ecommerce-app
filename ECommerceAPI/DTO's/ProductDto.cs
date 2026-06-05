using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }
    }
}