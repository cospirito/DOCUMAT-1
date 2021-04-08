using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.DataModels
{
    public class RegistreDataModel
    {
        // Registre
        public int RegistreID { get; set; }
        public string QrCode { get; set; }
        public string Numero { get; set; }
        public DateTime DateDepotDebut { get; set; }
        public DateTime DateDepotFin { get; set; }
        public int NumeroDepotDebut { get; set; }
        public int NumeroDepotFin { get; set; }
        public int NombrePageDeclaree { get; set; }
        public int NombrePage { get; set; }
        public string Observation { get; set; }
        public int StatutActuel { get; set; }
        public string StatutName { get; set; } //
        public string CheminDossier { get; set; }
        public string Type { get; set; }
        public int? ID_Unite { get; set; }
        public int VersementID { get; set; }
        public DateTime DateCreationRegistre { get; set; }
        public DateTime DateModifRegistre { get; set; }

        //Image 
        public int NombrePageIndex { get; set; }
        public int NombrePageScan { get; set; }
        public int NombrePageInst { get; set; }
        public double PercentPageTraiteIndex { get; set; }

        //Versement
        public int NumeroVers { get; set; }
        public string NomAgentVersant { get; set; }
        public string PrenomsAgentVersant { get; set; }
        public DateTime DateVers { get; set; }
        public string cheminBordereau { get; set; }
        public int NombreRegistreR3 { get; set; }
        public int NombreRegistreR4 { get; set; }
        public int LivraisonID { get; set; }
        public DateTime DateCreationVersement { get; set; }
        public DateTime DateModifVersement { get; set; }


        //Livraison
        public int NumeroLivraison { get; set; } //
        public DateTime DateLivraison { get; set; }
        public int ServiceID { get; set; }
        public DateTime DateCreationLivraison { get; set; }
        public DateTime DateModifLivraison { get; set; }

        // Service
        public string Code { get; set; }
        public string Nom { get; set; }
        public string NomComplet { get; set; }
        public string Description { get; set; }
        public string NomChefDeService { get; set; }
        public string PrenomChefDeService { get; set; }
        public string CheminDossierService { get; set; } //
        public int NombreR3 { get; set; }
        public int NombreR4 { get; set; }
        public int RegionID { get; set; }
        public DateTime DateCreationService { get; set; }
        public DateTime DateModifService { get; set; }

        //region
        public string NomRegion { get; set; } //
        public DateTime DateCreationRegion { get; set; }
        public DateTime DateModifRegion { get; set; }
    }
}
