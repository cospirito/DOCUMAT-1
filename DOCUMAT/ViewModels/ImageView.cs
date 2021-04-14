using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace DOCUMAT.ViewModels
{
    public class ImageView : IView<ImageView, Models.Image>
    {
        // Propriété du fichier scanné récupéré
        public int NumeroOrdre { get; set; }
        public string Type { get; set; }
        public string Taille { get; set; }
        public int NumeroScan { get; set; }
        public DateTime DateScan { get; set; }
        public string CheminScan { get; set; }
        public string NomImageScan { get; set; }
        public int Statut { get; set; }
        public object StatutName { get; private set; }


        //Propriété de la Base de Donnée
        public Image Image { get; set; }
        public Registre Registre { get; set; }


        // Propriété de visibilité sur les Infos de l'image créer en BD
        //public Visibility ImageBdExiste { get; set; }


        public ImageView()
        {
            Image = new Image();
        }

        public void Add()
        {
            using (var ct = new DocumatContext())
            {
                if (ct.Image.FirstOrDefault(i => i.NumeroPage == Image.NumeroPage && i.ImageID != Image.ImageID) == null)
                {
                    FileInfo fileInfo = new FileInfo(Image.CheminImage);
                    if (fileInfo.Exists)
                    {
                        string newFile = fileInfo.Name.Replace(fileInfo.Name.Remove(fileInfo.Name.Length - 4), Image.NumeroPage.ToString());
                        if (newFile.Trim().ToUpper() != fileInfo.Name.Trim().ToUpper())
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(fileInfo.FullName, newFile);
                            Image.CheminImage = fileInfo.FullName.Replace(fileInfo.Name, newFile);
                        }
                        ct.Image.Add(Image);
                        ct.SaveChanges();

                        //Création du statut d'image
                        Models.StatutImage statutImage = new Models.StatutImage();
                        statutImage.ImageID = Image.ImageID;
                        statutImage.Code = (int)Models.Enumeration.Image.CREEE;
                        statutImage.DateCreation = DateTime.Now;
                        statutImage.DateDebut = DateTime.Now;
                        statutImage.DateFin = DateTime.Now;
                        statutImage.DateModif = DateTime.Now;

                        ct.StatutImage.Add(statutImage);
                        ct.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Une image ayant le même numéro de page existe déja !!!");
                } 
            }
        }

        public Models.Image AddPreIndex()
        {
            using (var ct = new DocumatContext())
            {
                ct.Image.Add(Image);
                ct.SaveChanges();
                return Image; 
            }
        }

        public void AddSequencesPreIndex(List<Sequence> sequences)
        {
            using (var ct = new DocumatContext())
            {
                ct.Sequence.AddRange(sequences);
                ct.SaveChanges(); 
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool GetView(int Id)
        {
            throw new NotImplementedException();
        }

        public List<ImageView> GetSimpleViewsList(Registre registre)
        {
            using (var ct = new DocumatContext())
            {
                Registre = registre;
                List<ImageView> imageViews = new List<ImageView>();
                List<Image> images = ct.Image.Where(i => i.RegistreID == registre.RegistreID).ToList();

                foreach (var image in images)
                {
                    ImageView imageView = new ImageView();
                    imageView.Image = image;
                    imageView.Registre = registre;
                    imageViews.Add(imageView);
                }
                return imageViews; 
            }
        }

        public List<ImageView> GetViewsList(Registre registre)
        {
            // récupération du dossier du registre des images
            Registre = registre;
            string DossierRegistre = Registre.CheminDossier;
            var files = Directory.EnumerateFiles(DossierRegistre);
            List<ImageView> imageViews = new List<ImageView>();

            foreach (var file in files)
            {
                // Récupération des informations pour une image non ajouté à la BD / Statut 0
                FileInfo fileInfo = new FileInfo(file);
                ImageView imageView = new ImageView();

                // Obtention du numéro de Scan
                int numeroScan = 0;
                if (Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - 4), out numeroScan))
                    imageView.NumeroScan = numeroScan;
                else
                    imageView.NumeroScan = -1;

                // Récupération des éléments de Bd s'il existe
                using (var ct = new DocumatContext())
                {
                    imageView.Image = ct.Image.FirstOrDefault(i => i.RegistreID == registre.RegistreID && i.NumeroPage == numeroScan);
                }

                // Mise à jour du statut si l'image est ajouutée dans la BD
                if (imageView.Image != null)
                {
                    imageView.Statut = imageView.Image.StatutActuel;
                    imageView.StatutName = Enum.GetName(typeof(Models.Enumeration.Image), imageView.Image.StatutActuel);
                    //imageView.ImageBdExiste = Visibility.Visible;
                }
                else
                {
                    imageView.Statut = (int)Models.Enumeration.Image.SCANNEE;
                    imageView.StatutName = Enum.GetName(typeof(Models.Enumeration.Image), 0);
                    //imageView.ImageBdExiste = Visibility.Collapsed;
                }

                imageView.DateScan = fileInfo.LastWriteTime;
                imageView.CheminScan = fileInfo.FullName;
                imageView.NomImageScan = fileInfo.Name;
                imageView.Taille = Math.Ceiling(fileInfo.Length / 1024.0) + " Ko";
                imageView.Type = fileInfo.Extension.Substring(1).ToUpper();
                imageViews.Add(imageView);
            }
            return imageViews;
        }

        public List<ImageView> GetViewsList()
        {
            throw new NotImplementedException();
        }

        public bool Update(Image UpImage)
        {
            using (var ct = new DocumatContext())
            {
                Image = ct.Image.FirstOrDefault(i => i.ImageID == UpImage.ImageID);
                FileInfo fileInfo = new FileInfo(UpImage.CheminImage);
                int numeroPage = 0;
                if (Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - 4), out numeroPage))
                {
                    if (numeroPage != UpImage.NumeroPage)
                    {
                        if (ct.Image.FirstOrDefault(i => i.NumeroPage == UpImage.NumeroPage && i.ImageID != UpImage.ImageID) == null)
                        {
                            if (File.Exists(fileInfo.FullName))
                            {
                                if (numeroPage != UpImage.NumeroPage)
                                {

                                    string newFile = fileInfo.Name.Replace(numeroPage.ToString(), UpImage.NumeroPage.ToString());
                                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(fileInfo.FullName, newFile);
                                    Image.NumeroPage = UpImage.NumeroPage;
                                    Image.CheminImage = fileInfo.FullName.Replace(fileInfo.Name, newFile);
                                }
                                Image.DateModif = UpImage.DateModif;
                                ct.SaveChanges();
                                return true;
                            }
                            else
                            {
                                throw new Exception("L'image est manqante, elle a été supprimer ou rénomer, veuillez vérifier dans le dossier !!!");
                            }
                        }
                        else
                        {
                            throw new Exception("Une image ayant le même numéro de page à été enregistrer !!!");
                        }
                    }
                    else
                    {
                        throw new Exception("Aucun changemennt effectué !!!");
                    }
                }
                else
                {
                    throw new Exception("Le numéro de page entré n'est pas valide !!!");
                } 
            }
        }        
    }
}
