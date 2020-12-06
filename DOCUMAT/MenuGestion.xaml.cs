using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DOCUMAT.Models;
using DOCUMAT.Pages.Scannerisation;

namespace DOCUMAT
{
    /// <summary>
    /// Logique d'interaction pour Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        // Définition du timer de rafraichissement de la session de travail
        System.Timers.Timer Timer;
        Models.Agent Utilisateur;
        Models.SessionTravail UserSession;
        Pages.Home.Admin Accueil;
        Pages.Service.Main Service;
        Pages.Inventaire.Inventaire Inventaire;
        Pages.Preindexation.Preindexation Preindexation;
        Pages.Scannerisation.Scannerisation Scannerisation;
        Pages.Indexation.Indexation Indexation;
        Pages.Controle.Controle Controle;
        Pages.Correction.Correction Correction;
        
        Pages.Parametre.Parametre Parametre;
        Pages.Agent.GestionAgent GestionAgent;
        Pages.Dispatching.Dispatching Dispatching; //Dispatching Indexation
        Pages.Dispatching.DispatchingControle DispatchingControle; //Dispatching Indexation
        Pages.Dispatching.DispatchingCorrection DispatchingCorrection; //Dispatching Indexation

        bool isDeconex = false;
        // Index du Btn Module sélectionné 
        int IndexSelect = 0;

        public Menu()
        {
            InitializeComponent();
        }

