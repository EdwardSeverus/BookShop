using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id)
            };
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.CartTotal += cart.Product.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cartObj != null)
            {
                cartObj.Count = cartObj.Count + 1;
                _unitOfWork.ShoppingCart.Update(cartObj);
                _unitOfWork.Save();
            }
            return RedirectToAction("index");
        }

        public IActionResult Minus(int cartId)
        {
            var cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cartObj != null)
            {
                if(cartObj.Count <= 1)
                {
                    _unitOfWork.ShoppingCart.Remove(cartObj);
                    _unitOfWork.Save();
                }
                else
                {
                    cartObj.Count = cartObj.Count - 1;
                    _unitOfWork.ShoppingCart.Update(cartObj);
                    _unitOfWork.Save();
                }               
            }
            return RedirectToAction("index");
        }

        public IActionResult remove(int cartId)
        {
            var cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cartObj != null)
            {
                _unitOfWork.ShoppingCart.Remove(cartObj);
                _unitOfWork.Save();
            }
            return RedirectToAction("index");
        }

        public IActionResult Buy()
        {
            return View();
        }
    }
}
