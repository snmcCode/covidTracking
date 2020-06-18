using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd
{
    public class Utils
    {        
        public static string API_URL = "https://snmtrackingapi.azurewebsites.net/api";
        public static string REGISTER_FUNCTION = "user/put";
        public static string CODE = "gwQgJAGWvdkvykPEUcFhhkgJ2naRFAUqK029Rd/MruwA9APMgiOPug==";
        public static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Byte[] GenerateQRCodeBitmapByteArray(string txtQRCode)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            return BitmapToBytesCode(qrCodeImage);
        }
    }
}
