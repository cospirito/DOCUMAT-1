using System;
using System.Collections.Generic;
using System.IO;
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

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour SimpleViewer.xaml
    /// </summary>
    public partial class SimpleViewer : Window
    {
		string CheminImage = "";

		public SimpleViewer(string CheminImage)
        {
            try
            {
                InitializeComponent();

                this.CheminImage = CheminImage;
            }
            catch (Exception ex)
            {            
                 MessageBox.Show("Adobe Reader n'est pas Installé sur cette machine pour permettre la lecture du  fichier, le pdf sera ouvert par la lecteuse par défault", "Adobe Reader Introuvable", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

		/// <summary>
		///  Lance la visionneuse d'image
		/// </summary>
		/// <param name="ImagePath"> Chemin de l'image à afficher </param>
		private void viewImage(string ImagePath)
		{
            FileInfo fileInfo = new FileInfo(ImagePath);
            try
            {
                if(fileInfo.Exists)
                {
                    if(fileInfo.Extension.ToUpper() == ".PDF")
                    {
                        PdfViewerPanel.Visibility = Visibility.Visible;
                        PreviewD.Visibility = Visibility.Collapsed;
                        PdfViewer.axAcroPDF1.LoadFile(ImagePath);
                    }
                    else
                    {
                        PreviewD.Visibility = Visibility.Visible;
                        PdfViewerPanel.Visibility = Visibility.Collapsed;
                        System.Windows.Controls.Image Image;
                        FixedDocument FixedDocument;
                        FixedPage FixedPage;
                        PageContent PageContent;

                        ImageSource img = BitmapFrame.Create(new Uri(ImagePath), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                        Image = new System.Windows.Controls.Image();
                        Image.Source = img;
                        FixedPage = new FixedPage();
                        FixedPage.Height = img.Height;
                        FixedPage.Width = img.Width;
                        FixedPage.Children.Add(Image);
                        PageContent = new PageContent();
                        PageContent.Child = FixedPage;
                        FixedDocument = new FixedDocument();
                        FixedDocument.Pages.Add(PageContent);
                        PreviewD.Document = FixedDocument;
                    }
                }
                else
                {
                    MessageBox.Show("Fichier Introuvable !!!","Fichier Introuvable",MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                if (fileInfo.Extension.ToUpper() == ".PDF")
                {
                    try
                    {
                        System.Diagnostics.Process.Start(ImagePath);
                    }
                    catch (Exception exiner)
                    {
                        exiner.ExceptionCatcher();
                    }
                    finally { this.Close(); }
                }
                else
                {
                    ex.ExceptionCatcher();
                }
            }
		}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			try
			{
				if (File.Exists(CheminImage))
				{
					viewImage(CheminImage);
				}
				else
				{
					throw new Exception("L'image n'est pas trouvable, vérifier le chemin !");
				}
			}
			catch (Exception ex)
			{
				ex.ExceptionCatcher();
				this.Close();
			}
		}

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PdfViewer.axAcroPDF1.Height = (int)PdfViewerPanel.ActualHeight;
            PdfViewer.axAcroPDF1.Width = (int)PdfViewerPanel.ActualWidth;
        }
    }
}
