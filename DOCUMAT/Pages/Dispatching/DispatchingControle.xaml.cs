using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DOCUMAT.Pages.Dispatching
{
    /// <summary>
    /// Logique d'interaction pour DispatchingControle.xaml
    /// </summary>
    public partial class DispatchingControle : Window
    {
        List<ListViewItem> listViewItems;
        public bool AllComponentisLoad = false;
        public Models.Agent Utilisateur { get; }
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<Unite> ListUnites = new List<Unite>();

        public DispatchingControle()
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

        private void RefreshData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                ListUnites = e.Result as List<Unite>;
                dgUnite.ItemsSource = ListUnites;
                PanelLoader.Visibility = Visibility.Collapsed;
                PanelData.Visibility = Visibility.Visible;
                panelRecherche.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void RefreshData_DoWork(object sender, DoWorkEventArgs e)
        {
            // Définition de la methode de récupération des données 
            // Récupération de l'argument de la phase 
            var param = (object[])e.Argument;
            var phaseData = (int)param[0];
            var tranche = (Models.Tranche)param[1];

            try
            {
                using (var ct = new DocumatContext())
                {
                    if (phaseData == 0)
                    {
                        List<Models.Unite> unites = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID).ToList();
                        List<Models.Traitement> traitementsAttrUnite = ct.Traitement.Where(t => t.TableSelect.ToUpper() == DocumatContext.TbUnite.ToUpper()
                                                && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_ATTIBUE).ToList();

                        var jointure = from u in unites
                                       join t in traitementsAttrUnite on u.UniteID equals t.TableID
                                       select u;
                        e.Result = jointure.ToList();
                    }
                    else if (phaseData == 1)
                    {

                        List<Models.Unite> unites = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID).ToList();
                        List<Models.Traitement> traitementsAttrUnite = ct.Traitement.Where(t => t.TableSelect.ToUpper() == DocumatContext.TbUnite.ToUpper()
                                                && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_ATTIBUE).ToList();

                        var jointure = from u in unites
                                       join t in traitementsAttrUnite on u.UniteID equals t.TableID
                                       select u;
                        e.Result = unites.Except(jointure).ToList();
                    }
                    else
                    {
                        e.Result = ct.Unites.Where(u => u.TrancheID == tranche.TrancheID).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public DispatchingControle(Models.Agent user) : this()
        {
            this.Utilisateur = user;
        }

        private void BtnActualiser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.Tranche tranche = (Models.Tranche)cbChoixTranche.SelectedItem;
                object[] arg = new object[] { cbChoixStatut.SelectedIndex, tranche };
                RefreshData.RunWorkerAsync(arg);
                PanelLoader.Visibility = Visibility.Visible;
                PanelData.Visibility = Visibility.Collapsed;
                panelRecherche.IsEnabled = false;
            }
            catch (Exception ex)
            {
                //ex.ExceptionCatcher();
                RefreshData.CancelAsync();
                PanelLoader.Visibility = Visibility.Collapsed;
                PanelData.Visibility = Visibility.Visible;
                panelRecherche.IsEnabled = true;
            }
        }

        private void dgUnite_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void dgUnite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUnite.SelectedItems.Count == 1)
            {
                using (var ct = new DocumatContext())
                {
                    ContextMenu cm = this.FindResource("cmUnite") as ContextMenu;
                    MenuItem menuItem = (MenuItem)cm.Items.GetItemAt(0);
                    Models.Unite unite = (Models.Unite)dgUnite.SelectedItem;
                    Models.Traitement traitement = ct.Traitement.FirstOrDefault(t => t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_ATTIBUE
                    && t.TableSelect == DocumatContext.TbUnite && t.TableID == unite.UniteID);

                    if (traitement != null)
                    {
                        FindAgent(traitement.AgentID);
                        menuItem.IsEnabled = false;
                    }
                    else
                    {
                        ListAgentSelected.ItemsSource = null;
                        menuItem.IsEnabled = true;
                    }
                }
            }
            else
            {
                ListAgentSelected.ItemsSource = null;
            }
        }

        private void AttUnite_Click(object sender, RoutedEventArgs e)
        {
            dgUnite.IsEnabled = false;
            panelRecherche.IsEnabled = false;
            panelSelection.Visibility = Visibility.Visible;
            IsSelectionAgent.IsChecked = true;
            FindAgent(0);
        }

        private void cbChoixStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllComponentisLoad)
            {
                BtnActualiser_Click(sender, e);
            }
        }

        private void IsSelectionAgent_Unchecked(object sender, RoutedEventArgs e)
        {
            dgUnite.IsEnabled = true;
            panelRecherche.IsEnabled = true;
            panelSelection.Visibility = Visibility.Collapsed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = this.FindResource("cmUnite") as ContextMenu;
                dgUnite.ContextMenu = cm;
                FindAgent(0);
                AllComponentisLoad = true;

                // Liste des Tranches 
                using (var ct = new DocumatContext())
                {
                    cbChoixTranche.ItemsSource = ct.Tranches.ToList();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbChoixTranche_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChoixTranche.SelectedItem != null)
            {
                BtnActualiser_Click(sender, e);
            }
        }

        private void FindAgent(int IdAgent)
        {
            listViewItems = new List<ListViewItem>();
            using (var ct = new DocumatContext())
            {
                List<Models.Agent> agents;
                if (IdAgent == 0)
                {
                    agents = ct.Agent.Where(a => a.Affectation == (int)Enumeration.AffectationAgent.CONTROLE).ToList();
                }
                else
                {
                    agents = ct.Agent.Where(a => a.Affectation == (int)Enumeration.AffectationAgent.CONTROLE && a.AgentID == IdAgent).ToList();
                }

                foreach (Models.Agent agent in agents)
                {
                    Thickness thickness = new Thickness(20, 0, 0, 0);
                    StackPanel stackPanelInfo = new StackPanel() { Margin = thickness, VerticalAlignment = VerticalAlignment.Center };

                    System.Windows.Media.Imaging.BitmapImage bitmap;
                    if (!string.IsNullOrEmpty(agent.CheminPhoto))
                    {
                        bitmap = new BitmapImage(new Uri(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], agent.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
                    }
                    else
                    {
                        bitmap = new BitmapImage(new Uri("pack://application:,,,/Images/user.png"), new System.Net.Cache.RequestCachePolicy());
                    }

                    System.Windows.Controls.Image image = new System.Windows.Controls.Image()
                    {
                        Source = bitmap,
                        Height = 100
                    };

                    TextBlock nom = new TextBlock()
                    {
                        Text = "Nom : " + agent.Noms,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 20,
                        FontFamily = new System.Windows.Media.FontFamily("Agency Fb")
                    };

                    TextBlock login = new TextBlock()
                    {
                        Text = "Login : " + agent.Login,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 20,
                        FontFamily = new System.Windows.Media.FontFamily("Agency Fb")
                    };

                    stackPanelInfo.Children.Add(nom);
                    stackPanelInfo.Children.Add(login);

                    ListViewItem listViewItem = new ListViewItem() { Name = "agent" + agent.AgentID.ToString(), Tag = agent.AgentID.ToString() };
                    listViewItem.MouseDoubleClick += ListViewItem_MouseDoubleClick;
                    StackPanel stackPanelConteneur = new StackPanel() { Orientation = Orientation.Horizontal };
                    stackPanelConteneur.Children.Add(image);
                    stackPanelConteneur.Children.Add(stackPanelInfo);
                    listViewItem.Content = stackPanelConteneur;
                    listViewItems.Add(listViewItem);
                }
                ListAgentSelected.ItemsSource = listViewItems;
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (IsSelectionAgent.IsChecked == true)
                {
                    if (dgUnite.SelectedItems.Count > 0)
                    {
                        if (MessageBox.Show("Voulez vous attribuer à Cet Agent ?", "ATTRIBUER A L'AGENT ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            using (var ct = new DocumatContext())
                            {
                                List<Models.Unite> unites = dgUnite.SelectedItems.Cast<Models.Unite>().ToList();

                                foreach (Models.Unite unite in unites)
                                {
                                    //RegistreView registreView = (RegistreView)dgUnite.SelectedItem;
                                    Models.Traitement traitement = ct.Traitement.Where(t => t.TableSelect.ToUpper() == DocumatContext.TbUnite.ToUpper() && t.TableID == unite.UniteID
                                           && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_ATTIBUE).FirstOrDefault();

                                    if (traitement == null)
                                    {
                                        ListViewItem listViewItem = (ListViewItem)sender;
                                        int IdAgent = Int32.Parse(listViewItem.Tag.ToString());
                                        Models.Agent agent = ct.Agent.Where(a => a.AgentID == IdAgent).FirstOrDefault();
                                        // traitement de l'attribution 
                                        DocumatContext.AddTraitement(DocumatContext.TbUnite, unite.UniteID, agent.AgentID, (int)Enumeration.TypeTraitement.CONTROLE_ATTIBUE, "ATTRIBUTION CONTROLE UNITE PAR AGENT ID N° : " + Utilisateur.AgentID);

                                        // remise en place de l'affichage
                                        dgUnite.IsEnabled = true;
                                        panelRecherche.IsEnabled = true;
                                        panelSelection.Visibility = Visibility.Collapsed;
                                        IsSelectionAgent.IsChecked = false;
                                        BtnActualiser_Click(sender, e);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Impossible, ce registre est attribué à un autre agent !!!", "IMPOSSIBLE", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}
