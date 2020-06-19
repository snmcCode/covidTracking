﻿using QRCoder;
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
        public static string RETRIEVE_USER_API_URL = "https://snmtrackingapi.azurewebsites.net/api/user/{0}?code=K80O15HVIhCZ4V9jpX9YbAuLZT0vLU0W1EIdL30sZx5/S/Vrts9SkQ==";
        public static string REGISTER_API_URL = "https://snmtrackingapi.azurewebsites.net/api/user/put?code=gwQgJAGWvdkvykPEUcFhhkgJ2naRFAUqK029Rd/MruwA9APMgiOPug==";
     
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
