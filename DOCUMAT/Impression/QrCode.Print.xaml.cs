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
        string QrCodePrint ="";
        public QrCode()
        {
            InitializeComponent();
        }

        public QrCode(string QrCode)
        {
            InitializeComponent();
            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            QrCodeImage.Source = ConvertDrawingImageToWPFImage(qrcode.Draw(QrCode, 40)).Source;
            this.QrCodePrint = QrCode;
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

        private void btnImprimer_Click(object sender, RoutedEventArgs e)
        {
            //Résolution 1024 * 768

            var printDlg = new PrintDialog();
            this.Visibility = Visibility.Hidden;
            if (printDlg.ShowDialog() == true)
            {           
                //btnImprimer.Visibility = Visibility.Hidden;
                //this.Height = 1024;
                //this.Width = 768;
                //QrCodeImage.Margin = new Thickness(20);
                //parentImage.HorizontalAlignment = HorizontalAlignment.Left;
                //this.WindowState = WindowState.Maximized;
                //this.Visibility = Visibility.Visible;
                // Impression du contenu de l'écran.
                printDlg.PrintVisual(QrCodeImage, "IMPRESSION DU QRCODE");
            }
        }
    }

}
