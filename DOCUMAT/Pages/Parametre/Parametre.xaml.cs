using System.Configuration;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data.SqlClient;
using System.Windows.Media;

namespace DOCUMAT.Pages.Parametre
{
    /// <summary>
    /// Logique d'interaction pour Parametre.xaml
    /// </summary>
    public partial class Parametre : Window
    {
        bool isConnectable = false;

        public Parametre()
        {
            InitializeComponent();

            // Récupération des informations de config de base dans l'appConfig
            tbDosScan.Text = ConfigurationManager.AppSettings.Get("CheminDossier_Scan");
            tbDosAvatar.Text = ConfigurationManager.AppSettings.Get("CheminDossier_Avatar");
            tbCheminDB.Text = ConfigurationManager.ConnectionStrings["DOCUMAT"].ConnectionString;

            //vérification de la connextion 
            try
            {
                SqlConnection sqlConnection = new SqlConnection(tbCheminDB.Text);
                sqlConnection.Open();
                ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check;
                ckeckIcon.Foreground = Brushes.LightGreen;
                isConnectable = true;
            }
            catch (System.Exception ex)
            {
                ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                ckeckIcon.Foreground = Brushes.White;
            }
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("ENREGISTRER LES MODIFS ? ", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (Directory.Exists(tbDosScan.Text) && Directory.Exists(tbDosAvatar.Text))
                {
                    try
                    {
                        SqlConnection sqlConnection = new SqlConnection(tbCheminDB.Text);
                        sqlConnection.Open();
                        Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        configuration.AppSettings.Settings["CheminDossier_Scan"].Value = tbDosScan.Text;
                        configuration.AppSettings.Settings["CheminDossier_Avatar"].Value = tbDosAvatar.Text;
                        configuration.ConnectionStrings.ConnectionStrings["DOCUMAT"].ConnectionString = tbCheminDB.Text; 
                        configuration.Save();                        
                        ConfigurationManager.RefreshSection("appSettings");
                        MessageBox.Show("Veuillez redémarrer L'application pour Appliquer les Modifications", "Modification Effectuée !!!", MessageBoxButton.OK, MessageBoxImage.Information);
                        System.Windows.Forms.Application.Restart();
                        System.Windows.Application.Current.Shutdown();
                        this.Close();
                    }
                    catch (System.Exception ex)
                    {
                        ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                        ckeckIcon.Foreground = Brushes.Red;
                        MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Dossier introuvable !!!", "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }            
        }

        private void BtnParcourirScan_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "CHOISIR LE DOSSIER RACINE DE SCANNERISATION";
            dlg.IsFolderPicker = true;
            //dlg.InitialDirectory = currentDirectory;
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            //dlg.DefaultDirectory = currentDirectory;
            dlg.FileOk += Dlg_FileOk;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbDosScan.Text = dlg.FileName;
                // Do something with selected folder string
            }
        }

        private void Dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Focus();
        }

        private void BtnParcourirAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "CHOISIR LE DOSSIER DES AVATARS";
            dlg.IsFolderPicker = true;
            dlg.FileOk += Dlg_FileOk;
            //dlg.InitialDirectory = currentDirectory;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            //dlg.DefaultDirectory = currentDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbDosAvatar.Text = dlg.FileName;
                // Do something with selected folder string
            }
        }

        private void BtnCheckConnex_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(tbCheminDB.Text);
                sqlConnection.Open();
                ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check;
                ckeckIcon.Foreground = Brushes.LightGreen;
                MessageBox.Show("Connexion à la source de données reussie", "INFORMATION", MessageBoxButton.OK, MessageBoxImage.Information);
                isConnectable = true;
            }
            catch (System.Exception ex)
            {
                ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                ckeckIcon.Foreground = Brushes.Red;
                MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                isConnectable = false;
            }
        }
    }
}
