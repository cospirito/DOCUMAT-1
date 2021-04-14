using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOCUMAT.ViewModels
{
    public class VersementView : IView<VersementView, Models.Versement>
    {
        public int NumeroOrdre { get; set; }
        public Versement Versement { get; set; }
        public int NombreRegistre { get; private set; }
        public string NombreRegistreEsV { get; private set; }
        public int NombreR3 { get; set; }
        public string NombreR3EsV { get; set; }
        public string NombreR4EsV { get; private set; }
        public int NombreR4 { get; set; }
        public Service ServiceVersant { get; private set; }


        public VersementView()
        {
            Versement = new Versement();
        }

        public void Add()
        {
            using (var ct = new DocumatContext())
            {
                if (!ct.Versement.Any(v => v.NumeroVers == Versement.NumeroVers && v.LivraisonID == Versement.LivraisonID))
                {
                    ct.Versement.Add(Versement);
                    ct.SaveChanges();
                }
                else
                {
                    throw new Exception("Ce numero de versement est déja utilisé !!!");
                } 
            }
        }

        public int AddLivraison(Livraison livraison)
        {
            using (var ct = new DocumatContext())
            {
                if (!ct.Livraison.Any(l => l.Numero == livraison.Numero && l.ServiceID == livraison.ServiceID))
                {
                    ct.Livraison.Add(livraison);
                    ct.SaveChanges();
                }
                else
                {
                    Versement.Livraison = ct.Livraison.FirstOrDefault(l => l.Numero == livraison.Numero && l.ServiceID == livraison.ServiceID);
                    return 0;
                }
                return livraison.LivraisonID; 
            }
        }

        public int UpLivraison(Livraison livraison)
        {
            using (var ct = new DocumatContext())
            {
                Livraison oldLivre = ct.Livraison.FirstOrDefault(l => l.Numero == livraison.Numero && l.ServiceID == livraison.ServiceID);
                oldLivre.DateLivraison = livraison.DateLivraison;
                oldLivre.DateModif = DateTime.Now;
                ct.SaveChanges();
                return livraison.LivraisonID; 
            }
        }

        public void Dispose()
        {
        }

        public bool GetView(int Id)
        {
            using (var ct = new DocumatContext())
            {
                Versement = ct.Versement.FirstOrDefault(v => v.VersementID == Id);

                if (Versement != null)
                {
                    return true;
                }
                else
                {
                    throw new Exception("Le versement est introuvable !!!");
                } 
            }
        }

        public VersementView GetViewVers(int Id)
        {
            List<VersementView> versementViews = GetViewsList();
            VersementView versementView = versementViews.FirstOrDefault(v => v.Versement.VersementID == Id);
            if (versementView != null)
            {
                return versementView;
            }
            else
            {
                throw new Exception("Aucun élément trouvé dans la table service n'a l'identifiant mentionné !!!");
            }
        }

        public List<VersementView> GetViewsList()
        {
            using (var ct = new DocumatContext())
            {
                List<VersementView> versementViews = new List<VersementView>();
                List<Versement> versements = ct.Versement.ToList();

                foreach (var versement in versements)
                {
                    VersementView versementView = new VersementView();
                    versementView.Versement = versement;
                    versementView.NombreR3 = ct.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R3").Count();
                    versementView.NombreR4 = ct.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R4").Count();
                    versementView.NombreR3EsV = versementView.NombreR3 + "/" + versement.NombreRegistreR3;
                    versementView.NombreR4EsV = versementView.NombreR4 + "/" + versement.NombreRegistreR4;
                    versementView.NombreRegistreEsV = (versementView.NombreR3 + versementView.NombreR4) + " / " + (versement.NombreRegistreR3 + versement.NombreRegistreR4);
                    versementView.NombreRegistre = versementView.NombreR3 + versementView.NombreR4;
                    versementViews.Add(versementView);
                }
                return versementViews; 
            }
        }

        public List<VersementView> GetVersViewsByService(int IdService)
        {
            using (var ct = new DocumatContext())
            {
                List<Livraison> livraisons = ct.Livraison.Where(l => l.ServiceID == IdService).ToList();
                List<VersementView> versementViews = new List<VersementView>();
                List<Versement> versements = new List<Versement>();
                foreach (var livraison in livraisons)
                {
                    versements.AddRange(ct.Versement.Where(v => v.LivraisonID == livraison.LivraisonID).ToList());
                }

                foreach (var versement in versements)
                {
                    VersementView versementView = new VersementView();
                    versementView.Versement = versement;
                    versementView.NombreR3 = ct.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R3").Count();
                    versementView.NombreR4 = ct.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R4").Count();
                    versementView.NombreR3EsV = versementView.NombreR3 + "/" + versement.NombreRegistreR3;
                    versementView.NombreR4EsV = versementView.NombreR4 + "/" + versement.NombreRegistreR4;
                    versementView.NombreRegistreEsV = (versementView.NombreR3 + versementView.NombreR4) + " / " + (versement.NombreRegistreR3 + versement.NombreRegistreR4);
                    versementView.NombreRegistre = versementView.NombreR3 + versementView.NombreR4;
                    versementViews.Add(versementView);
                }
                return versementViews; 
            }
        }

        public bool Update(Versement UpVersement)
        {
            using (var ct = new DocumatContext())
            {
                if (!ct.Versement.Any(v => v.NumeroVers == UpVersement.NumeroVers && v.LivraisonID == UpVersement.LivraisonID && v.VersementID != UpVersement.VersementID))
                {
                    Models.Versement versement = ct.Versement.FirstOrDefault(v => v.VersementID == Versement.VersementID);
                    if(versement != null)
                    {
                        versement.NomAgentVersant = UpVersement.NomAgentVersant;
                        versement.PrenomsAgentVersant = UpVersement.PrenomsAgentVersant;
                        versement.DateVers = UpVersement.DateVers;
                        versement.NumeroVers = UpVersement.NumeroVers;
                        versement.NombreRegistreR3 = UpVersement.NombreRegistreR3;
                        versement.NombreRegistreR4 = UpVersement.NombreRegistreR4;
                        versement.DateModif = DateTime.Now;
                        ct.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Ce numero de versement est déja utilisé !!!");
                }
                return true; 
            }
        }
    }
}
