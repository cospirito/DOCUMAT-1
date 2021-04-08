using DOCUMAT.DataModels;
using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<RegistreDataModel> ListRegistreActuel = new List<RegistreDataModel>();
        public Models.Agent Utilisateur;
        int parRecup = Int32.Parse(ConfigurationManager.AppSettings["perRecup"]);

        public Controle()
        {
            InitializeComponent();
            RefreshData.WorkerSupportsCancellation = true;
            RefreshData.WorkerReportsProgress = true;
            RefreshData.DoWork += RefreshData_DoWork;
            RefreshData.RunWorkerCompleted += RefreshData_RunWorkerCompleted;
            RefreshData.ProgressChanged += RefreshData_ProgressChanged;
            RefreshData.Disposed += RefreshData_Disposed;
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

            if (phaseData == 1)
            {
                // Registre en PHASE 1
                try
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        // récupération du nombre total de registres au statut scanné
                        int nbLigne = ct.Database.SqlQuery<int>($"SELECT COUNT(RegistreID) FROM Registres WHERE StatutActuel = {(int)Enumeration.Registre.INDEXE} AND ID_Unite = {unite.UniteID}").FirstOrDefault();

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
                                registreDataModels.AddRange(RegistreView.GetRegistreDataByUnite(unite.UniteID, limitId, parRecup, (int)Enumeration.Registre.INDEXE).ToList());
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
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        string reqstr = $"SELECT COUNT(DISTINCT RG.RegistreID) FROM Registres RG " +
                                        $"INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                        $"WHERE RG.StatutActuel = {(int)Enumeration.Registre.PHASE2} " +
                                        $"AND RG.ID_Unite = {unite.UniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.PhaseCorrection <> 3";

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
                                registreDataModels.AddRange(RegistreView.GetRegistreDataControlePhase2(unite.UniteID, limitId, parRecup, (int)Enumeration.Registre.INDEXE).ToList());
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
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
            else if (phaseData == 3)
            {
                try
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        string reqstr = $"SELECT COUNT(DISTINCT RG.RegistreID) FROM Registres RG " +
                                        $"INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                        $"WHERE RG.StatutActuel = {(int)Enumeration.Registre.PHASE2} " +
                                        $"AND RG.ID_Unite = {unite.UniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.PhaseCorrection = 3";

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
                                registreDataModels.AddRange(RegistreView.GetRegistreDataControlePhaseFinal(unite.UniteID, limitId, parRecup, (int)Enumeration.Registre.INDEXE).ToList());
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
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
        }

        public Controle(Models.Agent user):this()
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

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {            
            //((RegistreView)e.Row.Item).NumeroOrdre = e.Row.GetIndex() + 1;
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
            catch (Exception ex) { }
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
                    object[] arg = new object[] {CurrentPhase,unite };
                    RefreshData.RunWorkerAsync(arg);
                    PanelLoader.Visibility = Visibility.Visible;
                    panelInteracBtn.IsEnabled = false;
                    PanelPhase.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                //ex.ExceptionCatcher();
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

            // On Déselectionne le btnPhaseLast par son design           
            BtnPhaseLast.Foreground = BtnPhaseLast.BorderBrush = Brushes.White;
            BtnPhaseLast.Background = null;

            CurrentPhase = 1;
            btnRefresh_Click(sender, e);
        }

        public void BtnPhase2_Click(object sender, RoutedEventArgs e)
        {
            // On Met le design en place pour changer de selection
            BtnPhase2.Foreground = BtnPhase2.BorderBrush = Brushes.SteelBlue;
            BtnPhase2.Background = Brushes.White;

            // On Déselectionne le btnPhase2 par son design           
            BtnPhase1.Foreground = BtnPhase1.BorderBrush = Brushes.White;
            BtnPhase1.Background = null;

            // On Déselectionne le btnPhaseLast par son design           
            BtnPhaseLast.Foreground = BtnPhaseLast.BorderBrush = Brushes.White;
            BtnPhaseLast.Background = null;

            CurrentPhase = 2;
            btnRefresh_Click(sender, e);
        }

        private void ControleImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    if (dgRegistre.SelectedItems.Count == 1)
                    {
                        Models.Registre registre = ct.Registre.FirstOrDefault(r=> r.RegistreID == ((RegistreDataModel)dgRegistre.SelectedItem).RegistreID);
                        if (CurrentPhase == 1)
                        {
                            Image.ImageController imageController = new Image.ImageController(registre, this);
                            imageController.Show();
                        }
                        else if (CurrentPhase == 2)
                        {
                            Image.ImageControllerSecond imageController = new Image.ImageControllerSecond(registre, this);
                            imageController.Show();
                        }
                        else if (CurrentPhase == 3)
                        {
                            Image.ImageControllerValidation imageControllerValidation = new Image.ImageControllerValidation(registre, this);
                            imageControllerValidation.Show();
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
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
            if(cbChoixunite.SelectedItem != null)
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

        private void BtnPhaseLast_Click(object sender, RoutedEventArgs e)
        {
            // On Met le design en place pour changer de selection
            BtnPhaseLast.Background = Brushes.White;
            BtnPhaseLast.Foreground = BtnPhaseLast.BorderBrush = Brushes.SteelBlue;

            // On Met le design en place pour changer de selection
            BtnPhase2.Foreground = BtnPhase2.BorderBrush = Brushes.White;
            BtnPhase2.Background = null;

            // On Déselectionne le btnPhase2 par son design           
            BtnPhase1.Foreground = BtnPhase1.BorderBrush = Brushes.White;
            BtnPhase1.Background = null;

            CurrentPhase = 3;
            btnRefresh_Click(sender, e);
        }

        private void cbChoixTranche_DropDownOpened(object sender, EventArgs e)
        {
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
