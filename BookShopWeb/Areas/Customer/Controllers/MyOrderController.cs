using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookShopWeb.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class MyOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyOrderController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);


            IEnumerable<OrderHeader> orderHeader = _unitOfWork.OrderHeaders.GetAll().Where(u=>u.CustomerId==applicationUser.Id).OrderByDescending(u => u.OrderDate);
            return View(orderHeader);
        }

        public IActionResult details(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u=>u.Id==id);
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
                Address = orderHeader.StreetAddress + ", "+ orderHeader.City + ", " + orderHeader.State + ", " + orderHeader.PostalCode,
            };
            return View(orderVM);
        }

        [HttpPost]
        public IActionResult details(OrderVM order)
        {
            int id = order.ID;
            IEnumerable<OrderDetails> orderDetails = _unitOfWork.OrderDetails.GetAll(includeProperties: "Product").Where(u => u.OrderId == id);
            OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == id);

            var domain = "https://localhost:44340/";
            var options = new SessionCreateOptions
            {

                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/MyOrder/OrderConfirmation?id={id}",
                CancelUrl = domain + $"customer/Myorder",
            };

            foreach (var item in orderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price*100),
                        Currency = "bdt",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },
                    },
                    Quantity = item.Count,
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            orderHeader.SessionId = session.Id;
            orderHeader.PaymentIntentId = session.PaymentIntentId;
            _unitOfWork.OrderHeaders.Update(orderHeader);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u=>u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if(session.PaymentStatus.ToLower()=="paid")
            {
                orderHeader.PaymentStatus = "Paid";
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.OrderHeaders.Update(orderHeader);
                _unitOfWork.Save();
            }
            TempData["success"] = "Payment Successful";
            return RedirectToAction("Index");
        }


        public IActionResult CancelOrder(int id) 
        {
            OrderHeader orderHeader= _unitOfWork.OrderHeaders.GetFirstOrDefault(u=>u.Id == id);
            orderHeader.OrderStatus = "Cancelled";
            orderHeader.IsCancelled = "1";
            _unitOfWork.OrderHeaders.Update(orderHeader) ;
            _unitOfWork.Save();
            return RedirectToAction("Index");

        }

    }
}
