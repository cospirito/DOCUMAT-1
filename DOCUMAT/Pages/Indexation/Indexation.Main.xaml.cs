using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DOCUMAT.Pages.Indexation
{
    /// <summary>
    /// Logique d'interaction pour Indexation.xaml
    /// </summary>
    public partial class Indexation : Page
    {
        public Models.Agent Utilisateur;

        #region FONCTIONS
            public void RefreshRegistre()
            {
                try
                {
                    if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        dgRegistre.ItemsSource = registreView.GetViewsList().Where(r=>r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE);
                    }
                    else
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        List<RegistreView> registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();
                        List<Traitement> traitementsAgent = registreView.context.Traitement.Where(t => t.AgentID == Utilisateur.AgentID
                                                        && t.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper() && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).ToList();
                        var jointure = from rv in registreViews
                                       join t in traitementsAgent on rv.Registre.RegistreID equals t.TableID
                                       select rv;

                        dgRegistre.ItemsSource = jointure;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
        #endregion

        public Indexation()
        {
            InitializeComponent();
        }

        public Indexation(Models.Agent user ):this()
        {
            Utilisateur = user;
        }

        private void ImprimerQrCode_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                string qrCode = ((RegistreView)dgRegistre.SelectedItem).Registre.QrCode;
                Impression.QrCode PageImp = new Impression.QrCode(qrCode);
                PageImp.Show();
            }
        }

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            //RegistreView view = (RegistreView)e.Row.Item;
            //view.NumeroOrdre = e.Row.GetIndex() + 1;
            //e.Row.Item = view;

            ((RegistreView)e.Row.Item).NumeroOrdre = e.Row.GetIndex() + 1;
        }

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            // Chargement de l'image du Qrcode de la ligne
            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            var element = e.DetailsElement.FindName("QrCode");
            RegistreView registre = (RegistreView)e.Row.Item;
            var image = qrcode.Draw(registre.Registre.QrCode, 40);
            var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
            ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
            dgRegistre.ContextMenu = cm;
            RefreshRegistre();
            MenuItem menuItemRegistre = (MenuItem)cm.Items.GetItemAt(1);

            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR 
                || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                menuItemRegistre.IsEnabled = true;
            }
            else
            {
                menuItemRegistre.IsEnabled = false;
            }
        }

        private void IndexerImage_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                Image.ImageViewer imageViewer = new Image.ImageViewer((RegistreView)dgRegistre.SelectedItem,this);
                this.IsEnabled = false;
                imageViewer.Show();
            }
        }

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    RegistreView RegistreView = new RegistreView();
                    List<RegistreView> registreViews = new List<RegistreView>();
                    if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();
                    }
                    else
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.SCANNE).ToList();
                        List<Traitement> traitementsAgent = registreView.context.Traitement.Where(t => t.AgentID == Utilisateur.AgentID
                                                        && t.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper() && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION).ToList();
                        var jointure = from rv in registreViews
                                       join t in traitementsAgent on rv.Registre.RegistreID equals t.TableID
                                       select rv;

                        registreViews = jointure.ToList();
                    }

                    switch (cbChoixRecherche.SelectedIndex)
                    {

                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreViews.Where(r => r.Registre.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper()));
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

        private void BtnRechercher_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            return;
        }

        private void BtnActualise_Click(object sender, RoutedEventArgs e)
        {
            RefreshRegistre();
        }

        private void SuperviserIndexation_Click(object sender, RoutedEventArgs e)
        {
            if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR
               || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    Image.ImageIndexSuperviseur image = new Image.ImageIndexSuperviseur((RegistreView)dgRegistre.SelectedItem, this);
                    this.IsEnabled = false;
                    image.Show();
                }
            }
        }
    }
}
