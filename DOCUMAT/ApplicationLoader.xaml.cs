using System;
using System.Windows;
using System.Timers;
using DOCUMAT.Models;
using System.Data.SqlClient;
using System.Configuration;
using DOCUMAT.Pages.Parametre;
using System.Linq;
using System.ComponentModel;

namespace DOCUMAT
{
    /// <summary>
    /// Logique d'interaction pour ApplicationLoader.xaml
    /// </summary>
    public partial class ApplicationLoader : Window
    {
        // VERSION DE L'APPLICATION ACTUELLE
        // est défini ici pour des raisons de sécurité 
        public string VERSION_APP = "1.1";
        public delegate void MontrerProgres(int valeur);
        public string DossierAnimation = ConfigurationManager.AppSettings["CheminDossier_Anim"];         
        public Connexion Connexion = null;
        private readonly BackgroundWorker AttempConnexion = new BackgroundWorker();

        public ApplicationLoader()
        {
            InitializeComponent();
            // définition du titre et de la version 
            txbTitre.Text = "DOCUMAT v" + VERSION_APP;
            AttempConnexion.WorkerSupportsCancellation = true;
            AttempConnexion.DoWork += AttempConnexion_DoWork; ;
            AttempConnexion.RunWorkerCompleted += AttempConnexion_RunWorkerCompleted; ;
            AttempConnexion.Disposed += AttempConnexion_Disposed;
        }

        private void AttempConnexion_Disposed(object sender, EventArgs e)
        {
            loaderGif.Visibility = Visibility.Collapsed;
        }

        private void AttempConnexion_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loaderGif.Visibility = Visibility.Collapsed;
            //loaderCap.Visibility = Visibility.Visible;

            if ((int)e.Result == 0)
            {
                if (Connexion == null)
                {
                    // Appel à la connexion 
                    Connexion = new Connexion();
                    Connexion.Show();
                }
            }
            else if ((int)e.Result == 1)
            {
                MessageBox.Show("La version que vous utilisez est obsolète,\n" +
                                "veuillez vous proccurer la nouvelle version SVP !!!", "Mise à Jour Nécessaire", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else 
            {
                Parametre parametre = new Parametre();
                parametre.Show();
            }
            this.Close();
        }

        private void AttempConnexion_DoWork(object sender, DoWorkEventArgs e)
        {
            int ResultPossible = 0; //0 : connex, 1: VersionOb , 2: DatabaseConnexErro
            try
            {
                using (var sqlConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["DOCUMAT"].ConnectionString))
                {
                    sqlConnexion.Open();

                    //Réquète de vérification de la version de l'application 
                    using (var ct = new DocumatContext())
                    {
                        if (!ct.Configs.Any(c => c.VersionApp.Trim() == VERSION_APP.Trim() && c.NomApp.Trim().ToUpper() == "DOCUMAT".ToUpper()))
                        {
                            ResultPossible = 1;
                        }
                        else
                        {
                            ResultPossible = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Creer un Fichier log des erreurs eventuel de l'Application 
                Console.WriteLine(ex.Message);
                ResultPossible = 2;
            }
            e.Result = ResultPossible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // initialisation de la Base de Données               
            AttempConnexion.RunWorkerAsync();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
        }

        private void Window_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ProgressBar_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
        }

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}

