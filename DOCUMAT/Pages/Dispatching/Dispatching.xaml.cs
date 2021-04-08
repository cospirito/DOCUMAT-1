using DOCUMAT.DataModels;
using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DOCUMAT.Pages.Dispatching
{
    /// <summary>
    /// Logique d'interaction pour Dispatching.xaml
    /// </summary>
    public partial class Dispatching : Window
    {
        List<ListViewItem> listViewItems;
        List<Models.Agent> AgentActuels = new List<Models.Agent>();
        bool AllComponentisLoad = false;
        public Models.Agent Utilisateur { get; }
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<RegistreDataModel> ListRegistreActuel = new List<RegistreDataModel>();
        int parRecup = Int32.Parse(ConfigurationManager.AppSettings["perRecup"]);

        public void RefreshRegistre()
        {
            try
            {

                if (cbChoixStatut.SelectedIndex == 0)
                {
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, true, (int)Enumeration.Registre.SCANNE).ToList();
                }
                else if (cbChoixStatut.SelectedIndex == 1)
                {
                    //Remplissage de la list de registre
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, false, (int)Enumeration.Registre.SCANNE).ToList();
                }
                else
                {
                    //Remplissage de la list de registre
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsListByStatus((int)Enumeration.Registre.SCANNE).ToList();
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
            RefreshData.WorkerSupportsCancellation = true;
            RefreshData.WorkerReportsProgress = true;
            RefreshData.DoWork += RefreshData_DoWork;
            RefreshData.RunWorkerCompleted += RefreshData_RunWorkerCompleted;
            RefreshData.Disposed += RefreshData_Disposed;
            RefreshData.ProgressChanged += RefreshData_ProgressChanged;
        }

        private void RefreshData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            #region SET PROGRESSION 
            // Affichage de la progression
            double widthPro = (BtnAnnuleChargement.ActualWidth * e.ProgressPercentage) / 100;

            if (e.ProgressPercentage > 97)
            {
                BtnAnnuleChargement.BorderBrush = Brushes.LightGreen;
                BtnAnnuleChargement.Foreground = Brushes.White;
            }

            if (e.ProgressPercentage > 40)
            {
                BtnAnnuleChargement.Foreground = Brushes.White;
            }

            if (e.ProgressPercentage > 2)
            {
                LoadText.Text = e.ProgressPercentage.ToString();
                LoadIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Percent;
                BtnAnnuleChargement.BorderThickness = new Thickness(widthPro, 1, 0, 1);
            }
            #endregion
        }

        private void RefreshData_Disposed(object sender, EventArgs e)
        {
            try
            {
                PanelLoader.Visibility = Visibility.Collapsed;
                PanelData.Visibility = Visibility.Visible;
                panelRecherche.IsEnabled = true;

                #region RESET PROGRESSION 
                // Affichage de la progression
                LoadText.Text = "Annuler";
                LoadIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                BtnAnnuleChargement.BorderThickness = new Thickness(0, 1, 0, 1);
                BtnAnnuleChargement.Foreground = Brushes.PaleVioletRed;
                BtnAnnuleChargement.BorderBrush = Brushes.PaleVioletRed;
                #endregion
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
                ListRegistreActuel = e.Result as List<RegistreDataModel>;
                dgRegistre.ItemsSource = ListRegistreActuel;
                PanelLoader.Visibility = Visibility.Collapsed;
                PanelData.Visibility = Visibility.Visible;
                panelRecherche.IsEnabled = true;

                #region RESET PROGRESSION 
                // Affichage de la progression
                LoadText.Text = "Annuler";
                LoadIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle;
                BtnAnnuleChargement.BorderThickness = new Thickness(0, 1, 0, 1);
                BtnAnnuleChargement.Foreground = Brushes.PaleVioletRed;
                BtnAnnuleChargement.BorderBrush = Brushes.PaleVioletRed;
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void RefreshData_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if ((int)e.Argument == 0)
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        // récupération du nombre total de registres au statut scanné
                        string reqCount = $"SELECT COUNT(DISTINCT RG.RegistreID) FROM Registres RG " +
                                          $"INNER JOIN TRAITEMENTS TR ON TR.TableID = RG.RegistreID " +
                                          $"WHERE RG.StatutActuel = {(int)Enumeration.Registre.SCANNE} " +
                                          $"AND Lower(TR.TableSelect) = Lower('Registres') AND TR.TypeTraitement = {(int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION}";
                        int nbLigne = ct.Database.SqlQuery<int>(reqCount).FirstOrDefault();

                        // Liste à récupérer
                        List<RegistreDataModel> registreDataModels = new List<RegistreDataModel>();
                        if (nbLigne > parRecup)
                        {
                            nbRecup = (int)Math.Ceiling(((float)nbLigne / (float)parRecup));
                        }

                        for (int i = 0; i < nbRecup; i++)
                        {
                            if (RefreshData.CancellationPending == false)
                            {
                                int limitId = (registreDataModels.Count == 0) ? 0 : registreDataModels.LastOrDefault().RegistreID;
                                registreDataModels.AddRange(RegistreView.GetRegistreDataByTrait((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION,limitId,parRecup, (int)Enumeration.Registre.SCANNE).ToList());
                                RefreshData.ReportProgress((int)Math.Ceiling(((float)i / (float)nbRecup) * 100));
                            }
                            else
                            {
                                e.Result = registreDataModels;
                                return;
                            }
                        }

                        //Remplissage de la list de registre
                        e.Result = registreDataModels;
                    }
                }
                else if ((int)e.Argument == 1)
                {
                    //Remplissage de la list de registre
                    List<RegistreDataModel> registreAttr = RegistreView.GetRegistreDataByTrait((int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, (int)Enumeration.Registre.SCANNE).ToList();
                    List<RegistreDataModel> AllRegistre = RegistreView.GetRegistreData((int)Enumeration.Registre.SCANNE).ToList();

                    var registreAttrStat = from rAll in AllRegistre
                                           join rAtt in registreAttr on rAll.RegistreID equals rAtt.RegistreID
                                           select rAll;
                    e.Result = AllRegistre.Except(registreAttrStat).ToList();
                }
                else
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        // récupération du nombre total de registres au statut scanné
                        int nbLigne = ct.Database.SqlQuery<int>($"SELECT COUNT(RegistreID) FROM Registres WHERE StatutActuel = {(int)Enumeration.Registre.SCANNE}").FirstOrDefault();

                        // Liste à récupérer
                        List<RegistreDataModel> registreDataModels = new List<RegistreDataModel>();
                        if(nbLigne > parRecup)
                        {
                            nbRecup = (int)Math.Ceiling(((float)nbLigne / (float)parRecup));
                        }

                        for (int i = 0; i < nbRecup; i++)
                        {
                            if(RefreshData.CancellationPending == false)
                            {
                                int limitId = (registreDataModels.Count == 0) ? 0 : registreDataModels.LastOrDefault().RegistreID;
                                registreDataModels.AddRange(RegistreView.GetRegistreData(limitId, parRecup,(int)Enumeration.Registre.SCANNE).ToList());
                                RefreshData.ReportProgress((int)Math.Ceiling(((float)i / (float)nbRecup) * 100));
                            }
                            else
                            {
                                e.Result  = registreDataModels;
                                return;
                            }
                        }

                        //Remplissage de la list de registre
                        e.Result = registreDataModels;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public Dispatching(Models.Agent user) : this()
        {
            this.Utilisateur = user;
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    List<RegistreDataModel> registreData = ListRegistreActuel as List<RegistreDataModel>;

                    switch (cbChoixRecherche.SelectedIndex)
                    {
                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 1:
                            // Récupération des registre par service
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomComplet.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 2:
                            // Récupération des registre par Région
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomRegion.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 3:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.Numero.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                    }
                }
                else
                {
                    dgRegistre.ItemsSource = ListRegistreActuel;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
                dgRegistre.ContextMenu = cm;
                AllComponentisLoad = true;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void AttRegistre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgRegistre.IsEnabled = false;
                panelRecherche.IsEnabled = false;
                panelSelection.Visibility = Visibility.Visible;
                IsSelectionAgent.IsChecked = true;
                FindAgent(0);
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void FindAgent(int IdAgent)
        {
            try
            {                
                using (var ct = new DocumatContext())
                {
                    if (IdAgent == 0)
                    {
                        AgentActuels = ct.Agent.Where(a => a.Affectation == (int)Enumeration.AffectationAgent.INDEXATION).OrderBy(a=>a.Login).ToList();
                    }
                    else
                    {
                        AgentActuels = ct.Agent.Where(a => a.Affectation == (int)Enumeration.AffectationAgent.INDEXATION && a.AgentID == IdAgent).ToList();
                    }

                    setListAgent(AgentActuels);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void setListAgent(List<Models.Agent> agents)
        {
            listViewItems = new List<ListViewItem>();
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
                    Height = 70
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

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (IsSelectionAgent.IsChecked == true)
                {
                    if (dgRegistre.SelectedItems.Count > 0)
                    {
                        if (MessageBox.Show("Voulez vous attribuer ces registres à Cet Agent ?", "ATTRIBUER A L'AGENT ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            using (var ct = new DocumatContext())
                            {
                                List<RegistreDataModel> registreDatas = dgRegistre.SelectedItems.Cast<RegistreDataModel>().ToList();

                                foreach (RegistreDataModel registreData in registreDatas)
                                {
                                    //RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                                    Traitement traitement = ct.Traitement.Where(t => t.TableSelect.ToUpper().Contains(DocumatContext.TbRegistre.ToUpper()) && t.TableID == registreData.RegistreID
                                           && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).FirstOrDefault();
                                    ListViewItem listViewItem = (ListViewItem)sender;
                                    int IdAgent = Int32.Parse(listViewItem.Tag.ToString());
                                    Models.Agent agent = ct.Agent.Where(a => a.AgentID == IdAgent).FirstOrDefault();

                                    if (traitement == null)
                                    {
                                        // traitement de l'attribution 
                                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreData.RegistreID, agent.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION, "ATTRIBUTION POUR INDEXATION PAR AGENT ID N° " + Utilisateur.AgentID);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Impossible, ce registre est attribué à un autre agent !!!", "IMPOSSIBLE", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }

                                // remise en place de l'affichage
                                dgRegistre.IsEnabled = true;
                                panelRecherche.IsEnabled = true;
                                panelSelection.Visibility = Visibility.Collapsed;
                                IsSelectionAgent.IsChecked = false;
                                MessageBox.Show("Le ou les registres ont bien été attibués", "Registres attribués !!", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
                    MenuItem menuItemAtt = (MenuItem)cm.Items.GetItemAt(0);
                    MenuItem menuItemRet = (MenuItem)cm.Items.GetItemAt(1);
                    RegistreDataModel registreData = (RegistreDataModel)dgRegistre.SelectedItem;

                    using (var ct = new DocumatContext())
                    {
                        Models.Traitement traitement = ct.Traitement.Where(t => t.TableSelect.ToUpper().Contains(DocumatContext.TbRegistre.ToUpper()) && t.TableID == registreData.RegistreID
                                && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).FirstOrDefault();
                        if (traitement != null)
                        {
                            FindAgent(traitement.AgentID);
                            menuItemAtt.IsEnabled = false;
                            menuItemRet.IsEnabled = true;
                        }
                        else
                        {
                            ListAgentSelected.ItemsSource = null;
                            menuItemAtt.IsEnabled = true;
                            menuItemRet.IsEnabled = false;
                        }
                    }
                }
                else
                {
                    ListAgentSelected.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
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
            if (AllComponentisLoad)
            {
                BtnRefresh_Click(sender, e);
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    List<RegistreDataModel> registreData = ListRegistreActuel as List<RegistreDataModel>;

                    switch (cbChoixRecherche.SelectedIndex)
                    {
                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 1:
                            // Récupération des registre par service
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomComplet.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 2:
                            // Récupération des registre par Région
                            dgRegistre.ItemsSource = registreData.Where(r => r.NomRegion.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                        case 3:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreData.Where(r => r.Numero.ToUpper().Contains(TbRechercher.Text.ToUpper())).ToList();
                            break;
                    }
                }
                else
                {
                    dgRegistre.ItemsSource = ListRegistreActuel;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshData.RunWorkerAsync(cbChoixStatut.SelectedIndex);
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

        private void RetirerRegistre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez vous retirer ces registres à Cet Agent ?", "RETIRER A L'AGENT ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        List<RegistreDataModel> registreDatas = dgRegistre.SelectedItems.Cast<RegistreDataModel>().ToList();

                        foreach (RegistreDataModel registreData in registreDatas)
                        {
                            using (var ct = new DocumatContext())
                            {
                                //RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                                Models.Traitement traitement = ct.Traitement.Where(t => t.TableSelect.ToUpper().Contains(DocumatContext.TbRegistre.ToUpper()) && t.TableID == registreData.RegistreID
                                        && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).FirstOrDefault();

                                if (traitement != null)
                                {
                                    // Retrait du traitement 
                                    ct.Traitement.Remove(traitement);
                                    ct.SaveChanges();

                                    // traitement de l'attribution 
                                    DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreData.RegistreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "RETRAIT INDEXATION A AGENT ID N° " + traitement.AgentID);
                                }
                                else
                                {
                                    MessageBox.Show("Impossible, ce registre n'est plus attribué !!!", $"ERREUR REGISTRE : {registreData.QrCode}", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        // remise en place de l'affichage
                        dgRegistre.IsEnabled = true;
                        panelRecherche.IsEnabled = true;
                        panelSelection.Visibility = Visibility.Collapsed;
                        IsSelectionAgent.IsChecked = false;
                        MessageBox.Show("Le ou les registres ont bien été Retiré", "Registres Retirés!!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void TbRechercherAgent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(TbRechercherAgent.Text.Trim()))
            {
                setListAgent(AgentActuels);
            }
            else
            {
                setListAgent(AgentActuels.Where(a => a.Login.ToLower().Trim().Contains(TbRechercherAgent.Text.Trim().ToLower()) || a.Noms.ToLower().Trim().Contains(TbRechercherAgent.Text.Trim().ToLower())).ToList());
            }
        }

        private void BtnAnnuleChargement_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Voulez vous vraiment annuler l'opération ?","Annuler",MessageBoxButton.YesNo,MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                RefreshData.CancelAsync();
            }
        }
    }
}
