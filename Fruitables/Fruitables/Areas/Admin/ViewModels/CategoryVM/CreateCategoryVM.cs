using System.ComponentModel.DataAnnotations;

namespace Fruitables.Areas.Admin.ViewModels.CategoryVM
{
    public class CreateCategoryVM
    {
        [Required(ErrorMessage = "Bos ola bilmez")]
        
        public string Name { get; set; }
    }
}
