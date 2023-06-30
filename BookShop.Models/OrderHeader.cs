using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        public string CustomerId { get; set; }
        public string Name { get; set; }


        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public DateTime ShippingDate { get; set; }

        public double OrderTotal { get; set; }

        public string? OrderStatus { get; set; }

        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public string? PaymentStatus { get; set; }

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get;set; }

        [Required]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set;}
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }

    }
}
