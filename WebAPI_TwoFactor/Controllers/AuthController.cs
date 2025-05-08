using Microsoft.AspNetCore.Mvc;
using WebAPI_TwoFactor.Models;
using WebAPI_TwoFactor.Services;
using WebAPI_TwoFactor.Clases;
using TwoFactorAuthNet;
using TwoFactorAuthNet.Providers.Qr;

namespace WebAPI_TwoFactor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            _authService = new AuthService(configuration);
        }

        [HttpPost("register")]
        public ActionResult<LoginResponse> Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Email y contraseña son requeridos"
                });
            }

            var result = _authService.Register(model);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Email y contraseña son requeridos"
                });
            }

            var result = _authService.Login(model);

            if (result.Success)
            {
                return Ok(result);
            }
            else if (result.RequiresTwoFactor)
            {
                return StatusCode(202, result); // 202 Accepted - Requiere más información
            }
            else
            {
                return Unauthorized(result);
            }
        }

        [HttpPost("login-2fa")]
        public ActionResult<LoginResponse> LoginWith2FA([FromBody] LoginWith2FAModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Code))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Email, contraseña y código 2FA son requeridos"
                });
            }

            var result = _authService.LoginWith2FA(model);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }

        [HttpPost("enable-2fa")]
        public ActionResult<object> Enable2FA([FromBody] EnableTwoFactorModel model)
        {
            var usuarios = new Usuarios(_configuration);

            // Verificar que el usuario existe
            if (string.IsNullOrEmpty(usuarios.GetPasswordHash(model.Email)))
            {
                return BadRequest(new { success = false, message = "Usuario no encontrado" });
            }

            // Verificar si ya tiene 2FA habilitado
            if (usuarios.IsTwoFactorEnabled(model.Email))
            {
                return BadRequest(new { success = false, message = "2FA ya está habilitado para este usuario" });
            }

            // Generar secreto y QR
            var tfa = new TwoFactorAuth("Sitema MFA", 6, 30, Algorithm.SHA256, new ImageChartsQrCodeProvider());
            var secret = tfa.CreateSecret(160);

            // Guardar el secreto temporalmente
            usuarios.SetSecret(model.Email, secret);

            // Generar código QR
            string qrCodeUrl = tfa.GetQrCodeImageAsDataUri(model.Email, secret);

            return Ok(new
            {
                success = true,
                message = "Escanea el código QR y luego confirma con un código",
                qrCode = qrCodeUrl,
                secret = secret // Solo para debug, no enviar en producción
            });
        }

        [HttpGet("get-2fa-qr-image")]
        public FileContentResult Get2FAQRImage(string email)
        {
            var usuarios = new Usuarios(_configuration);
            string secret = usuarios.GetSecret(email);

            if (string.IsNullOrEmpty(secret))
                return null;

            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256, new ImageChartsQrCodeProvider());
            string imgQR = tfa.GetQrCodeImageAsDataUri(email, secret);
            imgQR = imgQR.Replace("data:image/png;base64,", "");
            byte[] picture = Convert.FromBase64String(imgQR);

            return File(picture, "image/png");
        }

        [HttpPost("confirm-2fa")]
        public ActionResult<object> Confirm2FA([FromBody] ConfirmTwoFactorModel model)
        {
            var usuarios = new Usuarios(_configuration);

            // Obtener el secreto temporal
            string secret = usuarios.GetSecret(model.Email);

            if (string.IsNullOrEmpty(secret))
            {
                return BadRequest(new { success = false, message = "No se encontró configuración 2FA pendiente" });
            }

            // Verificar el código
            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256);

            if (tfa.VerifyCode(secret, model.Code))
            {
                // Activar 2FA
                usuarios.EnableTwoFactor(model.Email);
                return Ok(new { success = true, message = "2FA activado exitosamente" });
            }
            else
            {
                return BadRequest(new { success = false, message = "Código inválido" });
            }
        }
    }
}