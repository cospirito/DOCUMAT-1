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
                //Remplissage de la list de registre
                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                RegistreView registreView = new RegistreView();
                List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1).ToList();
                List<Models.Correction> corrections = new List<Models.Correction>();
                using(var ct = new DocumatContext())
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
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public void RefreshRegistrePhase2()
        {
            try
            {
                //Remplissage de la list de registre
                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                RegistreView registreView = new RegistreView();
                List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE3).ToList();
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
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            //RegistreView view = (RegistreView)e.Row.Item;
            //view.NumeroOrdre = e.Row.GetIndex() + 1;
            //e.Row.Item = view;

            ((RegistreView)e.Row.Item).NumeroOrdre = e.Row.GetIndex() + 1;
        }

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            // Chargement de l'image du Qrcode de la ligne
            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            var element = e.DetailsElement.FindName("QrCode");
            RegistreView registre = (RegistreView)e.Row.Item;
            var image = qrcode.Draw(registre.Registre.QrCode, 40);
            var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
            ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cm;

            if (CurrentPhase == 1)
            {
                RefreshRegistrePhase1();
            }
            else
            {
                RefreshRegistrePhase2();
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
                            //Remplissage de la list de registre
                            //Les registres de type Phase 1 sont les registre nouvellement indexée 
                            // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                            RegistreView registreView = new RegistreView();
                            List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE1).ToList();
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


                            switch (cbChoixRecherche.SelectedIndex)
                            {
                                case 0:
                                    dgRegistre.ItemsSource = jointure.Where(r => r.Registre.QrCode.Contains(TbRechercher.Text.ToUpper())).ToList();
                                    break;
                                case 1:
                                    // Récupération des registre par service
                                    List<Models.Service> Services1 = registreView.context.Service.ToList();
                                    List<Models.Livraison> Livraisons1 = registreView.context.Livraison.ToList();
                                    List<Models.Versement> Versements1 = registreView.context.Versement.ToList();
                                    List<RegistreView> registreViews1 = jointure.ToList();

                                    var jointure1 = from r in registreViews1
                                                    join v in Versements1 on r.Registre.VersementID equals v.VersementID into table1
                                                    from v in table1.ToList()
                                                    join l in Livraisons1 on v.LivraisonID equals l.LivraisonID into table2
                                                    from l in table2.ToList()
                                                    join s in Services1 on l.ServiceID equals s.ServiceID
                                                    where s.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                                    select r;
                                    dgRegistre.ItemsSource = jointure1;
                                    break;
                                case 2:
                                    // Récupération des registre par service
                                    List<Models.Region> Region2 = registreView.context.Region.ToList();
                                    List<Models.Service> Services2 = registreView.context.Service.ToList();
                                    List<Models.Livraison> Livraisons2 = registreView.context.Livraison.ToList();
                                    List<Models.Versement> Versements2 = registreView.context.Versement.ToList();
                                    List<RegistreView> registreViews2 = jointure.ToList();

                                    var jointure2 = from r in registreViews2
                                                    join v in Versements2 on r.Registre.VersementID equals v.VersementID into table1
                                                    from v in table1.ToList()
                                                    join l in Livraisons2 on v.LivraisonID equals l.LivraisonID into table2
                                                    from l in table2.ToList()
                                                    join s in Services2 on l.ServiceID equals s.ServiceID into table3
                                                    from s in table3.ToList()
                                                    join rg in Region2 on s.RegionID equals rg.RegionID
                                                    where rg.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                                    select r;
                                    dgRegistre.ItemsSource = jointure2;
                                    break;
                                case 3:
                                    dgRegistre.ItemsSource = jointure.Where(r => r.Registre.QrCode.Contains(TbRechercher.Text.ToUpper())).ToList();
                                    break;
                                default:
                                    RefreshRegistrePhase1();
                                    break;
                            }
                        }
                        else
                        {
                            RefreshRegistrePhase1();
                        }
                        break;

                    case 2:
                        if (TbRechercher.Text != "")
                        {
                            //Remplissage de la list de registre
                            //Les registres de type Phase 1 sont les registre nouvellement indexée 
                            //Devra être modifié pour empêcher l'affichage des régistres déja attribués
                            RegistreView registreView = new RegistreView();
                            List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE3).ToList();
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

                            switch (cbChoixRecherche.SelectedIndex)
                            {
                                case 0:
                                    dgRegistre.ItemsSource = jointure.Where(r => r.Registre.QrCode.Contains(TbRechercher.Text.ToUpper())).ToList();
                                    break;
                                case 1:
                                    // Récupération des registre par service
                                    List<Models.Service> Services1 = registreView.context.Service.ToList();
                                    List<Models.Livraison> Livraisons1 = registreView.context.Livraison.ToList();
                                    List<Models.Versement> Versements1 = registreView.context.Versement.ToList();
                                    List<RegistreView> registreViews1 = jointure.ToList();

                                    var jointure1 = from r in registreViews1
                                                    join v in Versements1 on r.Registre.VersementID equals v.VersementID into table1
                                                    from v in table1.ToList()
                                                    join l in Livraisons1 on v.LivraisonID equals l.LivraisonID into table2
                                                    from l in table2.ToList()
                                                    join s in Services1 on l.ServiceID equals s.ServiceID
                                                    where s.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                                    select r;
                                    dgRegistre.ItemsSource = jointure1;
                                    break;
                                case 2:
                                    // Récupération des registre par service
                                    List<Models.Region> Region2 = registreView.context.Region.ToList();
                                    List<Models.Service> Services2 = registreView.context.Service.ToList();
                                    List<Models.Livraison> Livraisons2 = registreView.context.Livraison.ToList();
                                    List<Models.Versement> Versements2 = registreView.context.Versement.ToList();
                                    List<RegistreView> registreViews2 = jointure.ToList();

                                    var jointure2 = from r in registreViews2
                                                    join v in Versements2 on r.Registre.VersementID equals v.VersementID into table1
                                                    from v in table1.ToList()
                                                    join l in Livraisons2 on v.LivraisonID equals l.LivraisonID into table2
                                                    from l in table2.ToList()
                                                    join s in Services2 on l.ServiceID equals s.ServiceID into table3
                                                    from s in table3.ToList()
                                                    join rg in Region2 on s.RegionID equals rg.RegionID
                                                    where rg.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                                    select r;
                                    dgRegistre.ItemsSource = jointure2;
                                    break;
                                case 3:
                                    dgRegistre.ItemsSource = jointure.Where(r => r.Registre.Numero.Contains(TbRechercher.Text.ToUpper())).ToList();
                                    break;
                                default:
                                    RefreshRegistrePhase1();
                                    break;
                            }
                        }
                        else
                        {
                            RefreshRegistrePhase2();
                        }
                        break;
                    default:
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
    }
}
