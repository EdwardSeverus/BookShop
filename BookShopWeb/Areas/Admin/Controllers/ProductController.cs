using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models.ViewModels;
using BookShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin,Seller")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser user=await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin"))
            {
                var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").OrderByDescending(p => p.Id);
                return View(objProductList);

            }
            else
            {
                var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").Where(p => p.ApplicationUserId == user.Id).OrderByDescending(p => p.Id);
                return View(objProductList);

            }
        }

        //get
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };

            if (id == 0 || id == null)
            {
                return View(productVM);
            }

            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM obj, IFormFile file)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            

                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                }
            if (obj.Product.Id != 0)
            {
                obj.Product.ApplicationUserId = user.Id;
                _unitOfWork.Product.Update(obj.Product);
                TempData["success"] = "Product Updated Successfully";

            }

            else
            {
                obj.Product.ApplicationUserId = user.Id;
                _unitOfWork.Product.Add(obj.Product);
                TempData["success"] = "Product Created Successfully";

            }


            _unitOfWork.Save();
            return RedirectToAction("Index");
        }


        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Product product)
        {
            if (product.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product Deleted Successfully";
            return RedirectToAction("Index");
        }

        public async Task <IActionResult> Sale()
        {
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);


            IEnumerable<OrderHeader> cancelledOrderHeaders = _unitOfWork.OrderHeaders.GetAll().Where(h => h.OrderStatus == "Cancelled");
            IEnumerable<int> cancelledOrderIds = cancelledOrderHeaders.Select(h => h.Id);

            IEnumerable<OrderDetails> orderDetails = _unitOfWork.OrderDetails.GetAll(includeProperties: "Product");

            
            var productCounts = orderDetails
                .Where(d => d.Product.ApplicationUserId == applicationUser.Id && !cancelledOrderIds.Contains(d.OrderId)) // Filter by matching userId
                .GroupBy(d => d.ProductId)
                .Select(g => new ProductCountViewModel
                {
                    ProductId = g.Key,
                    Product = g.First().Product,
                    TotalCount = g.Sum(d => d.Count)
                })
                .OrderByDescending(p => p.TotalCount);

            
            return View(productCounts);
        }


    }
}
