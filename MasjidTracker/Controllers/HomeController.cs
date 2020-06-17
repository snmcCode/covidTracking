using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MasjidTracker.FrontEnd.Models;
using QRCoder;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace MasjidTracker.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
 
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Landing()
        {
            return View();
        }
      
        public IActionResult Index(Visitor visitor)
        {
            if (visitor.FirstName == null)
            {
                return RedirectToAction("Landing");
            }
            else if (visitor.QrCode == null)
            {
                string txtQRCode = visitor.FirstName;

                QRCodeGenerator _qrCode = new QRCodeGenerator();
                QRCodeData _qrCodeData = _qrCode.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(_qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                visitor.QrCode = BitmapToBytesCode(qrCodeImage);
            }

            return View(visitor);
        }

        [NonAction]
        private static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        [HttpGet("Registration")]
        public IActionResult Registration()
        {
            return View("Registration");
        }

        [HttpPost("Registration")]
        public IActionResult Registration(Visitor visitor)
        {
            //Register user with web api and  get id for qr code

            //TODO: Remove when implementation for api is complete
            //return RedirectToAction("Index", visitor)

            //return RedirectToAction("Index", visitor);

            //return View();

            return RedirectToAction("Index", visitor);
        }

        public IActionResult Signout()
        {
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public IActionResult Scan(string code)
        //{
        //    Console.WriteLine("Scanned " + code);
        //    return Ok();
        //}


        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

    }
}
