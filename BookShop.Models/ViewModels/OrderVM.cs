using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models.ViewModels
{
    public class OrderVM
    {
        public int ID { get; set; }
        public IEnumerable<OrderDetails> ProductList { get; set; }
        public string PaymentStatus { get; set; }
        public string PhoneNumber { get; set; }
        public double OrderTotal { get; set; }
        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public string OrderStatus { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }

        public DateTime ShippingDate { get; set; }

    }
}
