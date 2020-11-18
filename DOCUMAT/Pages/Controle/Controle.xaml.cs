using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DOCUMAT.Pages.Controle
{
    /// <summary>
    /// Logique d'interaction pour Controle.xaml
    /// </summary>
    public partial class Controle : Page
    {
        public int CurrentPhase = 1;

        public Models.Agent Utilisateur;

        #region FONCTIONS
        public void RefreshRegistrePhase1()
        {
            try
            {
                if (cbChoixunite.SelectedItem != null)
                {
                    // Liste des Unités de la Tranche
                    Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                    //Remplissage de la list de registre
                    //Les registres de type Phase 1 sont les registre nouvellement indexée 
                    //Devra être modifié pour empêcher l'affichage des régistres déja attribués
                    RegistreView registreView = new RegistreView();
                    List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.INDEXE && r.Registre.ID_Unite == unite.UniteID).ToList();
                    dgRegistre.ItemsSource = registreViews;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public void RefreshRegistrePhase2()
        {
            try
            {
                if (cbChoixunite.SelectedItem != null)
                {
                    // Liste des Unités de la Tranche
                    Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                    //Remplissage de la list de registre
                    //Les registres de type Phase 1 sont les registre nouvellement indexée 
                    //Devra être modifié pour empêcher l'affichage des régistres déja attribués
                    RegistreView registreView = new RegistreView();
                    List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE2 && r.Registre.ID_Unite == unite.UniteID).ToList();
                    dgRegistre.ItemsSource = registreViews;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

        public Controle()
        {
            InitializeComponent();
        }

        public Controle(Models.Agent user):this()
        {
            Utilisateur = user;
        }

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {            
            ((RegistreView)e.Row.Item).NumeroOrdre = e.Row.GetIndex() + 1;
        }

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            try
            {
                // Chargement de l'image du Qrcode de la ligne
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                var element = e.DetailsElement.FindName("QrCode");
                RegistreView registre = (RegistreView)e.Row.Item;
                var image = qrcode.Draw(registre.Registre.QrCode, 40);
                var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
                ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cm;

            // Liste des Tranches 
            using (var ct = new DocumatContext())
            {
                cbChoixTranche.ItemsSource = ct.Tranches.ToList();
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (CurrentPhase)
                {
                    // Recherche des registre en phase 1
                    case 1:
                        if (TbRechercher.Text != "")
                        {

                            if (cbChoixunite.SelectedItem != null)
                            {
                                // Liste des Unités de la Tranche
                                Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                                //Remplissage de la list de registre
                                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                                //Devra être modifié pour empêcher l'affichage des régistres déja attribués
                                RegistreView registreView = new RegistreView();
                                List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.INDEXE 
                                                                    && r.Registre.ID_Unite == unite.UniteID && r.Registre.Numero.ToLower().Contains(TbRechercher.Text.Trim().ToLower())).ToList();
                                dgRegistre.ItemsSource = registreViews;
                            }
                        }
                        else { RefreshRegistrePhase1(); }
                        break;
                    case 2:
                        if (TbRechercher.Text != "")
                        {
                            if (cbChoixunite.SelectedItem != null)
                            {
                                // Liste des Unités de la Tranche
                                Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                                //Remplissage de la list de registre
                                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                                //Devra être modifié pour empêcher l'affichage des régistres déja attribués
                                RegistreView registreView = new RegistreView();
                                List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE2 
                                                                    && r.Registre.ID_Unite == unite.UniteID && r.Registre.Numero.ToLower().Contains(TbRechercher.Text.Trim().ToLower())).ToList();
                                dgRegistre.ItemsSource = registreViews;
                            }
                        }
                        else { RefreshRegistrePhase2(); }
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentPhase == 1)
            {
                RefreshRegistrePhase1();
            }
            else
            {
                RefreshRegistrePhase2();
            }
        }

        private void BtnPhase1_Click(object sender, RoutedEventArgs e)
        {
            // On Selectionne le btnPhase1 par son design 
            BtnPhase1.Foreground = BtnPhase1.BorderBrush = Brushes.SteelBlue;
            BtnPhase1.Background = Brushes.White;

            // On Déselectionne le btnPhase2 par son design           
            BtnPhase2.Foreground = BtnPhase2.BorderBrush = Brushes.White;
            BtnPhase2.Background = null;

            CurrentPhase = 1;
            RefreshRegistrePhase1();
        }

        public void BtnPhase2_Click(object sender, RoutedEventArgs e)
        {
            // On Met le design en place pour changer de selection
            BtnPhase2.Background = Brushes.White;
            BtnPhase2.Foreground = BtnPhase2.BorderBrush = Brushes.SteelBlue;

            // On Déselectionne le btnPhase2 par son design           
            BtnPhase1.Foreground = BtnPhase1.BorderBrush = Brushes.White;
            BtnPhase1.Background = null;

            CurrentPhase = 2;
            RefreshRegistrePhase2();
        }

        private void ControleImage_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                if(CurrentPhase == 1)
                {
                    Image.ImageController imageController = new Image.ImageController((RegistreView)dgRegistre.SelectedItem,this);
                    imageController.Show();            
                }
                else
                {
                    Image.ImageControllerSecond imageController = new Image.ImageControllerSecond((RegistreView)dgRegistre.SelectedItem, this);
                    imageController.Show();
                }
            }
        }

        private void cbChoixTranche_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChoixTranche.SelectedItem != null)
            {
                // Liste des Unités de la Tranche
                Models.Tranche tranche = (Models.Tranche)cbChoixTranche.SelectedItem;
                using (var ct = new DocumatContext())
                {
                    if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        cbChoixunite.ItemsSource = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID).ToList();
                    }
                    else if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.CONTROLE)
                    {
                        // Recherche des Traitement d'attibution d'Unité
                        List<Models.Traitement> traitementsAttrAgent = ct.Traitement.Where(t => t.TableSelect.ToUpper() == DocumatContext.TbUnite.ToUpper()
                            && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_ATTIBUE && t.AgentID == Utilisateur.AgentID).ToList();
                        List<Unite> unites = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID ).ToList();

                        var jointure = from t in traitementsAttrAgent
                                       join u in unites on t.TableID equals u.UniteID
                                       select u;

                        cbChoixunite.ItemsSource = jointure;
                    }
                }
            }
        }

        private void cbChoixunite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if(cbChoixunite.SelectedItem != null)
                {
                    if (CurrentPhase == 1)
                    {

                        RefreshRegistrePhase1();
                    }
                    else
                    {
                        RefreshRegistrePhase2();
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
