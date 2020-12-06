using iText.Kernel.Pdf;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace DOCUMAT.Impression
{
    /// <summary>
    /// Logique d'interaction pour BordereauRegistre.xaml
    /// </summary>
    public partial class BordereauRegistre : Window
    {
		string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
		Models.Registre Registre;

		public BordereauRegistre()
        {
            InitializeComponent();
		}

        public BordereauRegistre(Models.Registre registre):this()
        {
            try
            {
                Registre = registre;

                // Chargement du formulaire du code barre
                string[] cheminServices = registre.CheminDossier.Split(new char[2] { '/', '\\' });
                string CheminService = cheminServices[1] + "/" + cheminServices[2];
                Zen.Barcode.Code128BarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                BarCodeRegistre.Source = ConvertDrawingImageToWPFImage(qrcode.Draw(registre.QrCode, 50)).Source;
                BarCodeChemin.Source = ConvertDrawingImageToWPFImage(qrcode.Draw(CheminService, 50)).Source;

                // Remplissage des informations sur le Registre
                txtTitreRegistre.Text = "REGISTRE N° : " + registre.Numero;
                txtCodeRegistre.Text = registre.QrCode;
                txtRegion.Text = registre.Versement.Livraison.Service.Region.Nom.ToString();
                txtService.Text = registre.Versement.Livraison.Service.Nom.ToString();
                txtType.Text = registre.Type;
                txtNumeroDebutDepot.Text = registre.NumeroDepotDebut.ToString();
                txtNumeroFinDepot.Text = registre.NumeroDepotFin.ToString();
                txtNbPageReelle.Text = registre.NombrePage + " / " + registre.NombrePageDeclaree;
                txtDateDebutDepot.Text = registre.DateDepotDebut.ToShortDateString();
                txtDateFinDepot.Text = registre.DateDepotFin.ToShortDateString();
                txtCheminDossierScan.Text = CheminService;

                if (File.Exists(System.IO.Path.Combine(DossierRacine, Registre.CheminDossier, $"Bordereau{registre.QrCode}.xps")))
                {
                    File.Delete(System.IO.Path.Combine(DossierRacine, Registre.CheminDossier, $"Bordereau{registre.QrCode}.xps"));
                }
                var xpsDocument = new XpsDocument(System.IO.Path.Combine(DossierRacine, Registre.CheminDossier, $"Bordereau{registre.QrCode}.xps"), FileAccess.ReadWrite);
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                writer.Write(((IDocumentPaginatorSource)FD).DocumentPaginator);
                Document = xpsDocument.GetFixedDocumentSequence();
                xpsDocument.Close();

                PreviewD.Document = Document;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        public FixedDocumentSequence Document { get; set; }
        
        
        /// <summary>
        ///  Convertir une image de type System.Drawing.Image en System.Controls.Image
        /// </summary>
        private System.Windows.Controls.Image ConvertDrawingImageToWPFImage(System.Drawing.Image gdiImg)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            //convert System.Drawing.Image to WPF image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(gdiImg);
            IntPtr hBitmap = bmp.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            img.Source = WpfBitmap;
            img.Width = 400;
            img.Height = 400;
            img.Stretch = System.Windows.Media.Stretch.Fill;
            return img;
        }

    }
}
