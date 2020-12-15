using DOCUMAT.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
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
            Registre = registre;           
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
            img.Stretch = System.Windows.Media.Stretch.None;
            return img;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Chargement du formulaire du code barre
                string[] cheminServices = Registre.CheminDossier.Split(new char[2] { '/', '\\' });
                string CheminService = cheminServices[1] + "/" + cheminServices[2];

                Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                Zen.Barcode.Code128BarcodeDraw barcodeChemin = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                var barcodeImage = barcode.Draw(Registre.QrCode, 50);
                var barcodeCheminImage = barcodeChemin.Draw(CheminService, 50);
                string barcodeCheminImageString = (Path.Combine(DossierRacine, Registre.CheminDossier, "CodeBarRegistreChemin.bmp"));
                string barcodeImageString = (Path.Combine(DossierRacine, Registre.CheminDossier, "CodeBarRegistre.bmp"));

                if (!File.Exists(barcodeImageString))
                {
                    barcodeImage.Save(barcodeImageString, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                if (!File.Exists(barcodeCheminImageString))
                {
                    barcodeCheminImage.Save(barcodeCheminImageString, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                BarCodeRegistre.Source = new BitmapImage(new Uri(barcodeImageString), new System.Net.Cache.RequestCachePolicy());
                BarCodeChemin.Source = new BitmapImage(new Uri(barcodeCheminImageString), new System.Net.Cache.RequestCachePolicy());

                // Remplissage des informations sur le Registre
                txtTitreRegistre.Text = "REGISTRE N° : " + Registre.Numero;
                txtCodeRegistre.Text = Registre.QrCode;
                using(var ct = new DocumatContext())
                {
                // Récupération du Service 
                    var Versement = ct.Versement.FirstOrDefault(v => v.VersementID == Registre.VersementID);
                    var Livraison = ct.Livraison.FirstOrDefault(l=>l.LivraisonID == Versement.LivraisonID);
                    var Service = ct.Service.FirstOrDefault(s => s.ServiceID == Livraison.ServiceID);
                    txtService.Text = Service.NomComplet;
                    var Region = ct.Region.FirstOrDefault(re => re.RegionID == Service.RegionID);
                    txtRegion.Text = Region.Nom;
                }
                txtType.Text = Registre.Type;
                txtNumeroDebutDepot.Text = Registre.NumeroDepotDebut.ToString();
                txtNumeroFinDepot.Text = Registre.NumeroDepotFin.ToString();
                txtNbPageReelle.Text = Registre.NombrePage + "(pages) / " + Registre.NombrePageDeclaree + "(feuilles)";
                txtDateDebutDepot.Text = Registre.DateDepotDebut.ToShortDateString();
                txtDateFinDepot.Text = Registre.DateDepotFin.ToShortDateString();
                txtCheminDossierScan.Text = CheminService;

                string BordereauRegistre = System.IO.Path.Combine(DossierRacine,Registre.CheminDossier, $"Bordereau{Registre.QrCode}.xps");
                var xpsDocument = new XpsDocument(BordereauRegistre, FileAccess.ReadWrite);

                try
                {
                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                    writer.Write(((IDocumentPaginatorSource)FD).DocumentPaginator);
                }
                catch (Exception ein) { };

                Document = xpsDocument.GetFixedDocumentSequence();
                xpsDocument.Close();
                PreviewD.Document = Document;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                this.Close();
            }
        }
    }
}
