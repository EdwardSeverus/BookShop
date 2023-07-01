using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models.ViewModels
{
    public class IndexModel
    {
        public IEnumerable<Product> ProductList { get; set; }
        public IEnumerable<ProductCountViewModel> BestSeller {  get; set; }

        public IEnumerable<Product> NewArrival { get; set; }

    }
}
