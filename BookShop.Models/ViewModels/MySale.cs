using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models.ViewModels
{
    public class MySale
    {
        public IEnumerable<ProductCountViewModel> productCountViewModel { get; set; }
        public double Balance { get; set; }
    }
}
