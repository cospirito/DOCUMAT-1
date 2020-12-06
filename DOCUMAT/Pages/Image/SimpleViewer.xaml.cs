﻿using System;
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
        public SimpleViewer()
        {
            InitializeComponent();
		}

		public SimpleViewer(string CheminImage):this()
        {
			this.CheminImage = CheminImage;
        }

		/// <summary>
		///  Lance la visionneuse d'image
		/// </summary>
		/// <param name="ImagePath"> Chemin de l'image à afficher </param>
		private void viewImage(string ImagePath)
		{
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
    }
}
