namespace BookShoppingCartMvcUI.Repositories
{
    public interface IUserOrderRespository
    {    
       Task<IEnumerable<Order>> UserOrders();

    }
}