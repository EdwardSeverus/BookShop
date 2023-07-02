using BookShop.DataAccess.Repository;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
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
            IEnumerable<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .OrderBy(x => Guid.NewGuid())
                .Take(8);

            IEnumerable<Product> featured = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .OrderBy(x => Guid.NewGuid())
                .Take(3);


            IEnumerable<Product> newArrival = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .OrderByDescending(p => p.Id)
                .Take(4);

            IEnumerable<OrderDetails> orderDetails = _unitOfWork.OrderDetails.GetAll(includeProperties: "Product");

            var productCounts = orderDetails
                .GroupBy(d => d.ProductId)
                .Select(g => new ProductCountViewModel
                {
                    ProductId = g.Key,
                    Product = g.First().Product,
                    TotalCount = g.Sum(d => d.Count)
                })
                .OrderByDescending(p => p.TotalCount)
                .Take(4);

            var indexModel = new IndexModel
            {
                ProductList = objProductList,
                BestSeller = productCounts,
                NewArrival = newArrival,
                Featured = featured
            };

            return View(indexModel);
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

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.CustomerId == applicationUser.Id && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count = cartFromDb.Count + shoppingCart.Count;
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

        public IActionResult AllProduct()
        {
            IEnumerable<Product> objProductList;
            objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");

            var categories = _unitOfWork.Category.GetAll(); // Retrieve all categories

            var viewModel = new ProductCategoryViewModel
            {
                Products = objProductList,
                Categories = categories,
                SelectedCategories = new List<int>()
            };

         
            return View(viewModel);
        }


        [HttpPost]
        public IActionResult AllProduct(List<int> selectedCategories)
        {
            IEnumerable<Product> objProductList;

            if (selectedCategories.Count > 0)
            {
                objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category")
                    .Where(p => selectedCategories.Contains(p.CategoryId));
                    
            }
            else
            {
                objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");
                    
            }

            var categories = _unitOfWork.Category.GetAll();

            var viewModel = new ProductCategoryViewModel
            {
                Products = objProductList,
                Categories = categories,
                SelectedCategories = selectedCategories
            };

            return View(viewModel);
        }


        public IActionResult Search()
        {
            return RedirectToAction("AllProduct");
        }


        [HttpPost]
        public IActionResult Search(string searchQuery)
        {
            // Perform search logic based on the search query
            IEnumerable<Product> searchResults = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Title.ToLower().Contains(searchQuery.ToLower())
                    || p.Author.ToLower().Contains(searchQuery.ToLower()));




            var categories = _unitOfWork.Category.GetAll();

            var viewModel = new ProductCategoryViewModel
            {
                Products = searchResults,
                Categories = categories,
                SelectedCategories = new List<int>()
            };

            return View("AllProduct", viewModel);
        }



        public IActionResult NewArrival()
        {
            IEnumerable<Product> newArrival = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .OrderByDescending(p => p.Id)
                .Take(12);
            return View(newArrival);
        }


        public async Task<IActionResult> BestSeller()
        {

            IEnumerable<OrderDetails> orderDetails = _unitOfWork.OrderDetails.GetAll(includeProperties: "Product");


            var productCounts = orderDetails
                .GroupBy(d => d.ProductId)
                .Select(g => new ProductCountViewModel
                {
                    ProductId = g.Key,
                    Product = g.First().Product,
                    TotalCount = g.Sum(d => d.Count)
                })
                .OrderByDescending(p => p.TotalCount).Take(12);


            return View(productCounts);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AboutUs()
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