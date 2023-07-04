using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]

    public class Refund : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public Refund(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            IEnumerable<OrderHeader> orderHeader = _unitOfWork.OrderHeaders.GetAll().OrderByDescending(u => u.OrderDate).Where(x => x.OrderStatus == "Cancelled" && x.PaymentStatus=="Paid");
            return View(orderHeader);
        }

        public IActionResult details(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == id);
            IEnumerable<OrderDetails> orderDetails = _unitOfWork.OrderDetails.GetAll(includeProperties: "Product").Where(u => u.OrderId == id);
            OrderVM orderVM = new OrderVM()
            {
                ID = id,
                ProductList = orderDetails,
                Carrier = orderHeader.Carrier,
                PaymentStatus = orderHeader.PaymentStatus,
                TrackingNumber = orderHeader.TrackingNumber,
                OrderTotal = orderHeader.OrderTotal,
                OrderStatus = orderHeader.OrderStatus,
                OrderDate = orderHeader.OrderDate,
                ShippingDate = orderHeader.ShippingDate,
                Name = orderHeader.Name,
                PhoneNumber = orderHeader.PhoneNumber,
                Address = orderHeader.StreetAddress + ", " + orderHeader.City + ", " + orderHeader.State + ", " + orderHeader.PostalCode,
            };
            return View(orderVM);
        }

        [HttpPost]
        public IActionResult details(OrderVM orderVM)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u=>u.Id == orderVM.ID);
            orderHeader.OrderStatus = "Refunded";
            
            _unitOfWork.OrderHeaders.Update(orderHeader);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
