using System.ComponentModel.DataAnnotations;

namespace CineScore.Models
{
    public class UserProfileEditViewModel
    {
        [Required]
        [Display(Name = "Uporabniško ime")]
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "E-pošta")]
        public string Email { get; set; } = "";
    }
}
