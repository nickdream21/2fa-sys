using System.Security.Cryptography;
using System.Text;
using TwoFactorAuthNet;
using WebAPI_TwoFactor.Clases;
using WebAPI_TwoFactor.Models;

namespace WebAPI_TwoFactor.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        public LoginResponse Register(RegisterModel model)
        {
            var usuarios = new Usuarios(_configuration);

            // Verificar si el usuario ya existe
            if (!string.IsNullOrEmpty(usuarios.GetPasswordHash(model.Email)))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "El usuario ya existe"
                };
            }

            // Hash de la contraseña
            var passwordHash = HashPassword(model.Password);

            // Crear usuario
            usuarios.CreateUser(model.Email, passwordHash);

            return new LoginResponse
            {
                Success = true,
                Message = "Usuario registrado exitosamente"
            };
        }

        public LoginResponse Login(LoginModel model)
        {
            var usuarios = new Usuarios(_configuration);

            // Obtener hash de la contraseña almacenada
            var storedHash = usuarios.GetPasswordHash(model.Email);

            if (string.IsNullOrEmpty(storedHash))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos"
                };
            }

            // Verificar contraseña
            if (!VerifyPassword(model.Password, storedHash))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos"
                };
            }

            // Verificar si tiene 2FA habilitado
            if (usuarios.IsTwoFactorEnabled(model.Email))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Se requiere código 2FA",
                    RequiresTwoFactor = true
                };
            }

            return new LoginResponse
            {
                Success = true,
                Message = "Login exitoso",
                Token = "token_temporal" // Aquí generarías un JWT real
            };
        }

        public LoginResponse LoginWith2FA(LoginWith2FAModel model)
        {
            var usuarios = new Usuarios(_configuration);

            // Primero verificar email y contraseña
            var loginResult = Login(new LoginModel
            {
                Email = model.Email,
                Password = model.Password
            });

            if (!loginResult.RequiresTwoFactor && !loginResult.Success)
            {
                return loginResult;
            }

            // Verificar código 2FA
            var secret = usuarios.GetSecret(model.Email);
            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256);

            if (!tfa.VerifyCode(secret, model.Code))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Código 2FA inválido"
                };
            }

            return new LoginResponse
            {
                Success = true,
                Message = "Login exitoso con 2FA",
                Token = "token_temporal" // Aquí generarías un JWT real
            };
        }
    }
}