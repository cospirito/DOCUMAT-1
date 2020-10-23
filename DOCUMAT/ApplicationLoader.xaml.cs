using System;
using System.Windows;
using System.Timers;
using System.Data.Entity;
using DOCUMAT.Models;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
using DOCUMAT.Migrations;
using System.Configuration;
using DOCUMAT.Pages.Parametre;
using System.Linq;

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
        public System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        public void Progres(int valeur)
        {
            //ProgressBar.Value = valeur;
        }

        public ApplicationLoader()
        {
            InitializeComponent();
            // définition du titre et de la version 
            txbTitre.Text = "DOCUMAT v" + VERSION_APP;
            // Définition d'un chemin absolue pour les animations 
            string AnimFile = Directory.GetCurrentDirectory() + DossierAnimation + "MainLoader.gif";
            if(File.Exists(AnimFile))
            {
                myGif.Source = new Uri(AnimFile);
            }
            else
            {
                MessageBox.Show("Error gif not founded !!!"); 
            }
        }

        // gestion de l'évènement après le temps écoulé
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(Connexion == null)
            {
                // Appel à la connexion 
                Connexion = new Connexion(this);
                Connexion.Show();
            }
        }

        public void init()
        {
            try
            {
                using (var sqlConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["DOCUMAT"].ConnectionString))
                {
                   sqlConnexion.Open();

                    //Réquète de vérification de la version de l'application 
                    using (var ct = new DocumatContext())
                    {
                        if(!ct.Configs.Any(c => c.VersionApp.Trim() == VERSION_APP.Trim() && c.NomApp.Trim().ToUpper() == "DOCUMAT".ToUpper()))
                        {
                            MessageBox.Show("La version que vous utilisez est obsolète,\n" +
                                            "veuillez vous proccurer la nouvelle version SVP !!!", "MISE A JOUR NECESSAIRE", MessageBoxButton.OK, MessageBoxImage.Warning);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void myGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            myGif.Play();
        }

        private void ConnexionLuncher()
        {
            // Définition du Timer Après le Loading
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // initialisation de la Base de Données
            try
            {
                init();
                ConnexionLuncher();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERREUR : BASE DE DONNEES", MessageBoxButton.OK, MessageBoxImage.Error);
                Parametre parametre = new Parametre();
                parametre.Show();
                this.Close();
                //if (MessageBox.Show("VOULEZ VOUS CREER LA BASE DE DONNEES  ? ", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //{
                //    try
                //    {
                //        // on crée la base
                //        using (var context = new DocumatContext())
                //        {
                //            Database.SetInitializer(new DocumatDbInitializer());
                //            context.Database.CreateIfNotExists();
                //            context.Database.Initialize(true);
                //        }
                //        ConnexionLuncher();
                //    }
                //    catch (Exception ex_ex)
                //    {
                //        MessageBox.Show(ex_ex.Message, "ERREUR : BASE DE DONNEES", MessageBoxButton.OK, MessageBoxImage.Error);
                //        this.Close();
                //    }
                //}
                //else
                //{
                //    Parametre parametre = new Parametre();
                //    parametre.Show();
                //    this.Close();
                //}
            }

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
        }

        private void Invoke(Action<int> progres, long ticks)
        {
            throw new NotImplementedException();
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

