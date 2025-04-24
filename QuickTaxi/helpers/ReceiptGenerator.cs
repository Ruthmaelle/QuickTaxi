using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QuickTaxi.Models;
using System;
using System.IO;
using System.Reflection;

namespace QuickTaxi.helpers
{
    public class ReceiptGenerator
    {
        public static byte[] GeneratePdf(Ride ride)
        {
            using (var ms = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
                var fontNormal = new XFont("Arial", 12, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 12, XFontStyle.Bold);

                double y = 40;

                // Logo (chemin local ou embarqué)
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "quicktaxi_logo.png");
                if (File.Exists(logoPath))
                {
                    using var logoStream = File.OpenRead(logoPath);
                    var image = XImage.FromStream(() => logoStream);
                    gfx.DrawImage(image, 20, y, 100, 40);
                    y += 60;
                }

                // Titre
                gfx.DrawString("Reçu de paiement - QuickTaxi", fontTitle, XBrushes.Black, new XRect(0, y, page.Width, 40), XStringFormats.TopCenter);
                y += 50;

                // Infos course
                gfx.DrawString("🗓 Date de la course : " + ride.RideDate.ToLocalTime().ToString("dd MMM yyyy à HH:mm"), fontNormal, XBrushes.Black, 40, y); y += 20;
                gfx.DrawString("📍 Adresse de départ : " + ride.PickupAddress, fontNormal, XBrushes.Black, 40, y); y += 20;
                gfx.DrawString("🏁 Adresse d'arrivée : " + ride.DestinationAddress, fontNormal, XBrushes.Black, 40, y); y += 20;
                gfx.DrawString("📏 Distance : " + ride.DistanceKm.ToString("F2") + " km", fontNormal, XBrushes.Black, 40, y); y += 30;

                // Infos passager
                gfx.DrawString("👤 Passager : " + ride.Passenger?.FirstName + " " + ride.Passenger?.LastName, fontNormal, XBrushes.Black, 40, y); y += 20;
                gfx.DrawString("📧 Email : " + ride.Passenger?.Email, fontNormal, XBrushes.Black, 40, y); y += 30;

                // Infos chauffeur
                gfx.DrawString("🚖 Chauffeur : " + ride.Driver?.FirstName + " " + ride.Driver?.LastName, fontNormal, XBrushes.Black, 40, y); y += 30;
                

                // Paiement
                gfx.DrawString("💰 Prix estimé : " + ride.EstimatedPrice.ToString("C"), fontBold, XBrushes.Black, 40, y); y += 20;
                decimal taxes = ride.EstimatedPrice * 0.15m;
                gfx.DrawString("📊 Taxes (15%) : " + taxes.ToString("C"), fontBold, XBrushes.Black, 40, y); y += 20;
                decimal total = ride.EstimatedPrice + taxes;
                gfx.DrawString("✅ Total payé : " + total.ToString("C"), fontBold, XBrushes.Black, 40, y); y += 30;

                // Pied de page
                gfx.DrawString("Merci d'avoir utilisé QuickTaxi !", fontNormal, XBrushes.Black, 40, y);

                document.Save(ms);
                return ms.ToArray();
            }
        }
    }
}