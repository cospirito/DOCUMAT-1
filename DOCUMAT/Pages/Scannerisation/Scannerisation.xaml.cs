using DOCUMAT.DataModels;
using DOCUMAT.Models;
using DOCUMAT.Pages.Registre;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DOCUMAT.Pages.Scannerisation
{
    /// <summary>
    /// Logique d'interaction pour Scannerisation.xaml
   /// </summary>
    public partial class Scannerisation : Page
    {
        Models.Agent Utilisateur;
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        string fichier_sid = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], "sid_scanners_users.tmp");
        string FileLogName = "";// Sera le Nom du Registre lui même
        private readonly BackgroundWorker RefreshData = new BackgroundWorker();
        public List<RegistreDataModel> ListRegistreActuel = new List<RegistreDataModel>();
        int parRecup = Int32.Parse(ConfigurationManager.AppSettings["perRecup"]);

        #region FONCTIONS
        private bool ValiderScan(RegistreDataModel registreView)
        {
            // Décompte des fichiers images et comparaison;
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.CheminDossier));
            List<FileInfo> filesInfos = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();
            int NbImage = filesInfos.Count;
            int MajNbImage = Int32.Parse(ConfigurationManager.AppSettings["Maj_Scan"]);

            // Vérification du nombre de Page
            if ((registreView.NombrePage + 2 + MajNbImage) >= NbImage && NbImage >= (registreView.NombrePage + 2 - MajNbImage))
            {
                // Vérification des noms numériques des pages
                foreach (var file in filesInfos)
                {
                    if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        int NumeroPage = 0;
                        if (!Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                        {
                            MessageBox.Show($"Registre N° : {registreView.Numero}, Veuillez renommer la page : {file.Name} en format numérique !", $"Registre N° : {registreView.Numero} Erreur Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                }

                //Système de trie des images de l'aborescence !!
                Dictionary<int, string> filesInt = new Dictionary<int, string>();
                foreach (var file in filesInfos)
                {
                    if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        FileInfo file1 = new FileInfo(file.FullName);
                        int numero = 0;
                        if (Int32.TryParse(file1.Name.Substring(0, file1.Name.Length - 4), out numero))
                        {
                            // Vérification des doublons d'images
                            try
                            {
                                filesInt.Add(numero, file.FullName);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Registre N° : " + registreView.Numero + ", Vérifier que la page : " + numero + " n'est pas en double !!!", $"Registre N° : {registreView.Numero} Erreur Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }
                    }
                }
                var fileSorted = filesInt.OrderBy(f => f.Key);

                // Vérification des pages manquantes
                string ListPageManquante = "";
                for (int i = 1; i <= fileSorted.Count(); i++)
                {
                    bool ismiss = true;
                    foreach (var file in fileSorted)
                    {
                        if (file.Key == i)
                        {
                            ismiss = false;
                        }
                    }

                    if (ismiss)
                    {
                        if (i == 1 && filesInfos.Any(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()))
                        {
                            // Pas de Manquant
                        }
                        else if (i == 2 && filesInfos.Any(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper()))
                        {
                            // Pas Manquant
                        }
                        else
                        {
                            ListPageManquante = ListPageManquante + i + " ,";
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ListPageManquante))
                {
                    ListPageManquante = ListPageManquante.Remove(ListPageManquante.Length - 1);
                    MessageBox.Show("Registre N° : " + registreView.Numero + " Page(s) : " + ListPageManquante + " Manquante(s), veuillez importer !!!", $"Registre N° : {registreView.Numero} Erreur Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Vérification du fichier log de scan 
                FileLogName = $"{registreView.QrCode.ToLower()}.mets";
                FileInfo logFile = directoryInfo.GetFiles().Where(f => f.Name.ToLower() == FileLogName).FirstOrDefault();
                if (logFile != null)
                {
                    using (var ct = new DocumatContext())
                    {
                        #region TRAITEMENT DU FICHIER XML DE SCAN ET DES IMAGES
                        Models.Agent userScan = null;
                        if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                        {
                            // Récupération du propriétaire du Registre
                            IdentityReference sid = null;
                            string loginAgent = "";

                            try
                            {
                                FileSecurity fileSecurity = File.GetAccessControl(logFile.FullName);
                                sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                                NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                            }
                            catch (IdentityNotMappedException ex) { }

                            // Vérification de l'agent dans le fichier
                            if (File.Exists(fichier_sid))
                            {
                                using (StreamReader fileStream = new StreamReader(fichier_sid))
                                {
                                    while (!fileStream.EndOfStream)
                                    {
                                        string line = fileStream.ReadLine().Trim();
                                        string lineUser = line.Split(' ')[0].ToLower();
                                        string lineSid = line.Split(' ')[line.Split(' ').Length - 1].ToLower();

                                        if (sid.ToString().ToLower().Trim() == lineSid.Trim().ToLower())
                                        {
                                            loginAgent = lineUser;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Fichier Configuration de sid manquant ", "Fichier agent de scan introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }

                            if (!string.IsNullOrWhiteSpace(loginAgent))
                            {
                                userScan = ct.Agent.FirstOrDefault(ag => ag.Login.Trim().ToLower() == loginAgent.Trim().ToLower());
                            }
                            else
                            {
                                MessageBox.Show("L'Agent de Scan de SID : " + sid.ToString() + ", est introuvable dans le fichier des sid", $"Registre N° : {registreView.Numero} Erreur Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }
                        else if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SCANNE)
                        {
                            userScan = Utilisateur;
                        }

                        // Vérification que la Page de Garde est Présente 
                        if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                            == null)
                        {
                            FileInfo pageInfo = null;
                            //Recherche de la Page 1
                            foreach (var file in filesInfos)
                            {
                                int NumeroPage = 0;
                                if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                                {
                                    if (NumeroPage == 1)
                                    {
                                        pageInfo = file;
                                    }
                                }
                            }

                            if (pageInfo != null)
                            {
                                // Changement du nom 1 en nom de la Page de Garde                                   
                                File.Move(pageInfo.FullName, Path.Combine(pageInfo.DirectoryName, ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper() + pageInfo.Extension));
                            }
                            else
                            {
                                MessageBox.Show("Le Dossier de Scan ne Contient pas d'image ayant le Nom : " + ConfigurationManager.AppSettings["Nom_Page_Garde"] + " ou de numéro 1, Vérifier le Dossier de Scan et importer la" +
                                    " PAGE DE GARDE si nécessaire !", "Registre N° : " + registreView.Numero, MessageBoxButton.OK, MessageBoxImage.Information);
                                return false;
                            }
                        }

                        // Vérification que la Page d'Ouverture est Présente 
                        if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                            == null)
                        {
                            FileInfo pageInfo = null;
                            //Recherche de la Page 1
                            foreach (var file in filesInfos)
                            {
                                int NumeroPage = 0;
                                if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                                {
                                    if (NumeroPage == 2)
                                    {
                                        pageInfo = file;
                                    }
                                }
                            }

                            if (pageInfo != null)
                            {
                                // Changement du nom 2 en nom de la Page de Garde                                   
                                File.Move(pageInfo.FullName, Path.Combine(pageInfo.DirectoryName, ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper() + pageInfo.Extension));
                            }
                            else
                            {
                                MessageBox.Show("Le Dossier de Scan ne Contient pas d'image ayant le Nom : " + ConfigurationManager.AppSettings["Nom_Page_Ouverture"] + " ou de Numéro 2, Vérifier le Dossier de Scan et importer la " +
                                " PAGE D'OUVERTURE si nécessaire !", "Registre N° : " + registreView.Numero, MessageBoxButton.OK, MessageBoxImage.Information);
                                return false;
                            }
                        }

                        #region Nommenclature des Pages 
                        // Changement des noms des fichiers 
                        // Numéroté les pages de manière consice à partir de 1 sachant que le Premier est 3
                        int newNomInt = 1;
                        for (int i = 3; i <= (filesInfos.Count); i++)
                        {
                            string newNom;
                            if (newNomInt > 9)
                            {
                                if (newNomInt > 99)
                                {
                                    newNom = $"{newNomInt}";
                                }
                                else
                                {
                                    newNom = $"0{newNomInt}";
                                }
                            }
                            else
                            {
                                newNom = $"00{newNomInt}";
                            }

                            foreach (FileInfo fileInfo in filesInfos)
                            {
                                FileInfo pageInfo = null;
                                int NumeroPage = 0;
                                if (Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length), out NumeroPage))
                                {
                                    if (NumeroPage == i)
                                    {
                                        pageInfo = fileInfo;
                                    }
                                }

                                if (pageInfo != null)
                                {
                                    if (!File.Exists(Path.Combine(pageInfo.DirectoryName, newNom.ToUpper() + pageInfo.Extension)))
                                    {
                                        File.Move(pageInfo.FullName, Path.Combine(pageInfo.DirectoryName, newNom.ToUpper() + pageInfo.Extension));
                                    }
                                }
                            }
                            newNomInt++;
                        }
                        #endregion

                        #region ajout des pages
                        //Création de Toutes les Images Scannées 
                        List<Models.Image> images = new List<Models.Image>();
                        DirectoryInfo newdirectoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.CheminDossier));
                        List<FileInfo> newfilesInfos = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                            || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();

                        newfilesInfos = newfilesInfos.Where(n => n.Name.Remove(n.Name.Length - n.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper()
                                                            && n.Name.Remove(n.Name.Length - n.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()).ToList();
                        List<Models.Image> ImagesRegistre = new List<Models.Image>();
                        foreach (var fileinfo in newfilesInfos)
                        {
                            int NumOrdreDeb = 0;
                            if (fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length).ToUpper() == "001")
                            {
                                NumOrdreDeb = registreView.NumeroDepotDebut;
                            }

                            // Vérification qu'une Page Avec le même Numero de Page n'existe Pas dans la BD
                            int NumeroPage = Int32.Parse(fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length).ToUpper());
                            if (!ct.Image.Any(i => i.RegistreID == registreView.RegistreID
                                && i.NumeroPage == NumeroPage))
                            {
                                Models.Image image = new Models.Image()
                                {
                                    DateCreation = DateTime.Now,
                                    DateDebutSequence = registreView.DateDepotDebut,
                                    DateModif = DateTime.Now,
                                    DateScan = fileinfo.CreationTime,
                                    Taille = fileinfo.Length,
                                    NomPage = fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length).ToUpper(),
                                    NumeroPage = NumeroPage,
                                    Type = fileinfo.Extension.Substring(1).ToUpper(),
                                    CheminImage = Path.Combine(registreView.CheminDossier, fileinfo.Name),
                                    RegistreID = registreView.RegistreID,
                                    DebutSequence = NumOrdreDeb,
                                    StatutActuel = (int)Enumeration.Image.SCANNEE,
                                };
                                ImagesRegistre.Add(image);
                            }
                        }
                        ct.Image.AddRange(ImagesRegistre);
                        ct.SaveChanges();

                        // définition de l'agent de scan                                            
                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.RegistreID, userScan.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_SCANNE, "REGISTRE SCANNE VALIDE PAR " + userScan.Login + ", ID : " + userScan.AgentID, logFile.CreationTime, logFile.CreationTime);

                        // Changement du status du registre
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == registreView.RegistreID);
                        // Récupération et modification de l'ancien statut du registre
                        Models.StatutRegistre AncienStatut = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == registreView.RegistreID
                                                                && s.Code == registre.StatutActuel);
                        AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

                        // Création du nouveau statut de registre
                        Models.StatutRegistre NewStatut = new StatutRegistre();
                        NewStatut.Code = (int)Enumeration.Registre.SCANNE;
                        NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
                        NewStatut.RegistreID = registreView.RegistreID;
                        ct.StatutRegistre.Add(NewStatut);

                        //Changement du statut du registre
                        registre.StatutActuel = (int)Enumeration.Registre.SCANNE;
                        registre.DateModif = DateTime.Now;
                        ct.SaveChanges();
                        return true;
                        #endregion
                        #endregion 
                    }
                }
                else
                {
                    MessageBox.Show("Le fichier Métadonné de Nom : " + FileLogName + " est Introuvable. Vérifier que le Fichier a bien été importé " +
                        "dans le Dossier du Registre et qu'il porte bien le Nom : " + FileLogName, ", Registre N° : " + registreView.Numero, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Le nombre d'image compter dans le Dossier de Scan est différent du Nombre d'image attendu, " +
                    "La Marge de +/- " + MajNbImage + " est atteinte", "Registre N° : " + registreView.Numero, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private bool ValiderScanDirect(RegistreDataModel registreView)
        {
            // Décompte des fichiers images et comparaison;
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.CheminDossier));
            List<FileInfo> filesInfos = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();
            int NbImage = filesInfos.Count;
            int MajNbImage = Int32.Parse(ConfigurationManager.AppSettings["Maj_Scan"]);

            // Vérification du nombre de Page
            if ((registreView.NombrePage + 2 + MajNbImage) >= NbImage && NbImage >= (registreView.NombrePage + 2 - MajNbImage))
            {
                // Vérification des noms numériques des pages
                foreach (var file in filesInfos)
                {
                    if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        int NumeroPage = 0;
                        if (!Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                        {
                            return false;
                        }
                    }
                }

                //Système de trie des images de l'aborescence !!
                Dictionary<int, string> filesInt = new Dictionary<int, string>();
                foreach (var file in filesInfos)
                {
                    if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        FileInfo file1 = new FileInfo(file.FullName);
                        int numero = 0;
                        if (Int32.TryParse(file1.Name.Substring(0, file1.Name.Length - 4), out numero))
                        {
                            // Vérification des doublons d'images
                            try
                            {
                                filesInt.Add(numero, file.FullName);
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        }
                    }
                }
                var fileSorted = filesInt.OrderBy(f => f.Key);

                // Vérification des pages manquantes
                string ListPageManquante = "";
                for (int i = 1; i <= fileSorted.Count(); i++)
                {
                    bool ismiss = true;
                    foreach (var file in fileSorted)
                    {
                        if (file.Key == i)
                        {
                            ismiss = false;
                        }
                    }

                    if (ismiss)
                    {
                        if (i == 1 && filesInfos.Any(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()))
                        {
                            // Pas de Manquant
                        }
                        else if (i == 2 && filesInfos.Any(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper()))
                        {
                            // Pas Manquant
                        }
                        else
                        {
                            ListPageManquante = ListPageManquante + i + " ,";
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ListPageManquante))
                {
                    ListPageManquante = ListPageManquante.Remove(ListPageManquante.Length - 1);
                    return false;
                }

                // Vérification du fichier log de scan 
                FileLogName = $"{registreView.QrCode.ToLower()}.mets";
                FileInfo logFile = directoryInfo.GetFiles().Where(f => f.Name.ToLower() == FileLogName).FirstOrDefault();
                if (logFile != null)
                {
                    using (var ct = new DocumatContext())
                    {
                        #region TRAITEMENT DU FICHIER XML DE SCAN ET DES IMAGES
                        Models.Agent userScan = null;
                        if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                        {
                            // Récupération du propriétaire du Registre
                            IdentityReference sid = null;
                            string loginAgent = "";

                            try
                            {
                                FileSecurity fileSecurity = File.GetAccessControl(logFile.FullName);
                                sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                                NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                            }
                            catch (IdentityNotMappedException ex) { }

                            // Vérification de l'agent dans le fichier
                            if (File.Exists(fichier_sid))
                            {
                                using (StreamReader fileStream = new StreamReader(fichier_sid))
                                {
                                    while (!fileStream.EndOfStream)
                                    {
                                        string line = fileStream.ReadLine().Trim();
                                        string lineUser = line.Split(' ')[0].ToLower();
                                        string lineSid = line.Split(' ')[line.Split(' ').Length - 1].ToLower();

                                        if (sid.ToString().ToLower().Trim() == lineSid.Trim().ToLower())
                                        {
                                            loginAgent = lineUser;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }

                            if (!string.IsNullOrWhiteSpace(loginAgent))
                            {
                                userScan = ct.Agent.FirstOrDefault(ag => ag.Login.Trim().ToLower() == loginAgent.Trim().ToLower());
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SCANNE)
                        {
                            userScan = Utilisateur;
                        }

                        // Vérification que la Page de Garde est Présente 
                        if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                            == null)
                        {
                            FileInfo pageInfo = null;
                            //Recherche de la Page 1
                            foreach (var file in filesInfos)
                            {
                                int NumeroPage = 0;
                                if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                                {
                                    if (NumeroPage == 1)
                                    {
                                        pageInfo = file;
                                    }
                                }
                            }

                            if (pageInfo != null)
                            {
                                // Changement du nom 1 en nom de la Page de Garde                                   
                                File.Move(pageInfo.FullName, Path.Combine(pageInfo.DirectoryName, ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper() + pageInfo.Extension));
                            }
                            else
                            {
                                return false;
                            }
                        }

                        // Vérification que la Page d'Ouverture est Présente 
                        if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                            == null)
                        {
                            FileInfo pageInfo = null;
                            //Recherche de la Page 1
                            foreach (var file in filesInfos)
                            {
                                int NumeroPage = 0;
                                if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                                {
                                    if (NumeroPage == 2)
                                    {
                                        pageInfo = file;
                                    }
                                }
                            }

                            if (pageInfo != null)
                            {
                                // Changement du nom 2 en nom de la Page de Garde                                   
                                File.Move(pageInfo.FullName, Path.Combine(pageInfo.DirectoryName, ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper() + pageInfo.Extension));
                            }
                            else
                            {
                                return false;
                            }
                        }

                        #region Nommenclature des Pages 
                        // Changement des noms des fichiers 
                        // Numéroté les pages de manière consice à partir de 1 sachant que le Premier est 3
                        int newNomInt = 1;
                        for (int i = 3; i <= (filesInfos.Count); i++)
                        {
                            string newNom;
                            if (newNomInt > 9)
                            {
                                if (newNomInt > 99)
                                {
                                    newNom = $"{newNomInt}";
                                }
                                else
                                {
                                    newNom = $"0{newNomInt}";
                                }
                            }
                            else
                            {
                                newNom = $"00{newNomInt}";
                            }

                            foreach (FileInfo fileInfo in filesInfos)
                            {
                                FileInfo pageInfo = null;
                                int NumeroPage = 0;
                                if (Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length), out NumeroPage))
                                {
                                    if (NumeroPage == i)
                                    {
                                        pageInfo = fileInfo;
                                    }
                                }

                                if (pageInfo != null)
                                {
                                    if (!File.Exists(Path.Combine(pageInfo.DirectoryName, newNom.ToUpper() + pageInfo.Extension)))
                                    {
                                        File.Move(pageInfo.FullName, Path.Combine(pageInfo.DirectoryName, newNom.ToUpper() + pageInfo.Extension));
                                    }
                                }
                            }
                            newNomInt++;
                        }
                        #endregion

                        #region ajout des pages
                        //Création de Toutes les Images Scannées 
                        List<Models.Image> images = new List<Models.Image>();
                        DirectoryInfo newdirectoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.CheminDossier));
                        List<FileInfo> newfilesInfos = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                            || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();

                        newfilesInfos = newfilesInfos.Where(n => n.Name.Remove(n.Name.Length - n.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper()
                                                            && n.Name.Remove(n.Name.Length - n.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()).ToList();
                        List<Models.Image> ImagesRegistre = new List<Models.Image>();
                        foreach (var fileinfo in newfilesInfos)
                        {
                            int NumOrdreDeb = 0;
                            if (fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length).ToUpper() == "001")
                            {
                                NumOrdreDeb = registreView.NumeroDepotDebut;
                            }

                            // Vérification qu'une Page Avec le même Numero de Page n'existe Pas dans la BD
                            int NumeroPage = Int32.Parse(fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length).ToUpper());
                            if (!ct.Image.Any(i => i.RegistreID == registreView.RegistreID
                                && i.NumeroPage == NumeroPage))
                            {
                                Models.Image image = new Models.Image()
                                {
                                    DateCreation = DateTime.Now,
                                    DateDebutSequence = registreView.DateDepotDebut,
                                    DateModif = DateTime.Now,
                                    DateScan = fileinfo.CreationTime,
                                    Taille = fileinfo.Length,
                                    NomPage = fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length).ToUpper(),
                                    NumeroPage = NumeroPage,
                                    Type = fileinfo.Extension.Substring(1).ToUpper(),
                                    CheminImage = Path.Combine(registreView.CheminDossier, fileinfo.Name),
                                    RegistreID = registreView.RegistreID,
                                    DebutSequence = NumOrdreDeb,
                                    StatutActuel = (int)Enumeration.Image.SCANNEE,
                                };
                                ImagesRegistre.Add(image);
                            }
                        }
                        ct.Image.AddRange(ImagesRegistre);
                        ct.SaveChanges();

                        // définition de l'agent de scan                                            
                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.RegistreID, userScan.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_SCANNE, "REGISTRE SCANNE VALIDE PAR " + userScan.Login + ", ID : " + userScan.AgentID, logFile.CreationTime, logFile.CreationTime);

                        // Changement du status du registre
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == registreView.RegistreID);
                        // Récupération et modification de l'ancien statut du registre
                        Models.StatutRegistre AncienStatut = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == registreView.RegistreID
                                                                && s.Code == registre.StatutActuel);
                        AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

                        // Création du nouveau statut de registre
                        Models.StatutRegistre NewStatut = new StatutRegistre();
                        NewStatut.Code = (int)Enumeration.Registre.SCANNE;
                        NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
                        NewStatut.RegistreID = registreView.RegistreID;
                        ct.StatutRegistre.Add(NewStatut);

                        //Changement du statut du registre
                        registre.StatutActuel = (int)Enumeration.Registre.SCANNE;
                        registre.DateModif = DateTime.Now;
                        ct.SaveChanges();
                        return true;
                        #endregion
                        #endregion 
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private List<string> ValiderScanLog(RegistreDataModel registreView)
        {
            // Décompte des fichiers images et comparaison;
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.CheminDossier));
            List<FileInfo> filesInfos = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();
            int NbImage = filesInfos.Count;
            int MajNbImage = Int32.Parse(ConfigurationManager.AppSettings["Maj_Scan"]);
            List<string> notifRegistre = new List<string>();
            String valideRegistre = "";

            if (NbImage == 0)
            {
                notifRegistre.Add($"Pas d'image");
            }
            else
            {
                // Vérification du nombre de Page
                if ((registreView.NombrePage + 2 + MajNbImage) >= NbImage && NbImage >= (registreView.NombrePage + 2 - MajNbImage))
                {
                }
                else
                {
                    notifRegistre.Add($"{registreView.NombrePage + 2}(déclarées) est largement different de {NbImage}(scannées)");
                }

                // Vérification des noms numériques des pages
                var PageArenommer = "renommer page(s) : ";
                foreach (var file in filesInfos)
                {
                    if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        int NumeroPage = 0;
                        if (!Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                        {
                            PageArenommer += file.Name.ToString() + ", ";
                        }
                    }
                }

                if (PageArenommer.Length >= 20)
                {
                    notifRegistre.Add(PageArenommer);
                }

                //Système de trie des images de l'aborescence !!
                Dictionary<int, string> filesInt = new Dictionary<int, string>();
                foreach (var file in filesInfos)
                {
                    if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        FileInfo file1 = new FileInfo(file.FullName);
                        int numero = 0;
                        if (Int32.TryParse(file1.Name.Substring(0, file1.Name.Length - 4), out numero))
                        {
                            // Vérification des doublons d'images
                            try
                            {
                                filesInt.Add(numero, file.FullName);
                            }
                            catch (Exception)
                            {
                                notifRegistre.Add($"la page : " + numero + " en double");
                            }
                        }
                    }
                }
                var fileSorted = filesInt.OrderBy(f => f.Key);

                // Vérification des pages manquantes
                string ListPageManquante = "";
                for (int i = 1; i <= fileSorted.Count(); i++)
                {
                    bool ismiss = true;
                    foreach (var file in fileSorted)
                    {
                        if (file.Key == i)
                        {
                            ismiss = false;
                        }
                    }

                    if (ismiss)
                    {
                        if (i == 1 && filesInfos.Any(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()))
                        {
                            // Pas de Manquant
                        }
                        else if (i == 2 && filesInfos.Any(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper()))
                        {
                            // Pas Manquant
                        }
                        else
                        {
                            ListPageManquante = ListPageManquante + i + " ,";
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ListPageManquante))
                {
                    ListPageManquante = ListPageManquante.Remove(ListPageManquante.Length - 1);
                    notifRegistre.Add($"Page(s) : " + ListPageManquante + " Manquante(s)");
                }

                // Vérification du fichier log de scan 
                FileLogName = $"{registreView.QrCode.ToLower()}.mets";
                FileInfo logFile = directoryInfo.GetFiles().Where(f => f.Name.ToLower() == FileLogName).FirstOrDefault();
                if (logFile != null)
                {
                    Models.Agent userScan = null;
                    // Récupération du propriétaire du Registre
                    IdentityReference sid = null;
                    string loginAgent = "";

                    try
                    {
                        FileSecurity fileSecurity = File.GetAccessControl(logFile.FullName);
                        sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                        NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                    }
                    catch (IdentityNotMappedException ex) { }

                    // Vérification de l'agent dans le fichier
                    if (File.Exists(fichier_sid))
                    {
                        using (StreamReader fileStream = new StreamReader(fichier_sid))
                        {
                            while (!fileStream.EndOfStream)
                            {
                                string line = fileStream.ReadLine().Trim();
                                string lineUser = line.Split(' ')[0].ToLower();
                                string lineSid = line.Split(' ')[line.Split(' ').Length - 1].ToLower();

                                if (sid.ToString().ToLower().Trim() == lineSid.Trim().ToLower())
                                {
                                    loginAgent = lineUser;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        notifRegistre.Add($"Fichier Configuration de sid manquant");
                    }

                    if (!string.IsNullOrWhiteSpace(loginAgent))
                    {
                        using (var ct = new DocumatContext())
                        {
                            userScan = ct.Agent.FirstOrDefault(ag => ag.Login.Trim().ToLower() == loginAgent.Trim().ToLower());
                        }
                        notifRegistre.Add($"Scanné par : " + loginAgent);
                    }
                    else
                    {
                        notifRegistre.Add($"SID : " + sid.ToString() + ", non définie");
                    }
                }
                else
                {
                    notifRegistre.Add($"Le fichier Métadonné de Nom : " + FileLogName + " est Introuvable");
                }

                // Vérification que la Page de Garde est Présente 
                if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                    == null)
                {
                    FileInfo pageInfo = null;
                    //Recherche de la Page 1
                    foreach (var file in filesInfos)
                    {
                        int NumeroPage = 0;
                        if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                        {
                            if (NumeroPage == 1)
                            {
                                pageInfo = file;
                            }
                        }
                    }

                    if (pageInfo == null)
                    {
                        notifRegistre.Add($"LA PAGE 0001/PAGE DE GARDE n'existe pas, ");
                    }
                }

                // Vérification que la Page d'Ouverture est Présente 
                if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    == null)
                {
                    FileInfo pageInfo = null;
                    //Recherche de la Page 1
                    foreach (var file in filesInfos)
                    {
                        int NumeroPage = 0;
                        if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length), out NumeroPage))
                        {
                            if (NumeroPage == 2)
                            {
                                pageInfo = file;
                            }
                        }
                    }

                    if (pageInfo == null)
                    {
                        notifRegistre.Add($"LA PAGE 0001/PAGE D'OUVERTURE n'existe pas");
                    }
                }
            }

            return notifRegistre;
        }

        #endregion

        public Scannerisation()
        {
            InitializeComponent();
            RefreshData.WorkerSupportsCancellation = true;
            RefreshData.WorkerReportsProgress = true;
            RefreshData.DoWork += RefreshData_DoWork;
            RefreshData.RunWorkerCompleted += RefreshData_RunWorkerCompleted;
            RefreshData.ProgressChanged += RefreshData_ProgressChanged;
            RefreshData.Disposed += RefreshData_Disposed;
        }

        private void RefreshData_Disposed(object sender, EventArgs e)
        {
            try
            {
                PanelLoader.Visibility = Visibility.Collapsed;
                PanelData.Visibility = Visibility.Visible;
                panelInteracBtn.IsEnabled = true;

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

        private void RefreshData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                ListRegistreActuel = e.Result as List<RegistreDataModel>;
                dgRegistre.ItemsSource = ListRegistreActuel;
                PanelLoader.Visibility = Visibility.Collapsed;
                PanelData.Visibility = Visibility.Visible;
                panelInteracBtn.IsEnabled = true;

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
            // Définition de la methode de récupération des données 
            try
            {
                Models.Agent user = (Models.Agent)e.Argument;
                if (user.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || user.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    using (var ct = new DocumatContext())
                    {
                        int nbRecup = 1;
                        // récupération du nombre total de registres au statut scanné
                        int nbLigne = ct.Database.SqlQuery<int>($"SELECT COUNT(RegistreID) FROM Registres WHERE StatutActuel = {(int)Enumeration.Registre.CREE}").FirstOrDefault();

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
                                registreDataModels.AddRange(RegistreView.GetRegistreData(limitId, parRecup, (int)Enumeration.Registre.CREE).ToList());
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
                else
                {
                    List<RegistreDataModel> registreDatas = new List<RegistreDataModel>();
                    //Remplissage de la list de registre
                    registreDatas = RegistreView.GetRegistreDataByScanAgent(Utilisateur.Login).ToList();
                    e.Result = registreDatas;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public Scannerisation(Models.Agent user) : this()
        {
            Utilisateur = user;
        }

        private void BtnActualise_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshData.RunWorkerAsync(Utilisateur);
                PanelLoader.Visibility = Visibility.Visible;
                panelInteracBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                //ex.ExceptionCatcher();
                RefreshData.CancelAsync();
                PanelLoader.Visibility = Visibility.Collapsed;
                panelInteracBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Recherche des occurences dans le tableau de recherche sans actualisation en temps réel !!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            try
            {
                // Chargement de l'image du Qrcode de la ligne
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                var element = e.DetailsElement.FindName("QrCode");
                RegistreDataModel registre = (RegistreDataModel)e.Row.Item;
                var image = qrcode.Draw(registre.QrCode, 40);
                var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
                ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
            }
            catch (Exception ex) { }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Définition du contextMenu de la datagrid registre
                ContextMenu cmRegistre = this.FindResource("cmRegistre") as ContextMenu;
                dgRegistre.ContextMenu = cmRegistre;
                // Récupération du btn de Modification du registre
                MenuItem editRegistreItem = (MenuItem)cmRegistre.Items.GetItemAt(2);

                //Récupération du fichier de hachage des sid des connextions 
                string file_sid = Path.Combine(DossierRacine, fichier_sid);
                if (!File.Exists(file_sid))
                {
                    //// Création du fichier 
                    //System.Diagnostics.Process.Start("cmd","wmic useraccount get name,sid");
                    MessageBox.Show("Fichiers des Login Agents Introuvable, veuillez le regenerer !!!", "Fichier " + fichier_sid + " Introuvable",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR
                   || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    btnValiderAllScanLog.IsEnabled = true;
                    editRegistreItem.IsEnabled = true;
                }
                else
                {
                    btnValiderAllScanLog.IsEnabled = false;
                    editRegistreItem.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ValiderScanRegistre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    if (MessageBox.Show("Voulez Vous vraiment Valider le Scan du Registre ?", "CONFIRMER VALIDATION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        // Vérification des fichiers Scannés                     
                        RegistreDataModel registreView = (RegistreDataModel)dgRegistre.SelectedItem;
                        if (ValiderScan(registreView))
                        {
                            MessageBox.Show("Scan Régistre validé !!!", "REGISTRE VALIDE", MessageBoxButton.OK, MessageBoxImage.Information);
                            BtnActualise_Click(sender, e);
                        }
                    }
                }
                else if (dgRegistre.SelectedItems.Count > 1)
                {
                    if (MessageBox.Show("Voulez Vous vraiment Valider le Scan du Registre ?", "CONFIRMER VALIDATION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        string ListRegistreValide = "", ListRegistreNonValide = "";

                        List<RegistreDataModel> registreViews = (List<RegistreDataModel>)dgRegistre.SelectedItems;
                        foreach (RegistreDataModel registreView in registreViews)
                        {
                            if (ValiderScanDirect(registreView))
                            {
                                ListRegistreValide += registreView.Numero + " ,";
                            }
                            else
                            {
                                ListRegistreNonValide += registreView.Numero + " ,";
                            }
                        }

                        if (ListRegistreValide != "")
                        {
                            ListRegistreValide = ListRegistreValide.Remove(ListRegistreValide.Length - 1);
                            MessageBox.Show("Registre(s) N° : " + ListRegistreValide.ToUpper() + " Validé(s)", "REGISTRES VALIDES", MessageBoxButton.OK, MessageBoxImage.Information);
                            BtnActualise_Click(sender, e);
                        }

                        if (ListRegistreNonValide != "")
                        {
                            ListRegistreNonValide = ListRegistreNonValide.Remove(ListRegistreNonValide.Length - 1);
                            MessageBox.Show("Registre(s) N° : " + ListRegistreNonValide.ToUpper() + "Non Validé(s)", "REGISTRES NON VALIDES", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void OuvrirDossierScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    RegistreDataModel registreView = (RegistreDataModel)dgRegistre.SelectedItem;
                    DirectoryInfo RegistreDossier = new DirectoryInfo(Path.Combine(DossierRacine, registreView.CheminDossier));
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    var dlg = new CommonOpenFileDialog();
                    dlg.Title = "Dossier de Scan du Registre : " + registreView.QrCode;
                    dlg.IsFolderPicker = false;
                    //dlg.InitialDirectory = currentDirectory;
                    dlg.AddToMostRecentlyUsedList = false;
                    dlg.AllowNonFileSystemItems = false;
                    //dlg.DefaultDirectory = currentDirectory;
                    //dlg.FileOk += Dlg_FileOk;
                    dlg.EnsureFileExists = true;
                    dlg.EnsurePathExists = true;
                    dlg.EnsureReadOnly = false;
                    dlg.EnsureValidNames = true;
                    dlg.Multiselect = true;
                    dlg.ShowPlacesList = true;
                    dlg.InitialDirectory = RegistreDossier.FullName;

                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok) { }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ImporterImages_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                // Importation de l'image 
                try
                {
                    RegistreDataModel registreView = (RegistreDataModel)dgRegistre.SelectedItem;
                    OpenFileDialog open = new OpenFileDialog();
                    FileLogName = $"{registreView.QrCode}.mets";
                    open.Filter = "Fichiers PDF | *.pdf;*.mets; | Fichiers Images (TIF, PNG, JPG, mets) |*.jpg;*.png;*.tif;*.mets;";
                    open.Multiselect = true;

                    if (open.ShowDialog() == true)
                    {
                        List<FileInfo> filesInfos = new List<FileInfo>();
                        foreach (var filePath in open.FileNames)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (File.Exists(System.IO.Path.Combine(DossierRacine, registreView.CheminDossier, fileInfo.Name)))
                            {
                                throw new Exception("Le fichier : '" + fileInfo.Name + "' existe déja dans le répertoire du registre !!!");
                            }
                            else
                            {
                                filesInfos.Add(fileInfo);
                            }
                        }

                        // Copy des fichiers dans le dossier du registre 
                        string ListeFileImport = "";
                        foreach (var fileInfo in filesInfos)
                        {
                            File.Copy(fileInfo.FullName, System.IO.Path.Combine(DossierRacine, registreView.CheminDossier, fileInfo.Name));
                            ListeFileImport = ListeFileImport + fileInfo.Name + ",";
                        }
                        ListeFileImport = ListeFileImport.Remove(ListeFileImport.Length - 1);

                        // enregistrement du traitement 
                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.RegistreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "IMPORTATION FICHIER : " + ListeFileImport);
                        MessageBox.Show(filesInfos.Count + " Fichier(s) : " + ListeFileImport + " ont/a été importée(s) avec success !!!", "IMPORTATION EFFECTUEE", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImprimerQrCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    using (var ct = new DocumatContext())
                    {
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == ((RegistreDataModel)dgRegistre.SelectedItem).RegistreID);
                        // Affichage de l'impression du code barre
                        Impression.BordereauRegistre bordereauRegistre = new Impression.BordereauRegistre(registre);
                        bordereauRegistre.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnValiderAllScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez Vous Valider le Scan de Tous les Registre ?", "VALIDER TOUT", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                {
                    if (dgRegistre.Items.Count > 0)
                    {
                        string ListRegistreValide = "", ListRegistreNonValide = "";
                        List<RegistreDataModel> registreViews = (List<RegistreDataModel>)dgRegistre.ItemsSource;
                        foreach (RegistreDataModel registreView in registreViews)
                        {
                            if (ValiderScanDirect(registreView))
                            {
                                ListRegistreValide += registreView.Numero + " ,";
                            }
                            else
                            {
                                ListRegistreNonValide += registreView.Numero + " ,";
                            }
                        }

                        if (ListRegistreValide != "")
                        {
                            ListRegistreValide = ListRegistreValide.Remove(ListRegistreValide.Length - 1);
                            MessageBox.Show("Registre(s) N° : " + ListRegistreValide.ToUpper() + " Validé(s)", "REGISTRES VALIDES", MessageBoxButton.OK, MessageBoxImage.Information);
                            BtnActualise_Click(sender, e);
                        }

                        if (ListRegistreNonValide != "")
                        {
                            ListRegistreNonValide = ListRegistreNonValide.Remove(ListRegistreNonValide.Length - 1);
                            MessageBox.Show("Registre(s) N° : " + ListRegistreNonValide.ToUpper() + "Non Validé(s)", "REGISTRES NON VALIDES", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                int StandardHeight = 800;
                int StandardDgHeight = 400;
                if (e.NewSize.Height < StandardHeight)
                {
                    dgRegistre.MaxHeight = StandardDgHeight - ((StandardHeight - e.NewSize.Height) * 2 / 3);
                }
                else
                {
                    dgRegistre.MaxHeight = StandardDgHeight + ((e.NewSize.Height - StandardHeight) * 2 / 3);
                }
            }
        }

        private void RenomAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous renommer automatiquement ?", "Renommer Automatiquement", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (dgRegistre.SelectedItems.Count == 1)
                    {
                        RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                        // Décompte des fichiers images et comparaison;
                        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.Registre.CheminDossier));
                        List<FileInfo> filesInfos = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                            || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").OrderBy(s => s.Name).ToList();
                        int debut = 0, fin = 0;

                        string add0str = "";
                        int diff = 0;

                        //Système de trie des images de l'aborescence !!
                        int FileNumber = 1;
                        foreach (var file in filesInfos)
                        {
                            if (file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                                && file.Name.Remove(file.Name.Length - file.Extension.Length).ToUpper() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                            {
                                // Nom du Fichier 
                                string newNom;
                                if (FileNumber > 9)
                                {
                                    if (FileNumber > 99)
                                    {
                                        newNom = $"0{FileNumber}";
                                    }
                                    else
                                    {
                                        newNom = $"00{FileNumber}";
                                    }
                                }
                                else
                                {
                                    newNom = $"000{FileNumber}";
                                }

                                newNom = add0str + newNom;

                                // Rename each files
                                File.Move(file.FullName, Path.Combine(file.DirectoryName, newNom + file.Extension));
                                FileNumber++;
                            }
                        }

                        MessageBox.Show($"{FileNumber - 1} image(s) ont/a été renommée(s) dans le Dossier", "Image(s) Renommée(s)", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnValiderAllScanLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez Vous Génerer le FIchier Log ?", "Génerer le Fichier des non Validées", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                {
                    if (dgRegistre.Items.Count > 0)
                    {
                        // Définition d'un emplacement Pour le Fichier 
                        var svd = new SaveFileDialog();

                        svd.Title = " SELECTIONNER LE NOM ET L'EMPLACEMENT DU LOG";
                        svd.Filter = "CSV | *.csv";
                        if (svd.ShowDialog() == true)
                        {
                            List<RegistreDataModel> registreViews = (List<RegistreDataModel>)dgRegistre.ItemsSource;
                            //Ouverture du fichier de Sortie 
                            using (var cvsWriter = new StreamWriter(svd.OpenFile(),System.Text.Encoding.UTF8))
                            {
                                // Définition du Header
                                cvsWriter.WriteLine("QRCODE;SERVICE;TYPE;VOLUME;CHEMIN;ERREUR/NOTIF");
                                foreach (RegistreDataModel registreView in registreViews)
                                {
                                    List<string> ListNotifs = new List<string>();
                                    try
                                    {
                                        ListNotifs = ValiderScanLog(registreView);
                                    }
                                    catch (Exception ex)
                                    {
                                        ListNotifs.Add("Exception : " + ex.Message);
                                    }

                                    if (ListNotifs.Count > 0)
                                    {
                                        // Ajout du registre au fichiers de sortie !!!
                                        foreach (var error in ListNotifs)
                                        {
                                            cvsWriter.WriteLine($"{registreView.QrCode};{registreView.NomComplet};{registreView.Type};{registreView.Numero};{Path.Combine(DossierRacine, registreView.CheminDossier)};{error};");
                                        }
                                    }
                                }

                                MessageBox.Show("Operation Terminée", "Operation Terminée", MessageBoxButton.OK, MessageBoxImage.Information);
                                System.Diagnostics.Process.Start(svd.FileName);
                            }
                        }
                    }
                    else
                    { MessageBox.Show("Le Tableau des Registres est Vide !!", "Aucun Registre", MessageBoxButton.OK, MessageBoxImage.Warning); }
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

        private void EditRegistre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    using (var ct = new DocumatContext())
                    {
                        // Récupération du registre
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == ((RegistreDataModel)dgRegistre.SelectedItem).RegistreID);
                        // Récupération du versement du registre
                        Models.Versement versement = ct.Versement.FirstOrDefault(v => v.VersementID == registre.VersementID);
                        FormRegistre formRegistre = new FormRegistre(registre,Utilisateur);
                        formRegistre.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnAnnuleChargement_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Voulez vous vraiment annuler l'opération ?", "Annuler", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                RefreshData.CancelAsync();
            }
        }
    }
}
