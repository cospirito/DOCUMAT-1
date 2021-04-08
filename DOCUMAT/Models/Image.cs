using System;
using System.Collections.Generic;

namespace DOCUMAT.Models
{
    public class Image : TableModel
    {
        public int ImageID { get; set; }
        public string CheminImage { get; set; }
        public string Type { get; set; }
        public float Taille { get; set; }
        public DateTime DateScan { get; set; }
        public int NumeroPage { get; set; }
        public string NomPage { get; set; }
        public string typeInstance { get; set; } // Quand l'image est en instance précise le type d'instance
        public string ObservationInstance { get; set; } // Une observation éventuelle quand l'image est instance
        public int StatutActuel { get; set; } // Le statut de l'image : Si elle est scanné ou crée ou indexée,est lié au code de la table statut image !!!
        public int DebutSequence { get; set; } // Le numéro d'ordre de la prémière séquence de l'image
        public int FinSequence { get; set; } //Le numéro d'ordre de la dernière séquence
        public DateTime DateDebutSequence { get; set; } // Date de début de la première séquence 
        //public int IdUser { get; set; }
        public int RegistreID { get; set; }
        public Registre Registre { get; set; }
        // Statut des images 
        public virtual List<StatutImage> StatutImages { get; set; }
    }
}
