using QRCoder;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using LogManager;

namespace UltimaOnlineMacro.Service
{
    public static class QRCodeService
    {
        /// <summary>
        /// Genera un QR code generico
        /// </summary>
        /// <param name="content">Contenuto da codificare</param>
        /// <param name="pixelPerModule">Dimensione pixel per modulo (default: 10)</param>
        /// <param name="eccLevel">Livello di correzione errori (default: L)</param>
        /// <returns>BitmapImage del QR code generato</returns>
        public static BitmapImage GenerateQRCode(string content, int pixelPerModule = 10, QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.L)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("Il contenuto non può essere vuoto", nameof(content));
            }

            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, eccLevel);
                    using (BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(pixelPerModule);

                        // Converti l'array di byte in BitmapImage
                        BitmapImage qrCodeImage = new BitmapImage();
                        using (MemoryStream ms = new MemoryStream(qrCodeBytes))
                        {
                            qrCodeImage.BeginInit();
                            qrCodeImage.CacheOption = BitmapCacheOption.OnLoad;
                            qrCodeImage.StreamSource = ms;
                            qrCodeImage.EndInit();
                        }

                        return qrCodeImage;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nella generazione del QR Code: {ex.Message}", true, true);
                throw;
            }
        }

        /// <summary>
        /// Genera QR code per il nome del personaggio
        /// </summary>
        /// <param name="characterName">Nome del personaggio</param>
        /// <returns>BitmapImage del QR code</returns>
        public static BitmapImage GenerateCharacterQRCode(string characterName)
        {
            return GenerateQRCode(characterName, 10, QRCodeGenerator.ECCLevel.L);
        }

        /// <summary>
        /// Genera QR code per il download dell'app mobile
        /// </summary>
        /// <returns>BitmapImage del QR code per il download</returns>
        public static BitmapImage GenerateAppDownloadQRCode()
        {
            const string downloadUrl = "https://github.com/FabiodAgostino/UOMacroMobile/releases/download/1.2/UoMacroMobileV1.2.apk";
            return GenerateQRCode(downloadUrl, 8, QRCodeGenerator.ECCLevel.M); // Livello M per URL più lunghi
        }

        /// <summary>
        /// Risultato della generazione QR code
        /// </summary>
        public class QRCodeResult
        {
            public BitmapImage Image { get; set; }
            public string InfoText { get; set; }
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// Genera QR code con risultato completo per l'UI
        /// </summary>
        /// <param name="content">Contenuto da codificare</param>
        /// <param name="infoPrefix">Prefisso per il testo informativo</param>
        /// <param name="pixelPerModule">Dimensione pixel per modulo</param>
        /// <returns>Risultato completo per l'aggiornamento UI</returns>
        public static QRCodeResult GenerateQRCodeWithInfo(string content, string infoPrefix = "Codice per", int pixelPerModule = 10)
        {
            var result = new QRCodeResult();

            if (string.IsNullOrEmpty(content))
            {
                result.IsSuccess = false;
                result.InfoText = $"{infoPrefix}: Non disponibile";
                result.ErrorMessage = "Contenuto vuoto";
                return result;
            }

            try
            {
                result.Image = GenerateQRCode(content, pixelPerModule);
                result.InfoText = $"{infoPrefix}: {content}";
                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.InfoText = "Errore nella generazione del QR Code";
                result.ErrorMessage = ex.Message;
                Logger.Loggin($"Errore nella generazione del QR Code: {ex.Message}", true, true);
                return result;
            }
        }
    }
}