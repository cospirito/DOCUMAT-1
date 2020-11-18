using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DOCUMAT.Pages.Correction
{
    /// <summary>
    /// Logique d'interaction pour Correction.xaml
    /// </summary>
    public partial class Correction : Page
    {
        public Models.Agent Utilisateur;
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];

        public Correction()
        {
            InitializeComponent();
        }

        public Correction(Models.Agent user):this()
        {
            Utilisateur = user;
        }

        public int CurrentPhase = 1;

        #region FONCTIONS
        public void RefreshRegistrePhase1()
        {
            try
            {
                if (cbChoixunite.SelectedItem != null)
                {
                    //Remplissage de la list de registre
                    //Les registres de type Phase 1 sont les registre nouvellement indexée 
                    // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                    Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                    RegistreView registreView = new RegistreView();
                    List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1 && r.Registre.ID_Unite == unite.UniteID).ToList();
                    List<Models.Correction> corrections = new List<Models.Correction>();
                    using (var ct = new DocumatContext())
                    {
                        corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                                                          && c.StatutCorrection == 1 && c.PhaseCorrection == 1).ToList();
                    }
                    var jointure = from r in registreViews
                                   join c in corrections
                                   on r.Registre.RegistreID equals c.RegistreId
                                   select r;

                    //dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1);
                    dgRegistre.ItemsSource = jointure;
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
                    //Remplissage de la list de registre
                    //Les registres de type Phase 1 sont les registre nouvellement indexée 
                    // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                    Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                    RegistreView registreView = new RegistreView();
                    List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE3 && r.Registre.ID_Unite == unite.UniteID).ToList();
                    List<Models.Correction> corrections = new List<Models.Correction>();
                    using (var ct = new DocumatContext())
                    {
                        corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                                                          && c.StatutCorrection == 1 && c.PhaseCorrection == 3).ToList();
                    }
                    var jointure = from r in registreViews
                                   join c in corrections
                                   on r.Registre.RegistreID equals c.RegistreId
                                   select r;

                    //dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1);
                    dgRegistre.ItemsSource = jointure;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

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
                                //Remplissage de la list de registre
                                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                                // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                                Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                                RegistreView registreView = new RegistreView();
                                List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1 
                                                                    && r.Registre.ID_Unite == unite.UniteID && r.Registre.Numero.ToLower().Contains(TbRechercher.Text.Trim().ToLower())).ToList();
                                List<Models.Correction> corrections = new List<Models.Correction>();
                                using (var ct = new DocumatContext())
                                {
                                    corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                                                                      && c.StatutCorrection == 1 && c.PhaseCorrection == 1).ToList();
                                }
                                var jointure = from r in registreViews
                                               join c in corrections
                                               on r.Registre.RegistreID equals c.RegistreId
                                               select r;

                                //dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1);
                                dgRegistre.ItemsSource = jointure;
                            }                            
                        } else{RefreshRegistrePhase1();}
                        break;
                    case 2:
                        if (TbRechercher.Text != "")
                        {
                            if (cbChoixunite.SelectedItem != null)
                            {
                                //Remplissage de la list de registre
                                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                                // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                                Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                                RegistreView registreView = new RegistreView();
                                List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE3 
                                                && r.Registre.ID_Unite == unite.UniteID && r.Registre.Numero.ToLower().Contains(TbRechercher.Text.Trim().ToLower())).ToList();
                                List<Models.Correction> corrections = new List<Models.Correction>();
                                using (var ct = new DocumatContext())
                                {
                                    corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                                                                        && c.StatutCorrection == 1 && c.PhaseCorrection == 3).ToList();
                                }
                                var jointure = from r in registreViews
                                                join c in corrections
                                                on r.Registre.RegistreID equals c.RegistreId
                                                select r;

                                //dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1);
                                dgRegistre.ItemsSource = jointure;
                            }
                        } else{RefreshRegistrePhase2();}
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbChoixRecherche_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
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

        private void BtnPhase2_Click(object sender, RoutedEventArgs e)
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

        private void CorrectionImage_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                if(CurrentPhase == 1)
                {
                    Image.ImageCorrecteur imageCorrecteur = new Image.ImageCorrecteur((RegistreView)dgRegistre.SelectedItem, this);
                    imageCorrecteur.Show();
                }
                else
                {
                    Image.ImageCorrecteurSecond imageCorrecteur = new Image.ImageCorrecteurSecond((RegistreView)dgRegistre.SelectedItem,this);
                    imageCorrecteur.Show(); 
                }
            }
        }

        private void OuvrirDossierScan_Click(object sender, RoutedEventArgs e)
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

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
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
                    if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        cbChoixunite.ItemsSource = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID).ToList();
                    }
                    else if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.CORRECTION)
                    {
                        // Recherche des Traitement d'attibution d'Unité
                        List<Models.Traitement> traitementsAttrAgent = ct.Traitement.Where(t => t.TableSelect.ToUpper() == DocumatContext.TbUnite.ToUpper()
                            && t.TypeTraitement == (int)Enumeration.TypeTraitement.CORRECTION_ATTRIBUE && t.AgentID == Utilisateur.AgentID).ToList();
                        List<Unite> unites = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID).ToList();

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
                if (cbChoixunite.SelectedItem != null)
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
