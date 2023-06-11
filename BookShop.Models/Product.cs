using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BookShop.Models.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 1")]
        public double Price { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [Required]
        [ValidateNever]
        public Category Category { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
