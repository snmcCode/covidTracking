using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http.Headers;
using Common.Utilities;

namespace FrontEnd
{
    public class Utils
    {

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

        public static async Task<string> GetToken(string targetResource)
        {

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(targetResource);
            return accessToken;


        }

        public static async Task<HttpResponseMessage> CallAPI(string url, string targetResource, ILogger logger, HttpMethod method, HttpContent body)
        {
            Helper helper = new Helper(logger, "CallAPI", null, "Utils/CallAPI");
            var token = await GetToken(targetResource);
            using (var client = new HttpClient())
            {
                using (var httpMessage = new HttpRequestMessage())
                {

                    httpMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpMessage.Method = method;
                    httpMessage.RequestUri = new Uri(url);
                    if (method == HttpMethod.Post || method == HttpMethod.Delete || method == HttpMethod.Put)
                    {
                        httpMessage.Content = body;
                    }

                    var result = await client.SendAsync(httpMessage);

                    if (result.IsSuccessStatusCode)
                    {
                        return result;
                    }
                    else
                    {
                        var reasonPhrase = result.ReasonPhrase;
                        var message = result.RequestMessage;
                        var logMessage = reasonPhrase + "\n" + message;
                        helper.DebugLogger.LogCustomCritical(logMessage);
                        return result;
                    }
                }
            }
        }
    }
}
