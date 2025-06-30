using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShoppingCartMvcUI.Models
{
    [Table("Books")]
    public class Book
    {      
        public int  Id { get; set; }
         
        [Required]
        [MaxLength(40)]
        public string? BookName { get; set; }
        public double Price { get; set; }
        public string? Image { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }    
        public List<OrderDetail> OrderDetails { get; set; }
        public List<CartDetail> CartDetails { get; set; }


    }
}
