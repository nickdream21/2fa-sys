using Microsoft.AspNetCore.Mvc;
using TwoFactorAuthNet;
using TwoFactorAuthNet.Providers.Qr;
using WebAPI_TwoFactor.Clases;
using Microsoft.Extensions.Configuration;

namespace WebAPI_TwoFactor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TwoFactorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetQRCode")]
        public ActionResult<string> GetQRCode(string email)
        {
            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256, new ImageChartsQrCodeProvider());
            var secret = tfa.CreateSecret(160);

            Usuarios usu = new Usuarios(_configuration);
            usu.SetSecret(email, secret);

            string imgQR = tfa.GetQrCodeImageAsDataUri(email, secret);
            string imgHTML = $"<img src='{imgQR}'>";
            return Ok(imgHTML);
        }

        [HttpGet("GetQRCodeAsImage")]
        public FileContentResult GetQRCodeAsImage(string email)
        {
            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256, new ImageChartsQrCodeProvider());
            var secret = tfa.CreateSecret(160);

            Usuarios usu = new Usuarios(_configuration);
            usu.SetSecret(email, secret);

            string imgQR = tfa.GetQrCodeImageAsDataUri(email, secret);
            imgQR = imgQR.Replace("data:image/png;base64,", "");
            byte[] picture = Convert.FromBase64String(imgQR);
            return File(picture, "image/png");
        }

        [HttpGet("ValidarCodigo")]
        public ActionResult<bool> ValidarCodigo(string email, string code)
        {
            Usuarios usu = new Usuarios(_configuration);
            string secret = usu.GetSecret(email);

            if (string.IsNullOrEmpty(secret))
            {
                return BadRequest("Usuario no encontrado o sin configuración 2FA");
            }

            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256);
            return Ok(tfa.VerifyCode(secret, code));
        }
    }
}