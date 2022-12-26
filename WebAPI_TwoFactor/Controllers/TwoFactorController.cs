using Microsoft.AspNetCore.Mvc;
using TwoFactorAuthNet;
using TwoFactorAuthNet.Providers.Qr;
using WebAPI_TwoFactor.Clases;

namespace WebAPI_TwoFactor.Controllers
{
    public class TwoFactorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet, Route("GetQRCode")]
        public string GetQRCode(string email)
        {
            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256, new ImageChartsQrCodeProvider());
            var secret = tfa.CreateSecret(160);

            Usuarios usu = new Usuarios();
            usu.SetSecret(email, secret);
            usu = null;

            string imgQR = tfa.GetQrCodeImageAsDataUri(email, secret);
            string imgHTML = $"<img src='{imgQR}'>";
            return imgHTML;
        }

        [HttpGet, Route("GetQRCodeAsImage")]
        public FileContentResult GetQRCodeAsImage(string email)
        {
            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256, new ImageChartsQrCodeProvider());
            var secret = tfa.CreateSecret(160);

            Usuarios usu = new Usuarios();
            usu.SetSecret(email, secret);
            usu = null;

            string imgQR = tfa.GetQrCodeImageAsDataUri(email, secret);
            imgQR = imgQR.Replace("data:image/png;base64,", "");
            byte[] picture = Convert.FromBase64String(imgQR);
            return File(picture, "image/png");
        }

        [HttpGet, Route("ValidarQRCode")]
        public bool ValidarCodigo(string email, string code)
        {
            Usuarios usu = new Usuarios();
            string secret = usu.GetSecret(email);
            usu = null;

            var tfa = new TwoFactorAuth("ManuelToscanoDEV", 6, 30, Algorithm.SHA256);
            return tfa.VerifyCode(secret, code);
        }


    }
}
