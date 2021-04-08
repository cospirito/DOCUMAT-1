using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour FormRenommage.xaml
    /// </summary>
    public partial class FormRenommage : Window
    {
        Models.Registre RegistreParent;
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];

        public void Actualiser()
        {
            // Chargement des noms d'images dans les combobox

            var files = Directory.GetFiles(System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier));
            List<FileInfo> fileInfos1 = new List<FileInfo>();
            List<FileInfo> fileInfosNumeric = new List<FileInfo>();

            // Récupération des fichiers dans un fileInfo
            foreach (var file in files)
            {
                fileInfos1.Add(new FileInfo(file));
            }
            fileInfos1 = fileInfos1.Where(f => f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();

            // récupération de toutes les Pages numériques
            // Ensuite les autres pages numérotées 
            //Système de trie des images de l'aborescence !!
            foreach (var file in files)
            {
                FileInfo file1 = new FileInfo(file);
                int numero = 0;
                if (Int32.TryParse(file1.Name.Substring(0, file1.Name.Length - 4), out numero))
                {
                    fileInfosNumeric.Add(file1);
                }
            }

            cbNumeroImageDebutPlus.ItemsSource = fileInfosNumeric;
            cbNumeroImageFinPlus.ItemsSource = fileInfosNumeric;

            cbNumeroImageDebutMoins.ItemsSource = fileInfosNumeric;
            cbNumeroImageFinMoins.ItemsSource = fileInfosNumeric;

            cbNumeroImageDebutPageEn.ItemsSource = fileInfos1;
            tbNumeroImageFinPageEn.Text = "";

            cbNumeroImageDebutEchange.ItemsSource = fileInfos1;
            cbNumeroImageFinEchange.ItemsSource = fileInfos1;
        }

        public FormRenommage()
        {
            InitializeComponent();
        }

        public FormRenommage(Models.Registre registre):this()
        {
            RegistreParent = registre;

            // Chargement des combobox via le btn actuliser 
            Actualiser();
        }

        private void btnActualiser_Click(object sender, RoutedEventArgs e)
        {
            Actualiser();
        }

        private void btnFermer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOuvrirDossier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo RegistreDossier = new DirectoryInfo(Path.Combine(DossierRacine, RegistreParent.CheminDossier));
                OpenFileDialog openFileDialog = new OpenFileDialog();
                var dlg = new CommonOpenFileDialog();
                dlg.Title = "Dossier de Scan du Registre : " + RegistreParent.QrCode;
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
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnFairePLusValider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbNumeroImageDebutPlus.SelectedItem != null && cbNumeroImageFinPlus.SelectedItem != null)
                {
                    // Récupération des Images Limites 
                    FileInfo fileInfoDebut = (FileInfo)cbNumeroImageDebutPlus.SelectedItem;
                    FileInfo fileInfoFin = (FileInfo)cbNumeroImageFinPlus.SelectedItem;

                    if (System.Windows.MessageBox.Show($"Cette Opération Ajoutera (+1) à toutes les Pages dont les Numéros sont Compris entre {fileInfoDebut.Name} et {fileInfoFin.Name}, Voulez vous Continuer ?", "Confirmer L'Opération", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                    {
                        // Comparaison du début < a fin
                        int NumeroDebut = Int32.Parse(fileInfoDebut.Name.Replace(fileInfoDebut.Extension, "").Trim());
                        int NumeroFin = Int32.Parse(fileInfoFin.Name.Replace(fileInfoFin.Extension, "").Trim());

                        if (NumeroDebut < NumeroFin)
                        {
                            // On vérife D'abord que le La dernière image +1 n'existe pas 
                            // On renomme la page 
                            string newNomFin;
                            if ((NumeroFin + 1) > 9)
                            {
                                if ((NumeroFin + 1) > 99)
                                {
                                    newNomFin = $"{(NumeroFin + 1)}";
                                }
                                else
                                {
                                    newNomFin = $"0{(NumeroFin + 1)}";
                                }
                            }
                            else
                            {
                                newNomFin = $"00{(NumeroFin + 1)}";
                            }

                            string newFileFin = Path.Combine(fileInfoFin.DirectoryName, newNomFin + fileInfoFin.Extension);
                            if (!File.Exists(newFileFin))
                            {
                                // Récupération des image comprise dans la fouchette
                                List<FileInfo> fileInfos = (List<FileInfo>)cbNumeroImageDebutPlus.ItemsSource;
                                List<FileInfo> fileInfosToRename = new List<FileInfo>();

                                foreach (var file in fileInfos)
                                {
                                    int NumeroPageRen = 0;
                                    if (Int32.TryParse(file.Name.Replace(file.Extension, "").Trim(), out NumeroPageRen) && NumeroPageRen >= NumeroDebut && NumeroPageRen <= NumeroFin)
                                    {
                                        // On renomme la page avec un tiret à la fin
                                        string newFile = Path.Combine(file.DirectoryName, file.Name.Remove(file.Name.Length - file.Extension.Length) + "_" + file.Extension);
                                        File.Move(file.FullName, newFile);
                                        fileInfosToRename.Add(new FileInfo(newFile));
                                    }
                                }

                                // On Renomme Maintenant le tout 
                                foreach (var file in fileInfosToRename)
                                {
                                    int NumeroPageRen = 0;
                                    if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length).Replace("_", ""), out NumeroPageRen))
                                    {
                                        // On renomme la page 
                                        string newNom;
                                        if ((NumeroPageRen + 1) > 9)
                                        {
                                            if ((NumeroPageRen + 1) > 99)
                                            {
                                                newNom = $"{(NumeroPageRen + 1)}";
                                            }
                                            else
                                            {
                                                newNom = $"0{(NumeroPageRen + 1)}";
                                            }
                                        }
                                        else
                                        {
                                            newNom = $"00{(NumeroPageRen + 1)}";
                                        }
                                        string newFile = Path.Combine(file.DirectoryName, newNom + file.Extension);
                                        File.Move(file.FullName, newFile);
                                    }
                                }
                                System.Windows.MessageBox.Show("Les Pages ont bien été renommées", "Pages Renommées", MessageBoxButton.OK, MessageBoxImage.Information);
                                Actualiser();
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Le Numéro Page de Fin (+1) existe dans le répertoire", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Le Numéro de L'image de Début doit être inférieur à celui de Fin", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnFaireMoinsValider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbNumeroImageDebutMoins.SelectedItem != null && cbNumeroImageFinMoins.SelectedItem != null)
                {
                    // Récupération des Images Limites 
                    FileInfo fileInfoDebut = (FileInfo)cbNumeroImageDebutMoins.SelectedItem;
                    FileInfo fileInfoFin = (FileInfo)cbNumeroImageFinMoins.SelectedItem;

                    if (System.Windows.MessageBox.Show($"Cette Opération Retirera (-1) à toutes les Pages dont les Numéros sont Compris entre {fileInfoDebut.Name} et {fileInfoFin.Name}, Voulez vous Continuer ?", "Confirmer L'Opération", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                    {
                        // Comparaison du début < a fin
                        int NumeroDebut = Int32.Parse(fileInfoDebut.Name.Replace(fileInfoDebut.Extension, "").Trim());
                        int NumeroFin = Int32.Parse(fileInfoFin.Name.Replace(fileInfoFin.Extension, "").Trim());

                        if (NumeroDebut < NumeroFin)
                        {
                            if(NumeroDebut > 0)
                            {
                                // On vérife D'abord que le La première image -1 n'existe pas 
                                // On renomme la page 
                                string newNomDebut;
                                if ((NumeroDebut - 1) > 9)
                                {
                                    if ((NumeroFin - 1) > 99)
                                    {
                                        newNomDebut = $"{(NumeroDebut - 1)}";
                                    }
                                    else
                                    {
                                        newNomDebut = $"0{(NumeroDebut - 1)}";
                                    }
                                }
                                else
                                {
                                    newNomDebut = $"00{(NumeroDebut - 1)}";
                                }

                                string newFileDebut = Path.Combine(fileInfoDebut.DirectoryName, newNomDebut + fileInfoFin.Extension);
                                if (!File.Exists(newFileDebut))
                                {
                                    // Récupération des image comprise dans la fouchette
                                    List<FileInfo> fileInfos = (List<FileInfo>)cbNumeroImageDebutPlus.ItemsSource;
                                    List<FileInfo> fileInfosToRename = new List<FileInfo>();

                                    foreach (var file in fileInfos)
                                    {
                                        int NumeroPageRen = 0;
                                        if (Int32.TryParse(file.Name.Replace(file.Extension, "").Trim(), out NumeroPageRen) && NumeroPageRen >= NumeroDebut && NumeroPageRen <= NumeroFin)
                                        {
                                            // On renomme la page avec un tiret à la fin
                                            string newFile = Path.Combine(file.DirectoryName, file.Name.Remove(file.Name.Length - file.Extension.Length) + "_" + file.Extension);
                                            File.Move(file.FullName, newFile);
                                            fileInfosToRename.Add(new FileInfo(newFile));
                                        }
                                    }

                                    // On Renomme Maintenant le tout 
                                    foreach (var file in fileInfosToRename)
                                    {
                                        int NumeroPageRen = 0;
                                        if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length).Replace("_", ""), out NumeroPageRen))
                                        {
                                            // On renomme la page 
                                            string newNom;
                                            if ((NumeroPageRen - 1) > 9)
                                            {
                                                if ((NumeroPageRen - 1) > 99)
                                                {
                                                    newNom = $"{(NumeroPageRen - 1)}";
                                                }
                                                else
                                                {
                                                    newNom = $"0{(NumeroPageRen - 1)}";
                                                }
                                            }
                                            else
                                            {
                                                newNom = $"00{(NumeroPageRen - 1)}";
                                            }
                                            string newFile = Path.Combine(file.DirectoryName, newNom + file.Extension);
                                            File.Move(file.FullName, newFile);
                                        }
                                    }
                                    System.Windows.MessageBox.Show("Les Pages ont bien été renommées", "Pages Renommées", MessageBoxButton.OK, MessageBoxImage.Information);
                                    Actualiser();
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Le Numéro Page de Début (-1) existe dans le répertoire", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Le Numéro de L'image de Début ne doit pas être égale à 0", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Le Numéro de L'image de Début doit être inférieur à celui de Fin", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnFairePageEnValider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbNumeroImageDebutPageEn.SelectedItem != null && tbNumeroImageFinPageEn.Text.Trim() != "")
                {
                    // Récupération des Images Limites 
                    FileInfo fileInfoDebut = (FileInfo)cbNumeroImageDebutPageEn.SelectedItem;

                    if (System.Windows.MessageBox.Show($"Voulez vous renommer la page {fileInfoDebut.Name.Replace(fileInfoDebut.Extension, "")} en {tbNumeroImageFinPageEn.Text.Trim()} ?", "Confirmer L'Opération", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                    {
                        // Vérification que le nouveau Nom n'existe pas
                        string newFileDebut = Path.Combine(fileInfoDebut.DirectoryName, tbNumeroImageFinPageEn.Text.Trim() + fileInfoDebut.Extension);
                        if (!File.Exists(newFileDebut))
                        {
                            File.Move(fileInfoDebut.FullName, newFileDebut);
                            System.Windows.MessageBox.Show("La Page a bien été renommée", "Page Renommée", MessageBoxButton.OK, MessageBoxImage.Information);
                            Actualiser();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Ce nom existe déja !!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnFaireEchangeValider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbNumeroImageDebutEchange.SelectedItem != null && cbNumeroImageFinEchange.SelectedItem != null)
                {
                    // Récupération des Images Limites 
                    FileInfo fileInfoDebut = (FileInfo)cbNumeroImageDebutEchange.SelectedItem;
                    FileInfo fileInfoFin = (FileInfo)cbNumeroImageFinEchange.SelectedItem;

                    if (System.Windows.MessageBox.Show($"Cette Opération échanger le Nom des Pages {fileInfoDebut.Name} et {fileInfoFin.Name}, Voulez vous Continuer ?", "Confirmer L'Opération", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                    {
                        // On s'assure que les pages sont différentes 
                        if (fileInfoDebut.Name.ToString().ToLower() != fileInfoFin.Name.ToString().ToLower())
                        {
                            // On Changer le Nom de La première page en un Nom intermédiaire
                            // On renomme la page avec un tiret à la fin
                            string newFileDebut = Path.Combine(fileInfoDebut.DirectoryName, fileInfoDebut.Name.Remove(fileInfoDebut.Name.Length - fileInfoDebut.Extension.Length) + "_" + fileInfoDebut.Extension);
                            File.Move(fileInfoDebut.FullName, newFileDebut);

                            //On Change le Nom de La 2ème page pour celui de la première
                            File.Move(fileInfoFin.FullName, fileInfoDebut.FullName);

                            //Puis On Change le Nom de la Page Intermédiare Vers celle de la dernière
                            File.Move(newFileDebut, fileInfoFin.FullName);
                            System.Windows.MessageBox.Show("Les Pages ont bien été échangées", "Page Renommée", MessageBoxButton.OK, MessageBoxImage.Information);
                            Actualiser();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Assurez que les pages début et fin sont différentes !!", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
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
