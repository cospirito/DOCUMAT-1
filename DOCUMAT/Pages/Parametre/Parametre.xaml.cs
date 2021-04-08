using System.Configuration;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data.SqlClient;
using System.Windows.Media;
using System.ComponentModel;
using System;

namespace DOCUMAT.Pages.Parametre
{
    /// <summary>
    /// Logique d'interaction pour Parametre.xaml
    /// </summary>
    public partial class Parametre : Window
    {
        private readonly BackgroundWorker AttempConnexion = new BackgroundWorker();

        bool isConnectable = false;

        public Parametre()
        {
            InitializeComponent();
            AttempConnexion.WorkerSupportsCancellation = true;
            AttempConnexion.DoWork += AttempConnexion_DoWork; ;
            AttempConnexion.RunWorkerCompleted += AttempConnexion_RunWorkerCompleted; ;
            AttempConnexion.Disposed += AttempConnexion_Disposed;

            // Récupération des informations de config de base dans l'appConfig
            tbDosScan.Text = ConfigurationManager.AppSettings.Get("CheminDossier_Scan");
            tbDosAvatar.Text = ConfigurationManager.AppSettings.Get("CheminDossier_Avatar");
            tbCheminDB.Text = ConfigurationManager.ConnectionStrings["DOCUMAT"].ConnectionString;

            // initialisation de la Base de Données               
            ckeckIcon.Visibility = Visibility.Collapsed;
            loaderGif.Visibility = Visibility.Visible;
            AttempConnexion.RunWorkerAsync(tbCheminDB.Text);
        }

        private void AttempConnexion_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AttempConnexion_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if((bool)e.Result == true)
            {
                ckeckIcon.Visibility = Visibility.Visible;
                loaderGif.Visibility = Visibility.Collapsed;
                ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check;
                ckeckIcon.Foreground = Brushes.LightGreen;
                isConnectable = true;
            }
            else
            {
                ckeckIcon.Visibility = Visibility.Visible;
                loaderGif.Visibility = Visibility.Collapsed;
                ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                ckeckIcon.Foreground = Brushes.White;
            }
        }

        private void AttempConnexion_DoWork(object sender, DoWorkEventArgs e)
        {
            //vérification de la connextion 
            try
            {
                SqlConnection sqlConnection = new SqlConnection((string)e.Argument);
                sqlConnection.Open();
                e.Result = true;
            }
            catch (System.Exception ex)
            {
                e.Result = false;
                ex.ExceptionCatcher();
            }
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("ENREGISTRER LES MODIFS ET REDEMARRER L'APPLICATION ? ", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (Directory.Exists(tbDosScan.Text) && Directory.Exists(tbDosAvatar.Text))
                {
                    try 
                    {

                        if (ckeckIcon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Check && ckeckIcon.Visibility == Visibility.Visible)
                        {
                            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            configuration.AppSettings.Settings["CheminDossier_Scan"].Value = tbDosScan.Text;
                            configuration.AppSettings.Settings["CheminDossier_Avatar"].Value = tbDosAvatar.Text;
                            configuration.ConnectionStrings.ConnectionStrings["DOCUMAT"].ConnectionString = tbCheminDB.Text;
                            configuration.Save();
                            ConfigurationManager.RefreshSection("appSettings");
                            Application.Current.Shutdown();
                            MessageBox.Show("Veuillez redémarrer L'application pour Appliquer les Modifications", "Modification Effectuée !!!", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Windows.Forms.Application.Restart();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Le Test de La connexion doit être Reussi au paravant", "Connexion Non Vérifié!!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ckeckIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                        ckeckIcon.Foreground = Brushes.Red;
                        ex.ExceptionCatcher();
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
            try
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
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Focus();
        }

        private void BtnParcourirAvatar_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnCheckConnex_Click(object sender, RoutedEventArgs e)
        {
            if(loaderGif.Visibility == Visibility.Collapsed)
            {
                // initialisation de la Base de Données               
                ckeckIcon.Visibility = Visibility.Collapsed;
                loaderGif.Visibility = Visibility.Visible;
                AttempConnexion.RunWorkerAsync(tbCheminDB.Text);
            }
        }
    }
}
