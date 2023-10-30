namespace WebApplication1.Models
{
    public class LoginViewModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string CheckNewPassword { get; set; }
        public user_accounts User_Accounts { get; set; }
    }
}