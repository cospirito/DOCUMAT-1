using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
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
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public int CurrentPhase = 1;

        public Models.Agent Utilisateur;

        #region FONCTIONS
        public void RefreshRegistrePhase1()
        {
            try
            {
                //Remplissage de la list de registre
                //Les registres de type Phase 1 sont les registre nouvellement indexée 
                // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                RegistreView registreView = new RegistreView();
                dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.INDEXE);
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
                // Devra être modifié pour empêcher l'affichage des régistres déja attribués
                RegistreView registreView = new RegistreView();
                dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PHASE2 );
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

        private void ImprimerQrCode_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                string qrCode = ((RegistreView)dgRegistre.SelectedItem).Registre.QrCode;
                Impression.QrCode PageImp = new Impression.QrCode(qrCode);
                PageImp.Show();
            }
        }

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
            try
            {
                switch (CurrentPhase)
                {
                    // Recherche des registre en phase 1
                    case 1:
                            if (TbRechercher.Text != "")
                            {
                                RegistreView RegistreView = new RegistreView();
                                switch (cbChoixRecherche.SelectedIndex)
                                {
                                    case 0:
                                        dgRegistre.ItemsSource = RegistreView.GetViewsList().Where(r => r.Registre.QrCode.Contains(TbRechercher.Text.ToUpper())
                                                                 && r.Registre.StatutActuel == (int)Enumeration.Registre.INDEXE).ToList();
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cm;
            RefreshRegistrePhase1();
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbChoixRecherche_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
    }
}
