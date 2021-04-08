using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DOCUMAT.Pages.Service
{
    /// <summary>
    /// Logique d'interaction pour Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        #region Mes Fonctions

        // actualiser la Datagrid
        private void refreshListServices()
        {
            try
            {
                //Remplissage partial de la list
                ServiceView serviceView = new ServiceView();
                dgService.ItemsSource = serviceView.GetViewsList();

            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        // Rechercher des éléments dans la table service
        private void rechercheService()
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    using (var ct = new DocumatContext())
                    {


                        dgService.ItemsSource = ServiceView.GetServiceViewsList(ct.Service.Where(s => s.Nom.Contains(TbRechercher.Text)
                                                                    || s.Code.Contains(TbRechercher.Text)
                                                                    || s.Description.Contains(TbRechercher.Text)
                                                                    || s.CheminDossier.Contains(TbRechercher.Text)
                                                                    || s.DateCreation.ToString().Contains(TbRechercher.Text)
                                                                    || s.DateModif.ToString().Contains(TbRechercher.Text)).ToList());
                    }
                }
                else
                {
                    refreshListServices();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        #endregion

        // définit si le formulaire est en mode ajout ou modif
        private bool EditMode = false;

        // Dossier racine de l'aborescence
        private readonly string cheminDossierDefault = @"C:\DOCUMAT";

        public Main()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //((Menu)Application.Current.MainWindow).loader.Visibility = Visibility.Collapsed
            refreshListServices();
            // définition du contextMenu lié à la Datagrid
            ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
            dgService.ContextMenu = cm;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EditMode = false;
            txbFormHeader.Text = "FORMULAIRE D'AJOUT DE SERVICE";
            tbCode.Text = "";
            tbDesc.Text = "";
            tbNom.Text = "";
            tbChemin.Text = "";
            gridViewPanel.Visibility = Visibility.Collapsed;
            formService.Visibility = Visibility.Visible;
            panelInteracBtn.Visibility = Visibility.Collapsed;
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            gridViewPanel.Visibility = Visibility.Visible;
            formService.Visibility = Visibility.Collapsed;
            panelInteracBtn.Visibility = Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode)
            {
                #region Code D'ajout de service
                try
                {
                    //Création du dossier du service dans le dossier racine
                    string chemin = Path.Combine(cheminDossierDefault, tbChemin.Text);
                    Directory.CreateDirectory(chemin);
                    ServiceView serviceView = new ServiceView();
                    using (var context = new DocumatContext())
                    {
                        context.Service.Add(new Models.Service
                        {
                            Nom = tbNom.Text,
                            Code = tbCode.Text,
                            CheminDossier = tbChemin.Text,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                            Description = tbDesc.Text
                        });

                        context.SaveChanges();
                    }

                    refreshListServices();
                    gridViewPanel.Visibility = Visibility.Visible;
                    formService.Visibility = Visibility.Collapsed;
                    panelInteracBtn.Visibility = Visibility.Visible;
                    MessageBox.Show("Elément ajouté", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
                #endregion
            }
            else
            {
                #region Code de Modification de service

                ServiceView serviceViews = (ServiceView)dgService.SelectedItem;

                if (tbCode.Text != serviceViews.Service.Code || tbDesc.Text != serviceViews.Service.Description
                    || tbNom.Text != serviceViews.Service.Nom || tbChemin.Text != serviceViews.Service.CheminDossier)
                {
                    try
                    {
                        using (var context = new DocumatContext())
                        {
                            Models.Service servFind = context.Service.FirstOrDefault(s => s.ServiceID == serviceViews.Service.ServiceID);
                            if (servFind != null && tbChemin.Text != serviceViews.Service.CheminDossier)
                            {
                                if (Directory.Exists(Path.Combine(cheminDossierDefault, servFind.CheminDossier)))
                                {
                                    FileSystem.RenameDirectory(
                                    Path.Combine(cheminDossierDefault, servFind.CheminDossier), tbChemin.Text);
                                }
                                else
                                {
                                    var chemin = Path.Combine(cheminDossierDefault, tbChemin.Text);
                                    Directory.CreateDirectory(chemin);
                                }
                            }

                            servFind.Code = tbCode.Text;
                            servFind.Nom = tbNom.Text;
                            servFind.Description = tbDesc.Text;
                            servFind.CheminDossier = tbChemin.Text;
                            servFind.DateModif = DateTime.Now;

                            context.SaveChanges();
                        }

                        refreshListServices();
                        gridViewPanel.Visibility = Visibility.Visible;
                        formService.Visibility = Visibility.Collapsed;
                        panelInteracBtn.Visibility = Visibility.Visible;
                        MessageBox.Show("Modification éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        ex.ExceptionCatcher();
                    }
                }
                #endregion
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshListServices();
        }

        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (dgService.SelectedItems.Count != 0)
            {
                try
                {
                    if (MessageBox.Show("Voulez vous vraiment supprimer ?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        == MessageBoxResult.Yes)
                    {
                        using (var context = new DocumatContext())
                        {
                            foreach (ServiceView item in dgService.SelectedItems)
                            {
                                context.Service.Remove(context.Service.FirstOrDefault(s => s.ServiceID == item.Service.ServiceID));
                            }
                            context.SaveChanges();
                        }
                        refreshListServices();
                        MessageBox.Show("Suppression éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veillez sélectionner au moins un élément", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgService.SelectedItems.Count == 1)
            {
                EditMode = true;
                txbFormHeader.Text = "FORMULAIRE DE MODIFICATION";
                ServiceView serviceViews = (ServiceView)dgService.SelectedItem;

                tbCode.Text = serviceViews.Service.Code;
                tbDesc.Text = serviceViews.Service.Description;
                tbNom.Text = serviceViews.Service.Nom;
                tbChemin.Text = serviceViews.Service.CheminDossier;
                gridViewPanel.Visibility = Visibility.Collapsed;
                formService.Visibility = Visibility.Visible;
                panelInteracBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Veillez sélectionner un élément à modifier", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRechercher_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            rechercheService();
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            rechercheService();
        }

        private void tbNom_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void tbNom_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void tbNom_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void dgService_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            ServiceView view = (ServiceView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }
    }
}