        public Menu(Models.Agent agent,SessionTravail sessionTravail):this()
        {
            #region GESTION DES PRIVILEGES UTILISATEUR
            try
            {
                // Définition des paramètres de l'utilisateur connecté !!!
                Utilisateur = agent;
                UserSession = sessionTravail;
                if (!string.IsNullOrWhiteSpace(agent.CheminPhoto))
                {
                    if(File.Exists(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"],Utilisateur.CheminPhoto)))
                    {
                        try
                        {
                            FileInfo file = new FileInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], Utilisateur.CheminPhoto));
                            //ImgPhotoUser.Source = BitmapFrame.Create(new Uri(Path.Combine(Directory.GetCurrentDirectory(), Utilisateur.CheminPhoto)), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                            ImgPhotoUser.Source = BitmapFrame.Create(new Uri(file.FullName), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                        }
                        catch (Exception ex){MessageBox.Show(ex.Message, "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);}
                    }
                }
                tbxUserName.Text = Utilisateur.Login;
                tbxTypeUser.Text = Enum.GetName(typeof(Enumeration.AffectationAgent), Utilisateur.Affectation);

                //Controle des droits de l'agent
                switch (Utilisateur.Affectation)
                {
                    case 0: //INVENTAIRE
                        BtnINVENTAIRE.IsEnabled = true;
                        BtnPREINDEXATION.IsEnabled = true;
                        BtnSCANNERISATION.IsEnabled = false;
                        BtnINDEXATION.IsEnabled = false;
                        BtnCONTROLE.IsEnabled = false;
                        BtnCORRECTION.IsEnabled = false;
                        BtnGestionAgent.IsEnabled = false;
                        BtnParametre.IsEnabled = false;
                        BtnDispatchAgent.IsEnabled = false;
                        BtnDispatchControle.IsEnabled = false;
                        BtnDispatchCorrection.IsEnabled = false;
                        break;
                    case 1: //SCAN
                        BtnINVENTAIRE.IsEnabled = false;
                        BtnPREINDEXATION.IsEnabled = false;
                        BtnSCANNERISATION.IsEnabled = true;
                        BtnINDEXATION.IsEnabled = false;
                        BtnCONTROLE.IsEnabled = false;
                        BtnCORRECTION.IsEnabled = false;
                        BtnGestionAgent.IsEnabled = false;
                        BtnDispatchAgent.IsEnabled = false;
                        BtnParametre.IsEnabled = false;
                        BtnDispatchControle.IsEnabled = false;
                        BtnDispatchCorrection.IsEnabled = false;
                        break;
                    case 2: //INDEXATION
                        BtnINVENTAIRE.IsEnabled = false;
                        BtnPREINDEXATION.IsEnabled = false;
                        BtnSCANNERISATION.IsEnabled = false;
                        BtnINDEXATION.IsEnabled = true;
                        BtnCONTROLE.IsEnabled = false;
                        BtnCORRECTION.IsEnabled = false;
                        BtnGestionAgent.IsEnabled = false;
                        BtnDispatchAgent.IsEnabled = false;
                        BtnParametre.IsEnabled = false;
                        BtnDispatchControle.IsEnabled = false;
                        BtnDispatchCorrection.IsEnabled = false;
                        break;
                    case 3: //CONTROLE
                        BtnINVENTAIRE.IsEnabled = false;
                        BtnPREINDEXATION.IsEnabled = false;
                        BtnSCANNERISATION.IsEnabled = false;
                        BtnINDEXATION.IsEnabled = false;
                        BtnCONTROLE.IsEnabled = true;
                        BtnCORRECTION.IsEnabled = false;
                        BtnGestionAgent.IsEnabled = false;
                        BtnDispatchAgent.IsEnabled = false;
                        BtnParametre.IsEnabled = false;
                        BtnDispatchControle.IsEnabled = false;
                        BtnDispatchCorrection.IsEnabled = false;
                        break;
                    case 4: // CORRECTION
                        BtnINVENTAIRE.IsEnabled = false;
                        BtnPREINDEXATION.IsEnabled = false;
                        BtnSCANNERISATION.IsEnabled = false;
                        BtnINDEXATION.IsEnabled = false;
                        BtnCONTROLE.IsEnabled = false;
                        BtnCORRECTION.IsEnabled = true;
                        BtnGestionAgent.IsEnabled = false;
                        BtnDispatchAgent.IsEnabled = false;
                        BtnParametre.IsEnabled = false;
                        BtnDispatchControle.IsEnabled = false;
                        BtnDispatchCorrection.IsEnabled = false;
                        break;
                    case 5: // SUPERVISION
                        BtnINVENTAIRE.IsEnabled = true;
                        BtnPREINDEXATION.IsEnabled = true;
                        BtnSCANNERISATION.IsEnabled = true;
                        BtnINDEXATION.IsEnabled = true;
                        BtnCONTROLE.IsEnabled = true;
                        BtnCORRECTION.IsEnabled = true;
                        BtnGestionAgent.IsEnabled = true;
                        BtnDispatchAgent.IsEnabled =true;
                        BtnParametre.IsEnabled = false;
                        BtnDispatchControle.IsEnabled = true;
                        BtnDispatchCorrection.IsEnabled = true;
                        break;
                    case 6: // ADMINISTRATEUR 
                        BtnINVENTAIRE.IsEnabled = true;
                        BtnPREINDEXATION.IsEnabled = true;
                        BtnSCANNERISATION.IsEnabled = true;
                        BtnINDEXATION.IsEnabled = true;
                        BtnCONTROLE.IsEnabled = true;
                        BtnCORRECTION.IsEnabled = true;
                        BtnGestionAgent.IsEnabled = true;
                        BtnDispatchAgent.IsEnabled = true;
                        BtnParametre.IsEnabled = true;
                        BtnDispatchControle.IsEnabled = true;
                        BtnDispatchCorrection.IsEnabled = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion
        }

        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = int.Parse(((Button)e.Source).Uid);
            IndexSelect = index;

            double tailleBtn = btn.ActualWidth;
            GridCursor.Margin = new Thickness(20 + (tailleBtn * index), 50, 0, 0);
            GridMain.Background = Brushes.SteelBlue;

            switch (index)
            {
                case 0:
                    this.ContenuAdd.Navigate(Accueil);
                    //ContenuAdd.Content = new Pages.Home.Admin();
                    break;
                case 1:
                    this.ContenuAdd.Navigate(new Pages.Inventaire.Inventaire(Utilisateur));
                    //ContenuAdd.Content = new Pages.Inventaire.Inventaire();
                    break;
                case 2:
                    this.ContenuAdd.Navigate(new Pages.Preindexation.Preindexation(Utilisateur));
                    //ContenuAdd.Content = new Pages.Preindexation.Preindexation();                    
                    break;
                case 3:
                    // SCANNERISATION
                    this.ContenuAdd.Navigate(new Pages.Scannerisation.Scannerisation(Utilisateur));
                    break;
                case 4:
                    this.ContenuAdd.Navigate(new Pages.Indexation.Indexation(Utilisateur));
                    //ContenuAdd.Content = new Pages.Indexation.Indexation();
                    break;
                case 5:
                    this.ContenuAdd.Navigate(new Pages.Controle.Controle(Utilisateur));
                    //ContenuAdd.Content = new Pages.Controle.Controle();
                    break;
                case 6:
                    this.ContenuAdd.Navigate(new Pages.Correction.Correction(Utilisateur));
                    //ContenuAdd.Content = new Pages.Correction.Correction();
                    break;
                default:
                    //GridCursor.Background = Brushes.DimGray;
                    this.ContenuAdd.Navigate(Accueil);
                    //ContenuAdd.Content = new Pages.Home.Admin();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Service = new Main();
            Accueil = new Pages.Home.Admin();
            Parametre = new Pages.Parametre.Parametre();
            GestionAgent = new Pages.Agent.GestionAgent(Utilisateur);
            Dispatching = new Pages.Dispatching.Dispatching(Utilisateur);
            DispatchingControle = new Pages.Dispatching.DispatchingControle(Utilisateur);
            DispatchingCorrection = new Pages.Dispatching.DispatchingCorrection(Utilisateur);
            this.ContenuAdd.Navigate(Accueil);

            // Définition du Timer de maintient de session 
            Timer = new System.Timers.Timer() { AutoReset = true, Interval = 1 * 60 * 1000 };
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }   

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Mise à jour de la session de Travail
            using(var ct = new DocumatContext())
            {
                try
                {
                    SessionTravail sessionTravail = ct.SessionTravails.FirstOrDefault(st => st.SessionTravailID == UserSession.SessionTravailID);
                    sessionTravail.DateModif = DateTime.Now;
                    ct.SaveChanges();
                }
                catch(Exception ex){ /*MessageBox.Show(ex.Message, "ERREUR ARRET SESSION", MessageBoxButton.OK, MessageBoxImage.Error);*/ }
            }
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GridCursor2.Visibility = Visibility.Visible;
            Button btn = (Button)sender;
            int index = int.Parse(((Button)e.Source).Uid);

            double tailleBtn = btn.ActualWidth;
            GridCursor2.Margin = new Thickness(20 + (tailleBtn * index), 50, 0, 0);
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GridCursor2.Visibility = Visibility.Collapsed;
            Button btn = (Button)sender;
            int index = int.Parse(((Button)e.Source).Uid);

            double tailleBtn = btn.ActualWidth;
            GridCursor2.Margin = new Thickness(0);
        }

        private void btnCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            btnCloseMenu.Visibility = Visibility.Collapsed;
            btnOpenMenu.Visibility = Visibility.Visible;
        }

        private void btnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            btnOpenMenu.Visibility = Visibility.Collapsed;
            btnCloseMenu.Visibility = Visibility.Visible;
        }

        private void BtnGestionAgent_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GestionAgent.IsVisible)
            {
                if (!GestionAgent.IsActive)
                {
                    GestionAgent.Activate();
                }
                GestionAgent.WindowState = WindowState.Normal;
            }
            else
            {
                GestionAgent = new Pages.Agent.GestionAgent(Utilisateur);
                GestionAgent.Show();
            }
        }

        private void BtnFermer_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(isDeconex != true)
            {
                if (MessageBox.Show("VOULEZ VOUS VRAIMENT FERMER L'APPLICATION ? ", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    try
                    {
                        // Enregistrement de la fin de la session de travail
                        using (var ct = new DocumatContext())
                        {
                            SessionTravail sessionTravail = ct.SessionTravails.FirstOrDefault(st => st.SessionTravailID == UserSession.SessionTravailID);
                            sessionTravail.DateFin = sessionTravail.DateModif = DateTime.Now;
                            if (ct.SaveChanges() > 0)
                            {
                                Timer.Stop();
                                Application.Current.Shutdown();
                            }
                            else
                            {
                                MessageBox.Show("La session a été stopper !!!", "ERREUR ARRET SESSION", MessageBoxButton.OK, MessageBoxImage.Error);
                                Application.Current.Shutdown();

                            }
                        }
                    }
                    catch (Exception){ Application.Current.Shutdown(); }
                }
            }
        }

        private void BtnDeconnex_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MessageBox.Show("DECONNECTER SE COMPTE ? ", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Enregistrement de la fin de la session de travail
                using (var ct = new DocumatContext())
                {
                    SessionTravail sessionTravail = ct.SessionTravails.FirstOrDefault(st => st.SessionTravailID == UserSession.SessionTravailID);
                    sessionTravail.DateFin = sessionTravail.DateModif = DateTime.Now;
                    ct.SaveChanges();

                    Connexion Connexion = new Connexion();
                    Connexion.Show();
                    isDeconex = true;
                    Timer.Stop();
                    this.Close();

                    // Fermeture des Fenetres Potentiellement Ouverte 
                    if(Dispatching.IsVisible)
                    {
                        Dispatching.Close();
                    }

                    if (DispatchingControle.IsVisible)
                    {
                        DispatchingControle.Close();
                    }

                    if(DispatchingCorrection.IsVisible)
                    {
                        DispatchingCorrection.Close();
                    }

                    if(Parametre.IsVisible)
                    {
                        Parametre.Close();
                    }

                    if(GestionAgent.IsVisible)
                    {
                        GestionAgent.Close();
                    }
                }
            }
        }

        private void ContenuAdd_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            ContenuAdd.NavigationService.RemoveBackEntry();
        }

        private void BtnParametre_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Parametre.IsVisible)
            {
                if (!Dispatching.IsActive)
                {
                    Parametre.Activate();
                }
                Parametre.WindowState = WindowState.Normal;
            }
            else
            {
                Parametre = new Pages.Parametre.Parametre();
                Parametre.Show();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Reéquilibre la taille minimal du GridMenuContainer height pour 
            // ne pas avoir de pied de page blanc
            if(e.HeightChanged)
            {
                Size size = e.NewSize;
                double minGridHeight = 500;
                double menuHeight = 700;

                if (size.Height > menuHeight)
                {
                    double ratio = size.Height - menuHeight;                   
                    GridMain.MinHeight = minGridHeight + ratio;
                }
                else
                {
                    GridMain.MinHeight = minGridHeight;
                }
            }

            if(e.WidthChanged)
            {
                if(e.NewSize.Width < 1140)
                {
                    BtnACCUEIL.Width = 100;
                    BtnACCUEIL.Content = "ACC..";
                    BtnINVENTAIRE.Width = 100;
                    BtnINVENTAIRE.Content = "INV..";
                    BtnPREINDEXATION.Width = 100;
                    BtnPREINDEXATION.Content = "PRE..";
                    BtnSCANNERISATION.Width = 100;
                    BtnSCANNERISATION.Content = "SCA..";
                    BtnINDEXATION.Width = 100;
                    BtnINDEXATION.Content = "IND..";
                    BtnCONTROLE.Width = 100;
                    BtnCONTROLE.Content = "CTR..";
                    BtnCORRECTION.Width = 100;
                    BtnCORRECTION.Content = "CRR..";
                    GridCursor.Width = 100;
                    GridCursor.Margin = new Thickness(20 + (100 * IndexSelect), 50, 0, 0);
                    GridCursor2.Width = 100;
                }
                else
                {
                    BtnACCUEIL.Width = 150;
                    BtnACCUEIL.Content = "ACCUEIL";
                    BtnINVENTAIRE.Width = 150;
                    BtnINVENTAIRE.Content = "INVENTAIRE";
                    BtnPREINDEXATION.Width = 150;
                    BtnPREINDEXATION.Content = "PREINDEXATION";
                    BtnSCANNERISATION.Width = 150;
                    BtnSCANNERISATION.Content = "SCANNERISATION";
                    BtnINDEXATION.Width = 150;
                    BtnINDEXATION.Content = "INDEXATION";
                    BtnCONTROLE.Width = 150;
                    BtnCONTROLE.Content = "CONTROLE";
                    BtnCORRECTION.Width = 150;
                    BtnCORRECTION.Content = "CORRECTION";
                    GridCursor.Width = 150;
                    GridCursor.Margin = new Thickness(20 + (150 * IndexSelect), 50, 0, 0);
                    GridCursor2.Width = 150;
                }
            }
        }

        private void BtnDispatchCorrection_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DispatchingCorrection.IsVisible)
            {
                if (!DispatchingCorrection.IsActive)
                {
                    DispatchingCorrection.Activate();
                }
                DispatchingCorrection.WindowState = WindowState.Normal;
            }
            else
            {
                DispatchingCorrection = new Pages.Dispatching.DispatchingCorrection(Utilisateur);
                DispatchingCorrection.Show();
            }
        }

        private void BtnDispatchControle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DispatchingControle.IsVisible)
            {
                if (!DispatchingControle.IsActive)
                {
                    DispatchingControle.Activate();
                }
                DispatchingControle.WindowState = WindowState.Normal;
            }
            else
            {
                DispatchingControle = new Pages.Dispatching.DispatchingControle(Utilisateur);
                DispatchingControle.Show();
            }
        }

        private void BtnDispatchAgent_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Dispatching.IsVisible)
            {
                if (!Dispatching.IsActive)
                {
                    Dispatching.Activate();
                }
                Dispatching.WindowState = WindowState.Normal;
            }
            else
            {
                Dispatching = new Pages.Dispatching.Dispatching(Utilisateur);
                Dispatching.Show();
            }
        }
    }
}
