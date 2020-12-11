using DOCUMAT.Models;
using DOCUMAT.Pages.Registre;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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

        // Refresh dgRegistre
        public void RefreshVersement()
        {
            try
            {
                VersementView versementView = new VersementView();
                dgVersement.ItemsSource = cbVersement.ItemsSource = versementView.GetVersViewsByService((int)cbService.SelectedValue);
                cbVersement.SelectedIndex = 0;
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
                dgRegistre.ItemsSource = RegistreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.CREE);
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

            //Nouvelle RegionView
            RegionView regionView = new RegionView();
            cbRegion.ItemsSource = regionView.GetViewsList();
            ContextMenu cmVersement = this.FindResource("cmVersement") as ContextMenu;
            dgVersement.ContextMenu = cmVersement;
            ContextMenu cmRegistre = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cmRegistre;
            MenuItem menuItemVersement = (MenuItem)cmVersement.Items.GetItemAt(4);
            MenuItem menuItemRegistre = (MenuItem)cmRegistre.Items.GetItemAt(2);

            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR)
            {
                menuItemVersement.IsEnabled = true;
                menuItemRegistre.IsEnabled = true;
            }
            else
            {
                if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
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

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count != 0 )
            {
                ServiceView serviceView = new ServiceView();
                cbService.ItemsSource = serviceView.GetViewsList((int)cbRegion.SelectedValue);
                cbService.SelectedIndex = 0;
            }           
        }

        private void cbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count != 0)
            {
                VersementView versementView = new VersementView();
                dgVersement.ItemsSource = cbVersement.ItemsSource = versementView.GetVersViewsByService((int)cbService.SelectedValue);
                cbVersement.SelectedIndex = 0;
            }
            else
            {
                dgVersement.ItemsSource = null;
                cbVersement.ItemsSource = null;
            }
        }

        private void AfficherRegistre_Click(object sender, RoutedEventArgs e)
        {
            if(AfficherRegistre.IsChecked == true)
            {
                if(cbVersement.SelectedItem != null)
                {
                    gridViewPanelVersement.Visibility = Visibility.Collapsed;
                    gridViewPanelRegistre.Visibility = Visibility.Visible;
                    titreGrid.Text = "LISTE DES REGISTRES";
                    cbRegion.IsEnabled = false;
                    cbService.IsEnabled = false;
                    cbVersement.IsEnabled = false;
                    RefreshRegistre();
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
            if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                if (cbService.SelectedItem != null)
                {
                    this.IsEnabled = false;
                    Versement.FormVersement versement = new Versement.FormVersement((ServiceView)cbService.SelectedItem,this,Utilisateur);
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
                            VersementView versementView1 = new VersementView();
                            List<int> VersementIds = new List<int>();
                            foreach (VersementView versementView in dgVersement.SelectedItems)
                            {
                                VersementIds.Add(versementView.Versement.VersementID);
                                versementView1.context.Versement.Remove(versementView1.context.Versement.FirstOrDefault(v => v.VersementID == versementView.Versement.VersementID));
                            }
                            versementView1.context.SaveChanges();

                            // Enregistrement du traitement de l'agent 
                            foreach (int id in VersementIds)
                            {
                                DocumatContext.AddTraitement(DocumatContext.TbVersement, id, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION);
                            }
                            dgVersement.ItemsSource = cbVersement.ItemsSource = versementView1.GetVersViewsByService((int)cbService.SelectedValue);
                            versementView1.Dispose();
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
            FormRegistre formRegistre = new FormRegistre((VersementView)cbVersement.SelectedItem,this);            
            formRegistre.Show();
        }

        private void EditRegistre_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                this.IsEnabled = false;
                FormRegistre formRegistre = new FormRegistre((VersementView)cbVersement.SelectedItem,(RegistreView)dgRegistre.SelectedItem, this);
                formRegistre.Show();
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
                        RegistreView registreView = new RegistreView();
                        List<int> RegistreIds = new List<int>();

                        foreach (RegistreView item in dgRegistre.SelectedItems)
                        {
                            RegistreIds.Add(item.Registre.RegistreID);
                            registreView.context.Registre.Remove(registreView.context.Registre.FirstOrDefault(r => r.RegistreID == item.Registre.RegistreID));
                        }
                        registreView.context.SaveChanges();

                        foreach(int id in RegistreIds)
                        {
                            // Enregistrement du traitement de l'agent 
                            DocumatContext.AddTraitement(DocumatContext.TbRegistre, id,Utilisateur.AgentID,(int)Enumeration.TypeTraitement.SUPPRESSION);
                        }
                        MessageBox.Show("Suppression éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshRegistre();
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

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            RegistreView view = (RegistreView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
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
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void dgVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbVersement.SelectedItem = (VersementView)dgVersement.SelectedItem;            
        }

        private void cbVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgVersement.SelectedItem = (VersementView)cbVersement.SelectedItem;
            if(cbVersement.SelectedItem == null)
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
            if(gridViewPanelRegistre.Visibility == Visibility.Visible)
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
            if(dgVersement.SelectedItems.Count == 1)
            {
                Models.Versement versement = ((VersementView)dgVersement.SelectedItem).Versement;
                Pages.Image.SimpleViewer simpleViewer = new Image.SimpleViewer(Path.Combine(DossierRacine,versement.cheminBordereau));
                simpleViewer.Show();
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
                    dgVersement.MaxHeight = StandardDgHeight - ((StandardHeight - e.NewSize.Height)*2/3);
                    dgRegistre.MaxHeight = StandardDgHeight - ((StandardHeight - e.NewSize.Height)*2/3);
                }
                else
                {
                    dgVersement.MaxHeight = StandardDgHeight + ((e.NewSize.Height - StandardHeight)*2/3);
                    dgRegistre.MaxHeight = StandardDgHeight + ((e.NewSize.Height - StandardHeight)*2/3);
                }
            }
        }

        private void AffRegistre_Click(object sender, RoutedEventArgs e)
        {
            AfficherRegistre.IsChecked = true;
            AfficherRegistre_Click(null, null);
        }
    }
}
