using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models.ViewModels;
using BookShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;

        }
        public IActionResult Index()
        {
            var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(objProductList);
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
        public IActionResult Upsert(ProductVM obj, IFormFile file)
        {
            if (ModelState.IsValid)
            {
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
            }
            if (obj.Product.Id != 0)
            {
                _unitOfWork.Product.Update(obj.Product);
                TempData["success"] = "Product Updated Successfully";

            }

            else
            {

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

    }
}
