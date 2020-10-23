using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.VisualBasic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour Image.xaml
    /// </summary>
    public partial class Image : Page
    {

        #region FONCTIONS
            public void RefreshImages()
            {
                // Récupération du QrCode 
                var registre = (Models.Registre)cbRegistre.SelectedItem;
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                var image = qrcode.Draw(registre.QrCode, 40);
                var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
                QrCodeImage.Source = imageConvertie.Source;

                ImageView imageView = new ImageView();
                List<ImageView> listImageViews = new List<ImageView>();                
                listImageViews = imageView.GetViewsList(registre).ToList();
                dgImage.ItemsSource = listImageViews;

                //CheckImageList(listImageViews,registre.NombrePage);
            }

            //private void CheckImageList(List<ImageView> imageViews,int NombrePage)
            //{
            //    int page = 1;
            //    //List des numéros de pages manquants
            //    List<int> pageManquant = new List<int>();
                
            //    for (int i=0;i<NombrePage;i++)
            //    {
            //        page = page + i;
            //        foreach(ImageView imageView in imageViews)
            //        {
            //            if(imageView.NumeroScan == page)
                                                        
            //        }
            //    }
            //}
        #endregion

        public Image()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
            //dgImage.ContextMenu = cm;

            ////try
            ////{
            //using (var ct = new DocumatContext())
            //    {
            //        var services = ct.Service.ToList();
            //        cbService.ItemsSource = services;
            //        cbService.DisplayMemberPath = "Nom";
            //        cbService.SelectedValuePath = "ServiceID";
            //        cbService.SelectedIndex = 0;

            //        if (((Models.Service)cbService.SelectedItem) != null)
            //        {

            //            var agents  = ct.Agent.Where(a => a.ServiceID == ((Models.Service)cbService.SelectedItem).ServiceID).ToList();
            //            cbAgent.ItemsSource = agents;
            //            cbAgent.DisplayMemberPath = "Noms";
            //            cbAgent.SelectedValuePath = "AgentID";
            //            cbAgent.SelectedIndex = 0;

            //            if (((Models.Agent)cbAgent.SelectedItem) != null)
            //            {
            //                var versements = ct.Versement.Where(v => v.AgentID == ((Models.Agent)cbAgent.SelectedItem).AgentID).ToList();
            //                cbVersement.ItemsSource = versements;
            //                cbVersement.DisplayMemberPath = "NumeroVers";
            //                cbVersement.SelectedValuePath = "VersementID";
            //                cbVersement.SelectedIndex = 0;

            //                if (((Models.Versement)cbVersement.SelectedItem) != null)
            //                    {
            //                        var registres = ct.Registre.Where(r => r.VersementID == ((Models.Versement)cbVersement.SelectedItem).VersementID).ToList();
            //                        cbRegistre.ItemsSource = registres;
            //                        cbRegistre.DisplayMemberPath = "Numero";
            //                        cbRegistre.SelectedValuePath = "RegistreID";        
            //                        cbRegistre.SelectedIndex = 0;
            //                    }
            //                else
            //                    {
            //                        cbRegistre.ItemsSource = null; 
            //                    }
            //            }
            //        }
            //    }
            //}
            //catch (ArgumentNullException ex){}
            //catch(Exception ex)
            //{
            //    ex.ExceptionCatcher();
            //}
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(dgImage.SelectedItems.Count == 1)
            {
                //ImageView imageView = (ImageView)dgImage.SelectedItem;
                //Models.Registre registre = (Models.Registre)cbRegistre.SelectedItem;
                //ImageViewer imageViewer = new ImageViewer(registre);
                //imageViewer.Show();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un élément !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshImages();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgImage_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {

        }

        private void dgImage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgImage_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //var ligne = e.Row.Item;
            //ImageView view = (ImageView)e.Row.Item;
            //view.NumeroOrdre = e.Row.GetIndex() + 1;
            //e.Row.Item = view;
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            panelInteracBtn.Visibility = Visibility.Visible;
            dgImage.Visibility = Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try 
            //{ 
                using (var ct = new DocumatContext())
                {
                    if(((Models.Service)cbService.SelectedItem) !=  null)
                    {
                        var agents = ct.Agent.ToList();
                        cbAgent.ItemsSource = agents;
                        cbAgent.DisplayMemberPath = "Noms";
                        cbAgent.SelectedValuePath = "AgentID";
                        cbAgent.SelectedIndex = 0;
                    }
                }
            //}
            //catch (ArgumentNullException ex){}
            //catch(Exception ex)
            //{
            //    ex.ExceptionCatcher();
            //}
        }

        private void cbAgent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{ 
                //using (var ct = new DocumatContext())
                //{
                //    if (((Models.Agent)cbAgent.SelectedItem) != null)
                //    {
                //        var versements = ct.Versement.Where(v => v.AgentID == ((Models.Agent)cbAgent.SelectedItem).AgentID).ToList();
                //        cbVersement.ItemsSource = versements;
                //        cbVersement.DisplayMemberPath = "NumeroVers";
                //        cbVersement.SelectedValuePath = "VersementID";
                //        cbVersement.SelectedIndex = 0;
                //    }
                //}
            //}
            //catch (ArgumentNullException ex) { }
            //catch (Exception ex)
            //{
            //    ex.ExceptionCatcher();
            //}
        }

        private void cbVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   
            //try
            //{ 
                using(var ct = new DocumatContext())
                {
                    if (((Models.Versement)cbVersement.SelectedItem) != null)
                    {
                        var registres = ct.Registre.Where(r => r.VersementID == ((Models.Versement)cbVersement.SelectedItem).VersementID).ToList();
                        cbRegistre.ItemsSource = registres;
                        cbRegistre.DisplayMemberPath = "Numero";
                        cbRegistre.SelectedValuePath = "RegistreID";
                        cbRegistre.SelectedIndex = 0;
                }
                else
                    {
                        cbRegistre.ItemsSource = null;
                    }
                }
            //}
            //catch (ArgumentNullException ex) { }
            //catch (Exception ex)
            //{
            //    ex.ExceptionCatcher();
            //}
        }

        private void cbRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(((Models.Registre)cbRegistre.SelectedItem) != null)
            {
                RefreshImages();                
            }
            else
            {
                dgImage.ItemsSource = null;
                QrCodeImage.Source = null;
            }
        }


        private void cbSatutImage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            //if(((MenuItem)sender).Header.ToString() == "Mode Renommer")
            //{
            //    dgImage.IsReadOnly = false;
            //    ((MenuItem)sender).Header = "Désactiver Mode Renommer";
            //    ((MenuItem)sender).Foreground = Brushes.DimGray;
            //}
            //else
            //{
            //    dgImage.IsReadOnly = true;
            //    ((MenuItem)sender).Header = "Mode Renommer";
            //    ((MenuItem)sender).Foreground = Brushes.White;
            //}
        }

        private void dgImage_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //try
            //{
            //    // Récupération du QrCode 
            //    var registre = (Models.Registre)cbRegistre.SelectedItem;
            //    ImageView imageView = new ImageView();

            //    var newName = ((TextBox)e.EditingElement).Text.Trim();
            //    FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(registre.CheminDossier, newName));

            //    int numeroNomScan;
            //    if (newName == "" || fileInfo.Extension == "" || fileInfo.Extension.ToLower() != ".jpg" ||
            //        !Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - 4), out numeroNomScan) || File.Exists(fileInfo.FullName))
            //    {
            //        dgImage.ItemsSource = imageView.GetViewsList(registre).ToList();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ex.ExceptionCatcher();
            //}
        }

        private void dgImage_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
        }

        private void DeleteDataImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgImage.SelectedItems.Count > 0)
                {
                    if(((ImageView)dgImage.SelectedItem).Image != null)
                    {
                        if(MessageBox.Show("Voulez vous vraiment supprimer ?","QUESTION",
                            MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            ImageView imageView = new ImageView();
                            foreach (ImageView item in dgImage.SelectedItems)
                            {
                                imageView.context.Image.Remove(imageView.context.Image.FirstOrDefault( i=> i.ImageID == item.Image.ImageID));                                
                            }
                            imageView.context.SaveChanges();
                            RefreshImages();
                            MessageBox.Show("Image(s) supprimée(s) de la Base de Données", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);                         
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void DeleteFileImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgImage.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez vous vraiment supprimer ?", "QUESTION",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        foreach (ImageView imageView in dgImage.SelectedItems)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(imageView.CheminScan);
                        }
                        RefreshImages();
                        MessageBox.Show("Image(s) supprimée(s) du Dossier", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);                            
                    }                    
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void DeleteBoth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgImage.SelectedItems.Count > 0)
                {
                    if (((ImageView)dgImage.SelectedItem).Image != null)
                    {
                        if (MessageBox.Show("Voulez vous vraiment supprimer ?", "QUESTION",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {

                            ImageView imageView = new ImageView();
                            foreach (ImageView item in dgImage.SelectedItems)
                            {
                                imageView.context.Image.Remove(imageView.context.Image.FirstOrDefault(i => i.ImageID == item.Image.ImageID));
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(imageView.Image.CheminImage);
                            }
                            imageView.context.SaveChanges();
                            RefreshImages();
                            MessageBox.Show("Image(s) supprimée(s) ", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}
