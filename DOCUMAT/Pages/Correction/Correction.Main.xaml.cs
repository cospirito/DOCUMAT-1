using DOCUMAT.DataModels;
using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
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
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<RegistreDataModel> ListRegistreActuel = new List<RegistreDataModel>();
        public int CurrentPhase = 1;
        int parRecup = Int32.Parse(ConfigurationManager.AppSettings["perRecup"]);


        public Correction()
        {
            InitializeComponent();
            RefreshData.WorkerSupportsCancellation = true;
            RefreshData.WorkerReportsProgress = true;
            RefreshData.DoWork += RefreshData_DoWork;
            RefreshData.RunWorkerCompleted += RefreshData_RunWorkerCompleted;
            RefreshData.Disposed += RefreshData_Disposed;
            RefreshData.ProgressChanged += RefreshData_ProgressChanged;
        }

        private void RefreshData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            #region SET PROGRESSION 
            // Affichage de la progression
            double widthPro = (BtnAnnuleChargement.ActualWidth * e.ProgressPercentage) / 100;

            if (e.ProgressPercentage > 97)
            {
                BtnAnnuleChargement.BorderBrush = Brushes.LightGreen;
                BtnAnnuleChargement.Foreground = Brushes.White;
            }

            if (e.ProgressPercentage > 40)
            {
                BtnAnnuleChargement.Foreground = Brushes.White;
            }

            if (e.ProgressPercentage > 2)
            {
                LoadText.Text = e.ProgressPercentage.ToString();
                LoadIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Percent;
                BtnAnnuleChargement.BorderThickness = new Thickness(widthPro, 1, 0, 1);
            }
            #endregion
        }

        private void RefreshData_Disposed(object sender, EventArgs e)
        {
            try
            {
                PanelLoader.Visibility = Visibility.Collapsed;
                panelInteracBtn.IsEnabled = true;
                PanelPhase.IsEnabled = true;

                #region RESET PROGRESSION 
                // Affichage de la progression
                LoadText.Text = "Annuler";
                LoadIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                BtnAnnuleChargement.BorderThickness = new Thickness(0, 1, 0, 1);
                BtnAnnuleChargement.Foreground = Brushes.PaleVioletRed;
                BtnAnnuleChargement.BorderBrush = Brushes.PaleVioletRed;
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void RefreshData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                ListRegistreActuel = e.Result as List<RegistreDataModel>;
                dgRegistre.ItemsSource = ListRegistreActuel;
                PanelLoader.Visibility = Visibility.Collapsed;
                panelInteracBtn.IsEnabled = true;
                PanelPhase.IsEnabled = true;

                #region RESET PROGRESSION 
                // Affichage de la progression
                LoadText.Text = "Annuler";
                LoadIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                BtnAnnuleChargement.BorderThickness = new Thickness(0, 1, 0, 1);
                BtnAnnuleChargement.Foreground = Brushes.PaleVioletRed;
                BtnAnnuleChargement.BorderBrush = Brushes.PaleVioletRed;
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void RefreshData_DoWork(object sender, DoWorkEventArgs e)
        {
            // Définition de la methode de récupération des données 
            // Récupération de l'argument de la phase 
            var param = (object[])e.Argument;
            var phaseData = (int)param[0];
            var unite = (Models.Unite)param[1];
            var agent = (Models.Agent)param[2];

            if (phaseData == 1)
            {
                // Registre en PHASE 1
                try
                {
                    if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR ||
                       Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR ||
                       Utilisateur.Affectation == (int)Enumeration.AffectationAgent.CORRECTION)
                    {
                        using (var ct = new DocumatContext())
                        {
                            int nbRecup = 1;
                            string reqstr = $"SELECT COUNT(DISTINCT RG.RegistreID) FROM Registres RG " +
                                            $"INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                            $"WHERE RG.StatutActuel = {(int)Enumeration.Registre.PHASE1} " +
                                            $"AND RG.ID_Unite = {unite.UniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.StatutCorrection = 1 AND CR.PhaseCorrection = 1";

                            // récupération du nombre total de registres au statut scanné
                            int nbLigne = ct.Database.SqlQuery<int>(reqstr).FirstOrDefault();

                            // Liste à récupérer
                            List<RegistreDataModel> registreDataModels = new List<RegistreDataModel>();
                            if (nbLigne > parRecup)
                            {
                                nbRecup = (int)Math.Ceiling(((float)nbLigne / (float)parRecup));
                            }

                            for (int i = 0; i < nbRecup; i++)
                            {
                                if (RefreshData.CancellationPending == false)
                                {
                                    int limitId = (registreDataModels.Count == 0) ? 0 : registreDataModels.LastOrDefault().RegistreID;
                                    registreDataModels.AddRange(RegistreView.GetRegistreDataCorrectionPhase1(unite.UniteID, limitId, parRecup, (int)Enumeration.Registre.PHASE1).ToList());
                                    RefreshData.ReportProgress((int)Math.Ceiling(((float)i / (float)nbRecup) * 100));
                                }
                                else
                                {
                                    e.Result = registreDataModels;
                                    return;
                                }
                            }

                            //Remplissage de la list de registre
                            e.Result = registreDataModels;
                        }

                        ////Remplissage de la list de registre
                        //RegistreView registreView = new RegistreView();
                        //List<RegistreDataModel> registreViews = RegistreView.GetRegistreData((int)Enumeration.Registre.PHASE1).Where(r => r.ID_Unite == unite.UniteID).ToList();
                        //List<Models.Correction> corrections = new List<Models.Correction>();
                        //using (var ct = new DocumatContext())
                        //{
                        //    corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                        //                                      && c.StatutCorrection == 1 && c.PhaseCorrection == 1).ToList();
                        //}
                        //var jointure = from r in registreViews
                        //               join c in corrections
                        //               on r.RegistreID equals c.RegistreId
                        //               select r;

                        //e.Result = jointure.ToList();
                    }
                    else if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.INDEXATION)
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        List<RegistreDataModel> registreViews = RegistreView.GetRegistreDataByTraitbyAgent(agent.AgentID,(int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION,(int)Enumeration.Registre.PHASE1).Where(r => r.ID_Unite == unite.UniteID).ToList();
                        List<Models.Correction> corrections = new List<Models.Correction>();
                        using (var ct = new DocumatContext())
                        {
                            corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                                                              && c.StatutCorrection == 1 && c.PhaseCorrection == 1).ToList();
                        }
                        var jointure = from r in registreViews
                                       join c in corrections
                                       on r.RegistreID equals c.RegistreId
                                       select r;

                        e.Result = jointure.ToList();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
            else if (phaseData == 2)
            {
                try
                {
                    if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR ||
                       Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR ||
                       Utilisateur.Affectation == (int)Enumeration.AffectationAgent.CONTROLE)
                    {
                        using (var ct = new DocumatContext())
                        {
                            int nbRecup = 1;
                            string reqstr = $"SELECT COUNT(DISTINCT RG.RegistreID) FROM Registres RG " +
                                            $"INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                            $"WHERE RG.StatutActuel = {(int)Enumeration.Registre.PHASE1} " +
                                            $"AND RG.ID_Unite = {unite.UniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.StatutCorrection = 1 AND CR.PhaseCorrection = 3";

                            // récupération du nombre total de registres au statut scanné
                            int nbLigne = ct.Database.SqlQuery<int>(reqstr).FirstOrDefault();

                            // Liste à récupérer
                            List<RegistreDataModel> registreDataModels = new List<RegistreDataModel>();
                            if (nbLigne > parRecup)
                            {
                                nbRecup = (int)Math.Ceiling(((float)nbLigne / (float)parRecup));
                            }

                            for (int i = 0; i < nbRecup; i++)
                            {
                                if (RefreshData.CancellationPending == false)
                                {
                                    int limitId = (registreDataModels.Count == 0) ? 0 : registreDataModels.LastOrDefault().RegistreID;
                                    registreDataModels.AddRange(RegistreView.GetRegistreDataCorrectionPhase1(unite.UniteID, limitId, parRecup, (int)Enumeration.Registre.PHASE3).ToList());
                                    RefreshData.ReportProgress((int)Math.Ceiling(((float)i / (float)nbRecup) * 100));
                                }
                                else
                                {
                                    e.Result = registreDataModels;
                                    return;
                                }
                            }

                            //Remplissage de la list de registre
                            e.Result = registreDataModels;
                        }

                        ////Remplissage de la list de registre
                        //RegistreView registreView = new RegistreView();
                        //List<RegistreDataModel> registreViews = RegistreView.GetRegistreData((int)Enumeration.Registre.PHASE3).Where(r => r.ID_Unite == unite.UniteID).ToList();
                        //List<Models.Correction> corrections = new List<Models.Correction>();
                        //using (var ct = new DocumatContext())
                        //{
                        //    corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                        //                                      && c.StatutCorrection == 1 && c.PhaseCorrection == 3).ToList();
                        //}
                        //var jointure = from r in registreViews
                        //               join c in corrections
                        //               on r.RegistreID equals c.RegistreId
                        //               select r;

                        //e.Result = jointure.ToList();
                    }
                    else if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.INDEXATION)
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        List<RegistreDataModel> registreViews = RegistreView.GetRegistreDataByTraitbyAgent(agent.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION,(int)Enumeration.Registre.PHASE3).Where(r => r.ID_Unite == unite.UniteID).ToList();
                        List<Models.Correction> corrections = new List<Models.Correction>();
                        using (var ct = new DocumatContext())
                        {
                            corrections = ct.Correction.Where(c => c.ImageID == null && c.SequenceID == null
                                                              && c.StatutCorrection == 1 && c.PhaseCorrection == 3).ToList();
                        }
                        var jointure = from r in registreViews
                                       join c in corrections
                                       on r.RegistreID equals c.RegistreId
                                       select r;

                        e.Result = jointure.ToList();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
        }

        public Correction(Models.Agent user) : this()
        {
            try
            {
                Utilisateur = user;
                // Liste des Tranches 
                using (var ct = new DocumatContext())
                {
                    cbChoixTranche.ItemsSource = ct.Tranches.ToList();
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
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                var element = e.DetailsElement.FindName("QrCode");
                RegistreDataModel registre = (RegistreDataModel)e.Row.Item;
                var image = qrcode.Draw(registre.QrCode, 40);
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
            try
            {
                if (TbRechercher.Text != "")
                {
                    List<RegistreDataModel> registreData = ListRegistreActuel as List<RegistreDataModel>;

                    switch (cbChoixRecherche.SelectedIndex)
                    {
                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 1:
                            // Récupération des registre par service
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomComplet.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 2:
                            // Récupération des registre par Région
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomRegion.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 3:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.Numero.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                    }
                }
                else
                {
                    dgRegistre.ItemsSource = ListRegistreActuel;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
                dgRegistre.ContextMenu = cm;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    List<RegistreDataModel> registreData = ListRegistreActuel as List<RegistreDataModel>;

                    switch (cbChoixRecherche.SelectedIndex)
                    {
                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 1:
                            // Récupération des registre par service
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomComplet.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 2:
                            // Récupération des registre par Région
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomRegion.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 3:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.Numero.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                    }
                }
                else
                {
                    dgRegistre.ItemsSource = ListRegistreActuel;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbChoixunite.SelectedItem != null)
                {
                    Models.Unite unite = (Models.Unite)cbChoixunite.SelectedItem;
                    object[] arg = new object[] { CurrentPhase,unite,Utilisateur };
                    RefreshData.RunWorkerAsync(arg);
                    PanelLoader.Visibility = Visibility.Visible;
                    panelInteracBtn.IsEnabled = false;
                    PanelPhase.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                RefreshData.CancelAsync();
                PanelLoader.Visibility = Visibility.Collapsed;
                panelInteracBtn.IsEnabled = true;
                PanelPhase.IsEnabled = true;
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
            btnRefresh_Click(sender, e);
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
            btnRefresh_Click(sender, e);
        }

        private void CorrectionImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    if (dgRegistre.SelectedItems.Count == 1)
                    {
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == ((RegistreDataModel)dgRegistre.SelectedItem).RegistreID);
                        if (CurrentPhase == 1)
                        {
                            Image.ImageCorrecteur imageCorrecteur = new Image.ImageCorrecteur(registre, this);
                            imageCorrecteur.Show();
                        }
                        else
                        {
                            Image.ImageCorrecteurSecond imageCorrecteur = new Image.ImageCorrecteurSecond(registre, this);
                            imageCorrecteur.Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
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

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok){}
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
                    if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR 
                        || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR 
                        || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.INDEXATION)
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
            if (cbChoixunite.SelectedItem != null)
            {
                btnRefresh_Click(sender, e);
            }
            else
            {
                dgRegistre.ItemsSource = null;
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
                    dgRegistre.MaxHeight = StandardDgHeight - ((StandardHeight - e.NewSize.Height) * 2 / 3);
                }
                else
                {
                    dgRegistre.MaxHeight = StandardDgHeight + ((e.NewSize.Height - StandardHeight) * 2 / 3);
                }
            }
        }

        private void BtnAnnuleChargement_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Voulez vous vraiment annuler l'opération ?", "Annuler", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                RefreshData.CancelAsync();
            }
        }
    }
}
