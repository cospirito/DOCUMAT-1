using System;
using System.Collections.Generic;
using System.Linq;
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

        private void rechercheAgent()
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    dgAgent.ItemsSource = AgentView.GetViewsList().Where(av => av.Agent.Noms.ToLower().Contains(TbRechercher.Text.ToLower())).ToList();
                }
                else
                {
                    refreshListAgent();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

        public GestionAgent()
        {
            InitializeComponent();
        }

        public GestionAgent(Models.Agent user):this()
        {
            Utilisateur = user;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshListAgent();
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            rechercheAgent();
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
                        AgentView agentView = new AgentView();
                        List<int> agentIds = new List<int>();
 
                        foreach (AgentView item in dgAgent.SelectedItems)
                        {
                            agentIds.Add(item.Agent.AgentID);
                            agentView.context.Agent.Remove(agentView.context.Agent.FirstOrDefault(a => a.AgentID == item.Agent.AgentID));
                        }
                        agentView.context.SaveChanges();

                        foreach(int id in agentIds)
                        {
                            // Enregistrement du traitement de l'agent 
                            DocumatContext.AddTraitement(DocumatContext.TbAgent,id, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION);
                        }
                        refreshListAgent();
                        MessageBox.Show("Suppression éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
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
            rechercheAgent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
            dgAgent.ContextMenu = cm;
            MenuItem menuItem = (MenuItem)cm.Items.GetItemAt(2);
            refreshListAgent();

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
    }
}
