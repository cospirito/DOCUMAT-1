using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOCUMAT.ViewModels
{
    public class VersementView : IView<VersementView, Models.Versement>
    {
        public int NumeroOrdre { get; set; }
        public DocumatContext context { get; set; }
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
            context = new DocumatContext();
        }

        public void Add()
        {
            if (!context.Versement.Any(v => v.NumeroVers == Versement.NumeroVers && v.LivraisonID == Versement.LivraisonID))
            {
                context.Versement.Add(Versement);
                context.SaveChanges();
            }
            else
            {
                throw new Exception("Ce numero de versement est déja utilisé !!!");
            }
        }

        public int AddLivraison(Livraison livraison)
        {
            if (!context.Livraison.Any(l => l.Numero == livraison.Numero && l.ServiceID == livraison.ServiceID))
            {
                context.Livraison.Add(livraison);
                context.SaveChanges();
            }
            else
            {
                Versement.Livraison = context.Livraison.FirstOrDefault(l => l.Numero == livraison.Numero && l.ServiceID == livraison.ServiceID);
                return 0;
            }
            return livraison.LivraisonID;
        }

        public int UpLivraison(Livraison livraison)
        {
            Livraison oldLivre = context.Livraison.FirstOrDefault(l => l.Numero == livraison.Numero && l.ServiceID == livraison.ServiceID);
            oldLivre.DateLivraison = livraison.DateLivraison;
            oldLivre.DateModif = DateTime.Now;
            context.SaveChanges();
            return livraison.LivraisonID;
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public bool GetView(int Id)
        {
            Versement = context.Versement.FirstOrDefault(v => v.VersementID == Id);

            if (Versement != null)
                return true;
            else
                throw new Exception("Le versement est introuvable !!!");
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
            List<VersementView> versementViews = new List<VersementView>();
            List<Versement> versements = context.Versement.ToList();

            foreach (var versement in versements)
            {
                VersementView versementView = new VersementView();
                versementView.Versement = versement;
                versementView.NombreR3 = context.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R3").Count();
                versementView.NombreR4 = context.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R4").Count();
                versementView.NombreR3EsV = versementView.NombreR3 + "/" + versement.NombreRegistreR3;
                versementView.NombreR4EsV = versementView.NombreR4 + "/" + versement.NombreRegistreR4;
                versementView.NombreRegistreEsV = (versementView.NombreR3 + versementView.NombreR4) + " / " + (versement.NombreRegistreR3 + versement.NombreRegistreR4);
                versementView.NombreRegistre = versementView.NombreR3 + versementView.NombreR4;
                versementViews.Add(versementView);
            }
            return versementViews;
        }

        public List<VersementView> GetVersViewsByService(int IdService)
        {
            List<Livraison> livraisons = context.Livraison.Where(l => l.ServiceID == IdService).ToList();
            List<VersementView> versementViews = new List<VersementView>();
            List<Versement> versements = new List<Versement>();
            foreach (var livraison in livraisons)
            {
                versements.AddRange(context.Versement.Where(v => v.LivraisonID == livraison.LivraisonID).ToList());
            }

            foreach (var versement in versements)
            {
                VersementView versementView = new VersementView();
                versementView.Versement = versement;
                versementView.NombreR3 = context.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R3").Count();
                versementView.NombreR4 = context.Registre.Where(r => r.VersementID == versement.VersementID && r.Type == "R4").Count();
                versementView.NombreR3EsV = versementView.NombreR3 + "/" + versement.NombreRegistreR3;
                versementView.NombreR4EsV = versementView.NombreR4 + "/" + versement.NombreRegistreR4;
                versementView.NombreRegistreEsV = (versementView.NombreR3 + versementView.NombreR4) + " / " + (versement.NombreRegistreR3 + versement.NombreRegistreR4);
                versementView.NombreRegistre = versementView.NombreR3 + versementView.NombreR4;
                versementViews.Add(versementView);
            }
            return versementViews;
        }

        public bool Update(Versement UpVersement)
        {
            if (!context.Versement.Any(v => v.NumeroVers == UpVersement.NumeroVers && v.LivraisonID == UpVersement.LivraisonID && v.VersementID != UpVersement.VersementID))
            {
                Versement.NomAgentVersant = UpVersement.NomAgentVersant;
                Versement.PrenomsAgentVersant = UpVersement.PrenomsAgentVersant;
                Versement.DateVers = UpVersement.DateVers;
                Versement.NumeroVers = UpVersement.NumeroVers;
                Versement.NombreRegistreR3 = UpVersement.NombreRegistreR3;
                Versement.NombreRegistreR4 = UpVersement.NombreRegistreR4;
                Versement.DateModif = DateTime.Now;
                context.SaveChanges();
            }
            else
            {
                throw new Exception("Ce numero de versement est déja utilisé !!!");
            }
            return true;
        }
    }
}
