using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DOCUMAT
{
    /// <summary>
    /// Logique d'interaction pour Connexion.xaml
    /// </summary>
    public partial class Connexion : Window
    {
        Menu Menu = null;
        public Connexion()
        {
            InitializeComponent();
        }

        private void login_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "Login")
            {
                tb.Text = "";
            }
        }

        private void Pwd_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void btn_seconnecter_Click(object sender, RoutedEventArgs e)
        {
            //var versionInfo = FileVersionInfo.GetVersionInfo(pathToExe);
            //string version = versionInfo.FileVersion;
            Models.Agent agent = null;
            using (var ct = new DocumatContext())
            {
                agent = ct.Agent.FirstOrDefault(a => a.Login.ToLower() == login.Text.Trim().ToLower() && a.Mdp == Pwd.Password); 
            }
            if (agent != null)
            {
                try
                {
                    //Enregistrement de la connexion agent 
                    using (var ct = new DocumatContext())
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
                    }
                    this.Close();
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
            else
            {
                MessageBox.Show("Le Login ou le mot de passe incorrecte", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            login.Focus();
        }

        private void Pwd_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(login.Text) && !string.IsNullOrWhiteSpace(Pwd.Password))
                {
                    btn_seconnecter_Click(null, null);
                }
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
            if (Menu == null)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
