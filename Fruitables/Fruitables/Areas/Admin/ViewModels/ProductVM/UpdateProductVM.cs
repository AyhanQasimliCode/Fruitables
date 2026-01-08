using Fruitables.Models;
using System.ComponentModel.DataAnnotations;

namespace Fruitables.Areas.Admin.ViewModels.ProductVM
{
    public class UpdateProductVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bos ola bilmez")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Bos ola bilmez")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Bos ola bilmez")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Bos ola bilmez")]
        public int CategoryId { get; set; }

        public string ExistingImage { get; set; } = null!;

        public IFormFile? Image { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();
        public List<int> TagIds { get; set; } = new();

        public List<Tag> Tags { get; set; } = new();
    }

}
