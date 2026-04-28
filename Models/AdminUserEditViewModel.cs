using System.ComponentModel.DataAnnotations;

namespace CineScore.Models
{
    public class AdminUserEditViewModel
    {
        public string Id { get; set; } = "";

        [Required]
        [Display(Name = "Uporabniško ime")]
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "E-pošta")]
        public string Email { get; set; } = "";

        [Required]
        [Display(Name = "Vloga")]
        public string Role { get; set; } = "User";
    }
}
