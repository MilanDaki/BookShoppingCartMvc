using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartrepo;

        public CartController(ICartRepository cartrepo)
        {
            _cartrepo = cartrepo;
        }
        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _cartrepo.AddItem(bookId, qty);
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var cartCount = await _cartrepo.RemoveItem(bookId);
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartrepo.GetUserCart();
            return View(cart);
        }
        public async Task<IActionResult> GetTotaItemInCart()
        {
            var cartItem = await _cartrepo.GetCartItemCount();
            return Ok(cartItem);
        }
        public async Task<IActionResult> GetCartItemCount()
        {
           int cartItem = await _cartrepo.GetCartItemCount();
            return Ok(cartItem);
        }

        public IActionResult Checkout()
        {
            return View();
        }
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if(!ModelState.IsValid)
                return View(model);
            bool isCheckout = await _cartrepo.DoCheckout(model);
            if (!isCheckout)
                return RedirectToAction(nameof(OrderFailure));
            return RedirectToAction(nameof(OrderSuccess));
        }

        public async Task<IActionResult> OrderSuccess()
        {
            return View();
        }
        public async Task<IActionResult> OrderFailure()
        {
            return View();
        }
    }
}
