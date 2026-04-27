namespace MyMvcApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? UserName { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}