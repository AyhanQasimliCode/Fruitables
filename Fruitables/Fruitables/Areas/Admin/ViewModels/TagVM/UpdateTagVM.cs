using System.ComponentModel.DataAnnotations;

namespace Fruitables.Areas.Admin.ViewModels.TagVM
{
    public class UpdateTagVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bos ola bilmez")]
        public string Name { get; set; }
    }
}
