using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DOCUMAT.Models;
using DOCUMAT.ViewModels;

namespace DOCUMAT.Pages.Agent
{
    /// <summary>
    /// Logique d'interaction pour GestionAgent.xaml
    /// </summary>
    public partial class GestionAgent : Window
    {
        public Models.Agent Utilisateur;
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<AgentView> ListAgentActuel = new List<AgentView>();

        #region Mes Fonctions

        // actualiser la Datagrid
        public void refreshListAgent()
        {
            try
            {
                //Remplissage partial de  la list
                dgAgent.ItemsSource = AgentView.GetViewsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error); ;
            }
        }
        #endregion

        public GestionAgent()
        {
            InitializeComponent();
            RefreshData.WorkerSupportsCancellation = true;
            RefreshData.DoWork += RefreshData_DoWork;
            RefreshData.RunWorkerCompleted += RefreshData_RunWorkerCompleted;
            RefreshData.Disposed += RefreshData_Disposed;
        }

        private void RefreshData_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RefreshData_DoWork(object sender, DoWorkEventArgs e)
        {
            // Définition de la methode de récupération des données 
            try
            {
                List<AgentView> AgentViews = new List<AgentView>();
                AgentViews = AgentView.GetViewsList();
                e.Result = AgentViews;
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
                ListAgentActuel = e.Result as List<AgentView>;
                dgAgent.ItemsSource = ListAgentActuel;
                PanelLoader.Visibility = Visibility.Collapsed;
                panelInteracBtn.IsEnabled = true;
                dgAgent.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public GestionAgent(Models.Agent user):this()
        {
            Utilisateur = user;
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    dgAgent.ItemsSource = ListAgentActuel.Where(av => av.Agent.Noms.ToLower().Contains(TbRechercher.Text.ToLower())).ToList();
                }
                else
                {
                    dgAgent.ItemsSource = ListAgentActuel;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgAgent.SelectedItems.Count > 0)
            {
                FormAgent formAgent = new FormAgent(this, ((AgentView)dgAgent.SelectedItem).Agent,Utilisateur) ;
                this.IsEnabled = false;
                formAgent.Show();
            }
        }

        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgAgent.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez-vous vraiment supprimer cet élément ?", "ATTENTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        using (var ct = new DocumatContext())
                        {
                            AgentView agentView = new AgentView();
                            List<int> agentIds = new List<int>();

                            foreach (AgentView item in dgAgent.SelectedItems)
                            {
                                agentIds.Add(item.Agent.AgentID);
                                ct.SessionTravails.RemoveRange(ct.SessionTravails.Where(st => st.AgentID == item.Agent.AgentID));
                                ct.Agent.Remove(ct.Agent.FirstOrDefault(a => a.AgentID == item.Agent.AgentID));
                            }
                            ct.SaveChanges(); 

                            foreach(int id in agentIds)
                            {
                                // Enregistrement du traitement de l'agent 
                                DocumatContext.AddTraitement(DocumatContext.TbAgent,id, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION);
                            }
                            refreshListAgent();
                            MessageBox.Show("Suppression éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez sélecctionner au moins un élément ", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    dgAgent.ItemsSource = ListAgentActuel.Where(av => av.Agent.Noms.ToLower().Contains(TbRechercher.Text.ToLower())).ToList();
                }
                else
                {
                    dgAgent.ItemsSource = ListAgentActuel;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
            dgAgent.ContextMenu = cm;
            MenuItem menuItem = (MenuItem)cm.Items.GetItemAt(2);

            switch (Utilisateur.Affectation)
            {
                case (int)Models.Enumeration.AffectationAgent.ADMINISTRATEUR:
                    menuItem.IsEnabled = true;
                    break;
                case (int)Models.Enumeration.AffectationAgent.SUPERVISEUR:
                    menuItem.IsEnabled = false;
                    break;
                default:
                    menuItem.IsEnabled = false;
                    break;
            }

            //Chargement des éléments
            BtnActualise_Click(sender, e);
        }

        private void dgAgent_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            AgentView view = (AgentView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }

        private void BtnAddAgent_Click(object sender, RoutedEventArgs e)
        {
            FormAgent formAgent = new FormAgent(this,Utilisateur);
            this.IsEnabled = false;
            formAgent.Show();
        }

        private void BtnActualise_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshData.RunWorkerAsync(Utilisateur);
                PanelLoader.Visibility = Visibility.Visible;
                panelInteracBtn.IsEnabled = false;
                dgAgent.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                //ex.ExceptionCatcher();
                RefreshData.CancelAsync();
                PanelLoader.Visibility = Visibility.Collapsed;
                panelInteracBtn.IsEnabled = true;
                dgAgent.Visibility = Visibility.Visible;
            }
        }
    }
}
