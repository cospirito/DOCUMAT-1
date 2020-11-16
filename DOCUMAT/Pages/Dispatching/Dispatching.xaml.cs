using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
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
    /// Logique d'interaction pour Dispatching.xaml
    /// </summary>
    public partial class Dispatching : Window
    {
        List<ListViewItem> listViewItems ;
        bool AllComponentisLoad = false;

        public void RefreshRegistre()
        {
            try
            {
                
                if (cbChoixStatut.SelectedIndex == 0)
                {
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, true).Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();
                }
                else if (cbChoixStatut.SelectedIndex == 1)
                {
                    //Remplissage de la list de registre
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, false).Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();
                }
                else
                {
                    //Remplissage de la list de registre
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public Dispatching()
        {
            InitializeComponent();
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        
        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            RegistreView view = (RegistreView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cm;
            RefreshRegistre();
            FindAgent(0);
            AllComponentisLoad = true;
        }

        private void AttRegistre_Click(object sender, RoutedEventArgs e)
        {
            dgRegistre.IsEnabled = false;
            panelRecherche.IsEnabled = false;
            panelSelection.Visibility = Visibility.Visible;
            IsSelectionAgent.IsChecked = true;
            FindAgent(0);
        }

        private void FindAgent(int IdAgent)
        {
            listViewItems = new List<ListViewItem>();
            using (var ct = new DocumatContext())
            {
                List<Models.Agent> agents;
                if (IdAgent == 0)
                {
                    agents = ct.Agent.Where(a => a.Affectation == (int)Enumeration.AffectationAgent.INDEXATION).ToList();
                }
                else
                {
                    agents = ct.Agent.Where(a => a.Affectation == (int)Enumeration.AffectationAgent.INDEXATION && a.AgentID == IdAgent).ToList();
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
                        bitmap = new BitmapImage(new Uri("/Images/user.png"), new System.Net.Cache.RequestCachePolicy());
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

                    //TextBlock affection = new TextBlock()
                    //{
                    //    Text = "Aff.. : " + Enum.GetName(typeof(Enumeration.AffectationAgent), agent.Affectation),
                    //    VerticalAlignment = VerticalAlignment.Center,
                    //    FontSize = 20,
                    //    FontFamily = new System.Windows.Media.FontFamily("Agency Fb"),                        
                    //};
                    stackPanelInfo.Children.Add(nom);
                    stackPanelInfo.Children.Add(login);
                    //stackPanelInfo.Children.Add(affection);

                    ListViewItem listViewItem = new ListViewItem() { Name = "agent" + agent.AgentID.ToString(),Tag = agent.AgentID.ToString()};
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
            if(IsSelectionAgent.IsChecked == true)
            {
                if(dgRegistre.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez vous attribuer ces registres à Cet Agent ?", "ATTRIBUER A L'AGENT ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        foreach (RegistreView registreView in dgRegistre.SelectedItems)
                        {
                            //RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                            Models.Traitement traitement = registreView.context.Traitement.Where(t => t.TableSelect.ToUpper().Contains(DocumatContext.TbRegistre.ToUpper()) && t.TableID == registreView.Registre.RegistreID
                                   && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).FirstOrDefault();
                            ListViewItem listViewItem = (ListViewItem)sender;
                            int IdAgent = Int32.Parse(listViewItem.Tag.ToString());
                            Models.Agent agent = registreView.context.Agent.Where(a => a.AgentID == IdAgent).FirstOrDefault();

                            if (traitement == null)
                            {
                                // traitement de l'attribution 
                                DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.Registre.RegistreID, agent.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION);

                                // remise en place de l'affichage
                                dgRegistre.IsEnabled = true;
                                panelRecherche.IsEnabled = true;
                                panelSelection.Visibility = Visibility.Collapsed;
                                IsSelectionAgent.IsChecked = false;
                                RefreshRegistre();
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

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
                MenuItem menuItem = (MenuItem)cm.Items.GetItemAt(0);
                RegistreView registreView =  (RegistreView)dgRegistre.SelectedItem;
                Models.Traitement traitement = registreView.context.Traitement.Where(t => t.TableSelect.ToUpper().Contains(DocumatContext.TbRegistre.ToUpper()) && t.TableID == registreView.Registre.RegistreID
                        && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).FirstOrDefault();

                if(traitement != null)
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
            else
            {
                ListAgentSelected.ItemsSource = null;
            }
        }

        private void IsSelectionAgent_Unchecked(object sender, RoutedEventArgs e)
        {
            dgRegistre.IsEnabled = true;
            panelRecherche.IsEnabled = true;
            panelSelection.Visibility = Visibility.Collapsed;
        }

        private void cbChoixStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(AllComponentisLoad)
            {
                RefreshRegistre();
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (TbRechercher.Text != "")
                {
                    RegistreView RegistreView = new RegistreView();
                    List<RegistreView> registreViews = RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, true).ToList();

                    if (cbChoixStatut.SelectedIndex == 0)
                        registreViews = RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, true).ToList();
                    else if (cbChoixStatut.SelectedIndex == 1)
                        registreViews = RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, false).ToList();
                    else
                        registreViews = RegistreView.GetViewsList().ToList();

                    switch (cbChoixRecherche.SelectedIndex)
                    {

                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreViews.Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE && r.Registre.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper()));
                            break;
                        case 1:
                            // Récupération des registre par service
                            List<Models.Service> Services1 = RegistreView.context.Service.ToList();
                            List<Models.Livraison> Livraisons1 = RegistreView.context.Livraison.ToList();
                            List<Models.Versement> Versements1 = RegistreView.context.Versement.ToList();
                            List<RegistreView> registreViews1 = registreViews.Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();

                            var jointure1 = from r in registreViews1
                                            join v in Versements1 on r.Registre.VersementID equals v.VersementID into table1
                                            from v in table1.ToList()
                                            join l in Livraisons1 on v.LivraisonID equals l.LivraisonID into table2
                                            from l in table2.ToList()
                                            join s in Services1 on l.ServiceID equals s.ServiceID
                                            where s.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                            select r;
                            dgRegistre.ItemsSource = jointure1;
                            break;
                        case 2:
                            // Récupération des registre par service
                            List<Models.Region> Region2 = RegistreView.context.Region.ToList();
                            List<Models.Service> Services2 = RegistreView.context.Service.ToList();
                            List<Models.Livraison> Livraisons2 = RegistreView.context.Livraison.ToList();
                            List<Models.Versement> Versements2 = RegistreView.context.Versement.ToList();
                            List<RegistreView> registreViews2 = registreViews.Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();

                            var jointure2 = from r in registreViews2
                                            join v in Versements2 on r.Registre.VersementID equals v.VersementID into table1
                                            from v in table1.ToList()
                                            join l in Livraisons2 on v.LivraisonID equals l.LivraisonID into table2
                                            from l in table2.ToList()
                                            join s in Services2 on l.ServiceID equals s.ServiceID into table3
                                            from s in table3.ToList()
                                            join rg in Region2 on s.RegionID equals rg.RegionID
                                            where rg.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                            select r;
                            dgRegistre.ItemsSource = jointure2;
                            break;
                        case 3:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreViews.Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE && r.Registre.Numero.ToUpper().Contains(TbRechercher.Text.ToUpper()));
                            break;
                        default:
                            RefreshRegistre();
                            break;
                    }
                }
                else
                {
                    RefreshRegistre();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}
