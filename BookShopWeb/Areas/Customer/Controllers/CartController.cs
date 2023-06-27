using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<ShoppingCart> cartList = _unitOfWork.ShoppingCart.GetAll();
            return View(cartList);
        }
    }
}
