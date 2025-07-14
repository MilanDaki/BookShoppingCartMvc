using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            Console.WriteLine("User ID: " + userId);
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged in.");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cart = await GetCart(userId);

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                    await _db.SaveChangesAsync();
                }

                var cartItem = await _db.CartDetails
                    .FirstOrDefaultAsync(a => a.ShoppingCart_Id == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else 
                {
                    var book = _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        ShoppingCart_Id = cart.Id,
                        BookId = bookId,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AddItem: " + ex.Message);
                throw; 
            }
            var cartItemCount = await GetCartItemCount(userId); 
            return cartItemCount;
        }

        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();
            // using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
               
                if (string.IsNullOrEmpty(userId))
                   throw new Exception("User Is Not Logged In");

                var cart = await GetCart(userId);
                if (cart is null)
                {
                   throw new Exception("Cart is Empty");
                }
            
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCart_Id == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                {
                    throw new Exception("Not Items In Cart");
                }
                else if(cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;
                }
                _db.SaveChanges();
               // transaction.Commit();
            }
            catch (Exception)
            {
               
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("invalid userid") ;
            var shoppingCart = await _db.ShoppingCarts
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Book)
                .ThenInclude(a => a.Genre)
                .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(x => x.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId="")
        {
            if(string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _db.ShoppingCarts
                        join cartDetail in _db.CartDetails
                        on cart.Id equals cartDetail.ShoppingCart_Id
                        where cart.UserId == userId
                              select new { cartDetail.Id }
                        ).ToListAsync();
            return data.Count();
        }

        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in.");
                var cart = await GetCart(userId);
                if(cart is null)
                    throw new Exception("Invalid Cart");
                var cartDetail = _db.CartDetails
                    .Where(a => a.ShoppingCart_Id == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new Exception("Cart is empty");
                var pendingRecord = _db.OrderStatuses.FirstOrDefault(a => a.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new Exception("Pending Order Status not found");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,   
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id,
                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                    };
                    _db.OrderDetails.Add(orderDetail);
                }
                _db.SaveChanges();

                //Removing Cart Details
                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}

