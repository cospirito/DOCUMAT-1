using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DOCUMAT.Models
{
    public class Registre : TableModel
    {
        public int RegistreID { get; set; }
        public string QrCode { get; set; }
        public string Numero { get; set; }
        public DateTime DateDepotDebut { get; set; }
        public DateTime DateDepotFin { get; set; }
        public int NumeroDepotDebut { get; set; }
        public int NumeroDepotFin { get; set; }
        public int NombrePageDeclaree { get; set; }
        public int NombrePage { get; set; }
        public string Observation { get; set;}
        public int StatutActuel { get; set; }
        public string CheminDossier { get; set; }
        public string Type { get; set; }
        public int VersementID { get; set; }
        public Versement Versement { get; set; }
        //liste des images du registre
        public virtual List<Image> Images { get; set; }

        // Liste des statuts du registre
        public virtual List<StatutRegistre> StatutRegistres { get; set; }

        // Définition de la Tranche
        public int ID_Unite { get; set; }
    }
}
