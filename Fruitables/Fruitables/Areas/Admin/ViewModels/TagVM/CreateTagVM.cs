using System.ComponentModel.DataAnnotations;

namespace Fruitables.Areas.Admin.ViewModels.TagVM
{
    public class CreateTagVM
    {
        [Required(ErrorMessage = "Bos ola bilmez")]

        public string Name { get; set; }
    }
}
