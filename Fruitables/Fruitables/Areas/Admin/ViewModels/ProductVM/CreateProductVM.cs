using Fruitables.Models;
using System.ComponentModel.DataAnnotations;

namespace Fruitables.Areas.Admin.ViewModels.ProductVM
{
    public class CreateProductVM
    {
        [Required(ErrorMessage = "Bos ola bilmez")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Bos ola bilmez")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Bos ola bilmez")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Bos ola bilmez")]
        public int CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();
        [Required(ErrorMessage = "En azi 1 tag secilmelidir")]
        public List<int> TagIds { get; set; } = new();

        public List<Tag> Tags { get; set; } = new();

        [Required(ErrorMessage = "Bos ola bilmez")]
        public IFormFile Image { get; set; } = null!;
    }
}
