using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DOCUMAT.Impression
{
    /// <summary>
    /// Logique d'interaction pour QrCode.xaml
    /// </summary>
    public partial class QrCode : Window
    {
        public QrCode()
        {
            InitializeComponent();
        }

        public QrCode(Models.Registre registre):this()
        {
            try
            {
                // Chargement du formulaire du code barre
                string[] cheminServices = registre.CheminDossier.Split(new char[2] { '/', '\\' });
                string CheminService = cheminServices[1] + "/" + cheminServices[2];
                Zen.Barcode.Code128BarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                BarCodeRegistre.Source = ConvertDrawingImageToWPFImage(qrcode.Draw(registre.QrCode,50)).Source;
                tbxCodeRegistre.Text = registre.QrCode;
                BarCodeChemin.Source = ConvertDrawingImageToWPFImage(qrcode.Draw(CheminService, 50)).Source;
                tbxCheminRegistre.Text = CheminService;
                tbxTitreImpression.Text = "CODE ET CHEMIN DU REGISTRE N° VOLUME : " + registre.Numero;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
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

        public void btnImprimer_Click(object sender, RoutedEventArgs e)
        {
            //Résolution 1024 * 768

            var printDlg = new PrintDialog();
            //this.Visibility = Visibility.Hidden;
            if (printDlg.ShowDialog() == true)
            {
                //this.Height = 1024;
                //this.Width = 768;
                //QrCodeImage.Margin = new Thickness(20);
                //parentImage.HorizontalAlignment = HorizontalAlignment.Left;
                //this.WindowState = WindowState.Maximized;
                //this.Visibility = Visibility.Visible;
                // Impression du contenu de l'écran.
                btnImprimer.Visibility = Visibility.Hidden;
                printDlg.PrintVisual(this, "IMPRESSION DU QRCODE");
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }

}
