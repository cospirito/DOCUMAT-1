using DOCUMAT.Models;
using DOCUMAT.Pages.Registre;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DOCUMAT.Pages.Inventaire
{
    /// <summary>
    /// Logique d'interaction pour Inventaire.xaml
    /// </summary>
    public partial class Inventaire : Page
    {
        public Models.Agent Utilisateur = null;
        public string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<RegistreView> ListRegistreActuel = new List<RegistreView>();

        // Refresh dgRegistre
        public void RefreshVersement()
        {
            try
            {
                VersementView versementView = new VersementView();
                dgVersement.ItemsSource = cbVersement.ItemsSource = versementView.GetVersViewsByService((int)cbService.SelectedValue);
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public void RefreshRegistre()
        {
            try
            {
                //Remplissage de la list de registre
                RegistreView RegistreView = new RegistreView();
                RegistreView.Versement = ((VersementView)cbVersement.SelectedItem).Versement;
                dgRegistre.ItemsSource = RegistreView.GetViewsListByStatus((int)Enumeration.Registre.CREE).ToList();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public Inventaire(Models.Agent user)
        {
            InitializeComponent();
            Utilisateur = user;
            RefreshData.WorkerSupportsCancellation = true;
            RefreshData.DoWork += RefreshData_DoWork;
            RefreshData.RunWorkerCompleted += RefreshData_RunWorkerCompleted;

            //Nouvelle RegionView
            RegionView regionView = new RegionView();
            cbRegion.ItemsSource = regionView.GetViewsList();
            ContextMenu cmVersement = this.FindResource("cmVersement") as ContextMenu;
            dgVersement.ContextMenu = cmVersement;
            ContextMenu cmRegistre = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cmRegistre;
            MenuItem menuItemVersement = (MenuItem)cmVersement.Items.GetItemAt(4);
            MenuItem menuItemRegistre = (MenuItem)cmRegistre.Items.GetItemAt(4);

            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR)
            {
                menuItemVersement.IsEnabled = true;
                menuItemRegistre.IsEnabled = true;
            }
            else
            {
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    menuItemVersement.IsEnabled = false;
                    menuItemRegistre.IsEnabled = true;
                }
                else
                {
                    menuItemVersement.IsEnabled = false;
                    menuItemRegistre.IsEnabled = false;
                }
            }
        }

        private void RefreshData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ListRegistreActuel = e.Result as List<RegistreView>;
            dgRegistre.ItemsSource = ListRegistreActuel;
            PanelLoader.Visibility = Visibility.Collapsed;
            panelInteracBtn.IsEnabled = true;
        }

        private void RefreshData_DoWork(object sender, DoWorkEventArgs e)
        {
            // Définition de la methode de récupération des données 
            try
            {
                //Remplissage de la list de registre
                RegistreView RegistreView = new RegistreView();
                RegistreView.Versement = (Models.Versement)e.Argument;
                e.Result = RegistreView.GetViewsListByStatus((int)Enumeration.Registre.CREE).ToList();
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                ServiceView serviceView = new ServiceView();
                cbService.ItemsSource = serviceView.GetViewsList((int)cbRegion.SelectedValue);
            }
        }

        private void cbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                VersementView versementView = new VersementView();
                dgVersement.ItemsSource = cbVersement.ItemsSource = versementView.GetVersViewsByService((int)cbService.SelectedValue);
            }
            else
            {
                dgVersement.ItemsSource = null;
                cbVersement.ItemsSource = null;
            }
        }

        private void AfficherRegistre_Click(object sender, RoutedEventArgs e)
        {
            if (AfficherRegistre.IsChecked == true)
            {
                if (cbVersement.SelectedItem != null)
                {
                    gridViewPanelVersement.Visibility = Visibility.Collapsed;
                    gridViewPanelRegistre.Visibility = Visibility.Visible;
                    titreGrid.Text = "LISTE DES REGISTRES";
                    cbRegion.IsEnabled = false;
                    cbService.IsEnabled = false;
                    cbVersement.IsEnabled = false;

                    try
                    {
                        RefreshData.RunWorkerAsync(((VersementView)cbVersement.SelectedItem).Versement);
                        PanelLoader.Visibility = Visibility.Visible;
                        panelInteracBtn.IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        //ex.ExceptionCatcher();
                        RefreshData.CancelAsync();
                        PanelLoader.Visibility = Visibility.Collapsed;
                        panelInteracBtn.IsEnabled = true;
                    }
                }
            }
            else
            {
                gridViewPanelVersement.Visibility = Visibility.Visible;
                gridViewPanelRegistre.Visibility = Visibility.Collapsed;
                titreGrid.Text = "LISTE DES VERSEMENTS";
                cbRegion.IsEnabled = true;
                cbService.IsEnabled = true;
                cbVersement.IsEnabled = true;
            }
        }

        private void AddVersement_Click(object sender, RoutedEventArgs e)
        {
            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                if (cbService.SelectedItem != null)
                {
                    this.IsEnabled = false;
                    Versement.FormVersement versement = new Versement.FormVersement((ServiceView)cbService.SelectedItem, this, Utilisateur);
                    versement.Show();
                }
            }
            else
            {
                MessageBox.Show("Vos privilèges ne vous permettes pas d'effectuer cette action !", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditVersement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    if (dgVersement.SelectedItems.Count == 1)
                    {
                        this.IsEnabled = false;
                        Versement.FormVersement versement = new Versement.FormVersement((ServiceView)cbService.SelectedItem, ((VersementView)cbVersement.SelectedItem), this, Utilisateur);
                        versement.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Vos privilèges ne vous permettes pas d'effectuer cette action !", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void DelVersement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    if (dgVersement.SelectedItem != null)
                    {
                        if (MessageBox.Show("Voulez vous vraiment supprimer ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            using (var ct = new DocumatContext())
                            {
                                VersementView versementView1 = new VersementView();
                                List<int> VersementIds = new List<int>();
                                foreach (VersementView versementView in dgVersement.SelectedItems)
                                {
                                    VersementIds.Add(versementView.Versement.VersementID);
                                    ct.Versement.Remove(ct.Versement.FirstOrDefault(v => v.VersementID == versementView.Versement.VersementID));
                                }
                                ct.SaveChanges();

                                // Enregistrement du traitement de l'agent 
                                foreach (int id in VersementIds)
                                {
                                    DocumatContext.AddTraitement(DocumatContext.TbVersement, id, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION, "SUPPRESSION D'UN VERSEMENT");
                                }
                                dgVersement.ItemsSource = cbVersement.ItemsSource = versementView1.GetVersViewsByService((int)cbService.SelectedValue);
                                versementView1.Dispose(); 
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Vos privilèges ne vous permettes pas d'effectuer cette action !", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void AddRegistre_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            FormRegistre formRegistre = new FormRegistre(((VersementView)cbVersement.SelectedItem).Versement, this);
            formRegistre.Show();
        }

        private void EditRegistre_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                using (var ct = new DocumatContext())
                {
                    Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == ((RegistreView)dgRegistre.SelectedItem).Registre.RegistreID);
                    this.IsEnabled = false;
                    FormRegistre formRegistre = new FormRegistre(((VersementView)cbVersement.SelectedItem).Versement, registre, this);
                    formRegistre.Show();
                }
            }
        }

        private void DelRegistre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez-vous vraiment supprimer cet élément ?", "ATTENTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        using (var ct = new DocumatContext())
                        {
                            RegistreView registreView = new RegistreView();
                            List<int> RegistreIds = new List<int>();

                            foreach (RegistreView item in dgRegistre.SelectedItems)
                            {
                                RegistreIds.Add(item.Registre.RegistreID);
                                ct.Registre.Remove(ct.Registre.FirstOrDefault(r => r.RegistreID == item.Registre.RegistreID));
                            }
                            ct.SaveChanges();

                            foreach (int id in RegistreIds)
                            {
                                // Enregistrement du traitement de l'agent 
                                DocumatContext.AddTraitement(DocumatContext.TbRegistre, id, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION);
                            }
                            MessageBox.Show("Suppression éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                            RefreshRegistre(); 
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez sélecctionner au moins un élément ", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            try
            {
                // Chargement de l'image du Qrcode de la ligne
                Zen.Barcode.CodeQrBarcodeDraw codeQr = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                var element = e.DetailsElement.FindName("QrCode");
                RegistreView registre = (RegistreView)e.Row.Item;
                var image = codeQr.Draw(registre.Registre.QrCode, 100);
                var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
                ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
            }
            catch (Exception ex) { }
        }

        private void dgVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbVersement.SelectedItem = (VersementView)dgVersement.SelectedItem;
        }

        private void cbVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgVersement.SelectedItem = (VersementView)cbVersement.SelectedItem;
            if (cbVersement.SelectedItem == null)
            {
                AfficherRegistre.IsEnabled = false;
            }
            else
            {
                AfficherRegistre.IsEnabled = true;
            }
        }

        private void btnAjouterNouveau_Click(object sender, RoutedEventArgs e)
        {
            if (gridViewPanelRegistre.Visibility == Visibility.Visible)
            {
                AddRegistre_Click(null, null);
            }
            else
            {
                AddVersement_Click(null, null);
            }
        }

        private void voirBordereau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgVersement.SelectedItems.Count == 1)
                {
                    Models.Versement versement = ((VersementView)dgVersement.SelectedItem).Versement;
                    Pages.Image.SimpleViewer simpleViewer = new Image.SimpleViewer(Path.Combine(DossierRacine, versement.cheminBordereau));
                    simpleViewer.Show();
                }
            }
            catch (Exception ex)
            {
                if (dgVersement.SelectedItems.Count == 1)
                {
                    MessageBox.Show("Adobe Reader n'est pas Installé sur cette machine pour permettre la lecture du  fichier, le pdf sera ouvert par la lecteuse par défault", "Adobe Reader Introuvable", MessageBoxButton.OK, MessageBoxImage.Information);
                    Models.Versement versement = ((VersementView)dgVersement.SelectedItem).Versement;
                    System.Diagnostics.Process.Start(Path.Combine(DossierRacine, versement.cheminBordereau));
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                int StandardHeight = 800;
                int StandardDgHeight = 450;
                if (e.NewSize.Height < StandardHeight)
                {
                    dgVersement.MaxHeight = StandardDgHeight - ((StandardHeight - e.NewSize.Height) * 2 / 3);
                    dgRegistre.MaxHeight = StandardDgHeight - ((StandardHeight - e.NewSize.Height) * 2 / 3);
                }
                else
                {
                    dgVersement.MaxHeight = StandardDgHeight + ((e.NewSize.Height - StandardHeight) * 2 / 3);
                    dgRegistre.MaxHeight = StandardDgHeight + ((e.NewSize.Height - StandardHeight) * 2 / 3);
                }
            }
        }

        private void AffRegistre_Click(object sender, RoutedEventArgs e)
        {
            AfficherRegistre.IsChecked = true;
            AfficherRegistre_Click(null, null);
        }

        private void ImprimerQrCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    // Affichage de l'impression du code barre
                    Impression.BordereauRegistre bordereauRegistre = new Impression.BordereauRegistre(((RegistreView)dgRegistre.SelectedItem).Registre);
                    bordereauRegistre.Show();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void OuvrirDossierScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                    DirectoryInfo RegistreDossier = new DirectoryInfo(Path.Combine(DossierRacine, registreView.Registre.CheminDossier));
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    var dlg = new CommonOpenFileDialog();
                    dlg.Title = "Dossier de Scan du Registre : " + registreView.Registre.QrCode;
                    dlg.IsFolderPicker = false;
                    //dlg.InitialDirectory = currentDirectory;
                    dlg.AddToMostRecentlyUsedList = false;
                    dlg.AllowNonFileSystemItems = false;
                    //dlg.DefaultDirectory = currentDirectory;
                    //dlg.FileOk += Dlg_FileOk;
                    dlg.EnsureFileExists = true;
                    dlg.EnsurePathExists = true;
                    dlg.EnsureReadOnly = false;
                    dlg.EnsureValidNames = true;
                    dlg.Multiselect = true;
                    dlg.ShowPlacesList = true;
                    dlg.InitialDirectory = RegistreDossier.FullName;

                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok) { }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}
