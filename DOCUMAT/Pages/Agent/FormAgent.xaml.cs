using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DOCUMAT.Pages.Agent
{
    /// <summary>
    /// Logique d'interaction pour FormAgent.xaml
    /// </summary>
    public partial class FormAgent : Window
    {
        //Parent appelant
        Pages.Agent.GestionAgent MainAgent;
        Models.Agent Utilisateur;

        public Models.Agent AgentAModif { get; }

        //Mode edition / Ajout
        bool EditMode = false;
        private string nomDossierPhoto = "PHOTO";

        public string CheminPhotoCharger = "";
        public string CheminPhotoChargerOld = "";
        public string NomPhotoCharger = "";

        public FormAgent()
        {
            InitializeComponent();
        }

        public FormAgent(Pages.Agent.GestionAgent mainAgent, Models.Agent user) : this()
        {
            MainAgent = mainAgent;
            Utilisateur = user;
        }

        public FormAgent(Pages.Agent.GestionAgent mainAgent,Models.Agent agent, Models.Agent user) : this()
        {
            try
            {
                // Récupération des élément parents 
                MainAgent = mainAgent;
                AgentAModif = agent;
                Utilisateur = user;

                // Remplissage des données 
                EditMode = true;               
                tbMatricule.Text = agent.Matricule;
                tbNom.Text = agent.Nom;
                tbPrenoms.Text = agent.Prenom;
                tbLogin.Text = agent.Login;
                cbAffection.SelectedIndex = agent.Affectation;
                cbGenre.SelectedIndex = (int)Enum.Parse(typeof(Enumeration.Genre), agent.Genre);
                cbStatuMat.SelectedIndex = (int)Enum.Parse(typeof(Enumeration.StatutMatrimonial), agent.StatutMat);
                dtNaissance.SelectedDate = agent.DateNaiss;
                pwMdp.Password = agent.Mdp;
                pwMdpConfirme.Password = agent.Mdp;
                CheminPhotoCharger = agent.CheminPhoto;
                
                if (!string.IsNullOrEmpty(agent.CheminPhoto))
                {
                    ImgPhotoAgent.Source = new BitmapImage(new Uri(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], agent.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode)
            {
                AjouterAgent();
            }
            else
            {
                ModifierAgent();
            }
        }

        private void ModifierAgent()
        {
            try
            {
                // Limitation des droits du superviseur
                if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    if (cbAffection.SelectedIndex == (int)Enumeration.AffectationAgent.ADMINISTRATEUR)
                    {
                        throw new Exception("Vous ne disposer pas des privilèges nécessaire pour affecter ce agent à cette tâche !!!");
                    }
                    else if(cbAffection.SelectedIndex == (int)Enumeration.AffectationAgent.SUPERVISEUR && AgentAModif.AgentID != Utilisateur.AgentID)
                    {
                        throw new Exception("Vous ne disposer pas des privilèges nécessaire pour affecter ce agent à cette tâche !!!");
                    }
                }
    
                //Quand aucune date n'est sélectionner définit la date par défaut à null 
                DateTime? datNaiss;
                if (dtNaissance.SelectedDate.HasValue)
                    datNaiss = dtNaissance.SelectedDate.Value;
                else
                    datNaiss = null;

                if (tbMatricule.Text != AgentAModif.Matricule || tbNom.Text != AgentAModif.Nom
                     || tbPrenoms.Text != AgentAModif.Prenom || tbLogin.Text != AgentAModif.Login
                     || datNaiss != AgentAModif.DateNaiss
                     || pwMdp.Password != AgentAModif.Mdp || CheminPhotoCharger != AgentAModif.CheminPhoto 
                     || cbAffection.SelectedIndex != AgentAModif.Affectation ||cbGenre.SelectedIndex != (int)Enum.Parse(typeof(Enumeration.Genre), AgentAModif.Genre)
                     || cbStatuMat.SelectedIndex != (int)Enum.Parse(typeof(Enumeration.StatutMatrimonial), AgentAModif.StatutMat))
                {
                    if (pwMdp.Password != pwMdpConfirme.Password)
                        throw new Exception("Le Mot de Passe de Confirmation est différent du Mot de Passe");


                    //Vérifier que le nom n'existe pas déja dans le dossier 
                    if (CheminPhotoCharger != AgentAModif.CheminPhoto)
                    {
                        // Définition du nom de la photo enregistré !!!
                        NomPhotoCharger = "" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + "_" + tbLogin.Text.Trim() + "_" + new Random().Next(0001, 9999) + ".jpg";
                        CheminPhotoCharger = nomDossierPhoto + @"\" + NomPhotoCharger;

                        if (File.Exists(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], CheminPhotoCharger)))
                        {
                            throw new Exception("Recharger la photo SVP !!!");
                        }

                        File.Copy(CheminPhotoChargerOld, Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], CheminPhotoCharger));
                    }

                    // Modification de l'agent
                    // Création d'un agent à ajouter
                    Models.Agent agent = new Models.Agent()
                    {
                        AgentID = AgentAModif.AgentID,
                        Matricule = tbMatricule.Text.Trim(),
                        Nom = tbNom.Text.Trim(),
                        Prenom = tbPrenoms.Text.Trim(),
                        DateNaiss = datNaiss,
                        Genre = Enum.GetName(typeof(Enumeration.Genre), cbGenre.SelectedIndex),
                        StatutMat = Enum.GetName(typeof(Enumeration.StatutMatrimonial), cbStatuMat.SelectedIndex),
                        Affectation = cbAffection.SelectedIndex,
                        Login = tbLogin.Text.Trim(),
                        Mdp = pwMdp.Password.Trim(),
                        CheminPhoto = CheminPhotoCharger,
                        DateCreation = DateTime.Now,
                        DateModif = DateTime.Now
                    };
                    AgentView.Update(agent);

                    // Enregistrement du traitement de l'agent 
                    DocumatContext.AddTraitement(DocumatContext.TbAgent, AgentAModif.AgentID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION);

                    MessageBox.Show("Elément Modifié", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainAgent.refreshListAgent();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void AjouterAgent()
        {
            try
            {
                // Limitation des droits du superviseur
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    if (cbAffection.SelectedIndex == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || cbAffection.SelectedIndex == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                        throw new Exception("Vous ne disposer pas des privilèges nécessaires pour affecter ce agent à cette tâche !!!");
                }

                if (tbMatricule.Text != "" && tbNom.Text != "" && tbPrenoms.Text != "" && tbLogin.Text != ""
                && pwMdp.Password != "" && pwMdpConfirme.Password != null)
                {
                    if (pwMdp.Password != pwMdpConfirme.Password)
                        throw new Exception("Le Mot de Passe de Confirmation est différent du Mot de Passe");

                    if(!string.IsNullOrWhiteSpace(CheminPhotoChargerOld))
                    {
                        // Définition du nom de la photo enregistré !!!
                        NomPhotoCharger = "" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + "_" + tbLogin.Text.Trim() + "_" + new Random().Next(0001, 9999) + ".jpg";
                        CheminPhotoCharger = nomDossierPhoto + @"\" + NomPhotoCharger;

                        //Vérifier que le nom n'existe pas déja dans le dossier 
                        if (File.Exists(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], CheminPhotoCharger)))
                            throw new Exception("Recharger la photo SVP !!!");

                        //Déplacement du fichier dans le dossier de l'application   
                        File.Copy(CheminPhotoChargerOld,Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"],CheminPhotoCharger));
                    }

                    //Quand aucune date n'est sélectionner définit la date par défaut à null 
                    DateTime? datNaiss;
                    if (dtNaissance.SelectedDate.HasValue)
                        datNaiss = dtNaissance.SelectedDate.Value;
                    else
                        datNaiss = null;

                    // Création d'un agent à ajouter
                    Models.Agent agent = new Models.Agent()
                    {
                        Matricule = tbMatricule.Text.Trim(),
                        Nom = tbNom.Text.Trim(),
                        Prenom = tbPrenoms.Text.Trim(),
                        DateNaiss = datNaiss,
                        Genre = Enum.GetName(typeof(Enumeration.Genre), cbGenre.SelectedIndex),
                        StatutMat = Enum.GetName(typeof(Enumeration.StatutMatrimonial), cbStatuMat.SelectedIndex),
                        Affectation = cbAffection.SelectedIndex,
                        Login = tbLogin.Text.Trim(),
                        Mdp = pwMdp.Password.Trim(),
                        CheminPhoto = CheminPhotoCharger,
                        DateCreation = DateTime.Now,
                        DateModif = DateTime.Now
                    };
                    int agentId = AgentView.Add(agent);

                    // Enregistrement du traitement de l'agent 
                    DocumatContext.AddTraitement(DocumatContext.TbAgent, agentId,Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);

                    MessageBox.Show("Elément Ajouté", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainAgent.refreshListAgent();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnChoixImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Fichier image |*.jpg;*.png;*.tif";
                open.Multiselect = false;
                open.FilterIndex = 3;
                var dir = new DirectoryInfo(Directory.GetCurrentDirectory() +"/" + ConfigurationManager.AppSettings["CheminDossier_AvatarTemplate"]);
                if (Directory.Exists(dir.FullName))
                    open.InitialDirectory = dir.FullName;

                if (open.ShowDialog() == true)
                {
                    if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], nomDossierPhoto)))
                    {
                        Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], nomDossierPhoto));
                    }

                    CheminPhotoCharger = open.FileNames[0];
                    CheminPhotoChargerOld = open.FileNames[0];
                    ImgPhotoAgent.Source = new BitmapImage(new Uri(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], CheminPhotoChargerOld)), new System.Net.Cache.RequestCachePolicy());                        
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainAgent.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(EditMode)
            {
                // Droit de modification d'un superviseur vers un administrateur 
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    if (AgentAModif.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || (AgentAModif.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR && AgentAModif.AgentID != Utilisateur.AgentID))
                    {
                        MessageBox.Show("Vous n'avez pas les privilèges nécessaires pour faire cette modification !!! ", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        this.Close();
                    }

                    // Blocage de la modification du matricule et du login
                    tbMatricule.IsEnabled = false;
                    tbLogin.IsEnabled = false;
                }
                else if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR)
                {
                    // Blocage de la modification du matricule et du login
                    tbMatricule.IsEnabled = true;
                    tbLogin.IsEnabled = true;
                }
            }
        }
    }
}