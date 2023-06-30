using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
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
                ListCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id),
                OrderHeader = new()
            };
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Product.Price * cart.Count;
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

        public async Task<IActionResult> Buy()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.Name = user.FirstName;
            ShoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = user.Address;
            

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Product.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(ShoppingCartVM shoppingCartVM)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            shoppingCartVM.OrderHeader.Name = user.FirstName;
            shoppingCartVM.OrderHeader.CustomerId = user.Id;
            shoppingCartVM.OrderHeader.ShippingDate = DateTime.Now.AddDays(7);
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.OrderStatus = "Processing";
            shoppingCartVM.OrderHeader.PaymentStatus = "Pending";
            var myCart = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").Where(u => u.CustomerId == user.Id);
            double TotalPrice = 0;
            foreach (var cart in myCart)
            {
                TotalPrice += cart.Product.Price * cart.Count;
            }
            shoppingCartVM.OrderHeader.OrderTotal = TotalPrice;


            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in myCart)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartVM.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.Product.Price 
                };
                _unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.Save();

                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

            }



            return RedirectToAction("index");
        }
    }
}
