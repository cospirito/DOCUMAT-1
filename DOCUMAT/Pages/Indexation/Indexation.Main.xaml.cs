using DOCUMAT.DataModels;
using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DOCUMAT.Pages.Indexation
{
    /// <summary>
    /// Logique d'interaction pour Indexation.xaml
    /// </summary>
    public partial class Indexation : Page
    {
        public Models.Agent Utilisateur;
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<RegistreDataModel> ListRegistreActuel = new List<RegistreDataModel>();
        int parRecup = Int32.Parse(ConfigurationManager.AppSettings["perRecup"]);

        public Indexation()
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
                PanelData.Visibility = Visibility.Visible;
                panelInteracBtn.IsEnabled = true;

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
                PanelData.Visibility = Visibility.Visible;
                panelInteracBtn.IsEnabled = true;

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
            try
            {
                Models.Agent user = (Models.Agent)e.Argument;
                if (user.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || user.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        // récupération du nombre total de registres au statut scanné
                        int nbLigne = ct.Database.SqlQuery<int>($"SELECT COUNT(RegistreID) FROM Registres WHERE StatutActuel = {(int)Enumeration.Registre.SCANNE}").FirstOrDefault();

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
                                registreDataModels.AddRange(RegistreView.GetRegistreData(limitId, parRecup, (int)Enumeration.Registre.SCANNE).ToList());
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
                else
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        // Nombre d'index du Registres 
                        string reqAllreg = $"SELECT COUNT(RG.RegistreID) "+
                                           $"FROM REGISTRES RG " +
                                           $"INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                           $"INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                           $"INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                           $"INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                           $"INNER JOIN TRAITEMENTS TR ON TR.TableID = RG.RegistreID "+
                                           $"WHERE Lower(TR.TableSelect) = Lower('Registres') AND TR.TypeTraitement = {(int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION} " +
                                           $"AND TR.AgentID = {user.AgentID}  AND RG.StatutActuel = {(int)Enumeration.Registre.SCANNE}";
                        // récupération du nombre total de registres au statut scanné
                        int nbLigne = ct.Database.SqlQuery<int>(reqAllreg).FirstOrDefault();

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
                                registreDataModels.AddRange(RegistreView.GetRegistreDataByTraitbyAgent(Utilisateur.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, limitId, parRecup, (int)Enumeration.Registre.SCANNE).ToList());
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
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public Indexation(Models.Agent user) : this()
        {
            Utilisateur = user;
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
            ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cm;
            MenuItem menuItemIndexation = (MenuItem)cm.Items.GetItemAt(0);
            MenuItem menuItemSupervision = (MenuItem)cm.Items.GetItemAt(1);

            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR
                || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                menuItemSupervision.IsEnabled = true;
                menuItemIndexation.IsEnabled = true;
            }
            else
            {
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.INDEXATION)
                {
                    menuItemIndexation.IsEnabled = true;
                }
                else
                {
                    menuItemIndexation.IsEnabled = false;
                }
                menuItemSupervision.IsEnabled = false;
            }
        }

        private void IndexerImage_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                try
                {
                    using (var ct = new DocumatContext())
                    {
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == ((RegistreDataModel)dgRegistre.SelectedItem).RegistreID);
                        Image.ImageViewer imageViewer = new Image.ImageViewer(registre, this);
                        this.IsEnabled = false;
                        imageViewer.Show();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
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

        private void BtnActualise_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshData.RunWorkerAsync(Utilisateur);
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

        private void SuperviserIndexation_Click(object sender, RoutedEventArgs e)
        {
            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR
               || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    try
                    {
                        using (var ct = new DocumatContext())
                        {
                            Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == ((RegistreDataModel)dgRegistre.SelectedItem).RegistreID);
                            Image.ImageIndexSuperviseur image = new Image.ImageIndexSuperviseur(registre, this);
                            this.IsEnabled = false;
                            image.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ExceptionCatcher();
                    }
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
