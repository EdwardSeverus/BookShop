﻿using BookShop.DataAccess.Repository;
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
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
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

            ShoppingCart cart = new ShoppingCart();
            cart.ApplicationUserId = applicationUser.Id;
            cart.ProductId = shoppingCart.ProductId;
            cart.Count = shoppingCart.Count;
            _unitOfWork.ShoppingCart.Add(cart);
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