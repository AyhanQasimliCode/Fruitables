using System.ComponentModel.DataAnnotations;

namespace Fruitables.Areas.Admin.ViewModels.CategoryVM
{
    public class UpdateCategoryVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bos ola bilmez")]
        public string Name { get; set; }
    }
}
