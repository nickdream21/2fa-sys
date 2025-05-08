namespace WebAPI_TwoFactor.Models
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginWith2FAModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
    }

    public class ConfirmTwoFactorModel  // Renombrar Enable2FAModel a esto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string Token { get; set; } // Para JWT en el futuro
    }

    public class EnableTwoFactorModel
    {
        public string Email { get; set; }
    }

}
