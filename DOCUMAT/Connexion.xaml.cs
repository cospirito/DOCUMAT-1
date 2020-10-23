using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DOCUMAT
{
    /// <summary>
    /// Logique d'interaction pour Connexion.xaml
    /// </summary>
    public partial class Connexion : Window
    {
        ApplicationLoader Loader = null;
        Menu Menu = null;
        public Connexion()
        {
            InitializeComponent();
        }

        public Connexion(ApplicationLoader loader):this()
        {
            Loader = loader;
        }

        private void login_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "Login")
                tb.Text = "";
        }

        private void Pwd_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void btn_seconnecter_Click(object sender, RoutedEventArgs e)
        {
            AgentView agentView = new AgentView();
            Models.Agent agent = agentView.context.Agent.FirstOrDefault(a => a.Login.ToLower() == login.Text.Trim().ToLower() && a.Mdp == Pwd.Password);
            if ( agent != null)
            {
                //Enregistrement de la connexion agent 
                using(var ct = new DocumatContext())
                {
                    Models.SessionTravail sessionTravail = new SessionTravail()
                    {
                         AgentID = agent.AgentID,
                         DateCreation = DateTime.Now,
                         DateDebut = DateTime.Now,
                         DateModif = DateTime.Now,                          
                    };

                    sessionTravail = ct.SessionTravails.Add(sessionTravail);
                    ct.SaveChanges();

                    Menu = new Menu(agent, sessionTravail);
                    Menu.Show();
                    this.Close();
                }
            }

            else
            {
                MessageBox.Show("Le Login ou le mot de passe incorrecte", "ATTENTION",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            login.Focus();
            if(Loader != null)
            {
                Loader.dispatcherTimer.Stop();
                Loader.Close();
            }
        }

        private void Pwd_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(login.Text) && !string.IsNullOrWhiteSpace(Pwd.Password))
                    btn_seconnecter_Click(null,null);
            }
        }

        private void login_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Pwd.Focus();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Menu == null)
                Application.Current.Shutdown();
        }
    }
}
