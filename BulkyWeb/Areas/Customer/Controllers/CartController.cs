using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        [BindProperty] public ShoppingCartVM ShoppingCartVM { get; set; }
        private readonly IUnitOfWork _unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBaseOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        private decimal GetPriceBaseOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }

        public IActionResult Plus(int cardId)
        {
            var cardFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == cardId);
            cardFromDb.Count += 1;
            _unitOfWork.ShoppingCartRepository.Update(cardFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cardId)
        {
            var cardFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == cardId);
            if (cardFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCartRepository.Delete(cardFromDb);
            }
            else
            {
                cardFromDb.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(cardFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cardId)
        {
            var cardFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == cardId);
            _unitOfWork.ShoppingCartRepository.Delete(cardFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser =
                _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StressAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StressAddress;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PortalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM.ShoppingCartList =
                _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationId = userId;

            ApplicationUser user = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                // regular customer account 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // company account
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDeTayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _unitOfWork.OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }

            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7254/";
                // regular customer account and need to process payment
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderComfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long?)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id,
                    session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderComfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderComfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == id,
                includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDeTayedPayment)
            {
                // this is an order by customer 
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartRepository
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationId).ToList();
            _unitOfWork.ShoppingCartRepository.DeleteRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
    }
}