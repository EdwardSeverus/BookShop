using BookShop.DataAccess.Repository;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objProductList;
            objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(objProductList);
        }

        //get
        public IActionResult Details(int? id)
        {
            Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category");

            ShoppingCart cartObj = new()
            {
                Count = 1,
                Product = product,
                ProductId = product.Id

            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);

            shoppingCart.CustomerId = applicationUser.Id;
            
            ShoppingCart cartFromDb= _unitOfWork.ShoppingCart.GetFirstOrDefault(u=>u.CustomerId==applicationUser.Id && u.ProductId==shoppingCart.ProductId);

            if(cartFromDb!=null)
            {
                cartFromDb.Count= cartFromDb.Count+shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);

            }
            else
            {
                ShoppingCart cart = new()
                {
                    ProductId = shoppingCart.ProductId,
                    CustomerId = shoppingCart.CustomerId,
                    Count = shoppingCart.Count
                };
                _unitOfWork.ShoppingCart.Add(cart);

            }

            _unitOfWork.Save();
            TempData["Success"] = "Added To Cart";
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}