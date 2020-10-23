using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOCUMAT.Models
{
    public class Versement : TableModel
    {
        public int VersementID { get; set; }
        public int NumeroVers { get; set; }
        public string NomAgentVersant { get; set; }
        public string PrenomsAgentVersant { get; set; }        
        public DateTime DateVers { get; set; }
        public string cheminBordereau { get; set; }
        public int LivraisonID { get; set; }
        public Livraison Livraison { get; set; }
        public int NombreRegistreR3 { get; set; }
        public int NombreRegistreR4 { get; set; }
        //liste des registres liés
        public List<Registre> Registres { get; set; }
    }
}
