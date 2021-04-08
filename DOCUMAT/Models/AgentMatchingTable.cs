using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class AgentMatchingTable : TableModel
    {
        public int AgentMatchingTableID { get; set; }
        public int? Casa_id { get; set; }
        public int? Rabat_id { get; set; }
        [Required(ErrorMessage = "Vous devez entrer le matricule de l'agent")]
        public string Matricule { get; set; }
        [Required(ErrorMessage = "Le champ Nom est requis")]
        public string Nom { get; set; }
        [Required(ErrorMessage = "Le champ Prenom est requis")]
        public string Prenom { get; set; }
        [Required(ErrorMessage = "Le champ Genre est requis")]
        public string Genre { get; set; }
        //[Required(ErrorMessage = "Le champ Date de naissance est requis")]
        public DateTime? DateNaiss { get; set; }
        //[Required(ErrorMessage = "Le champ Satut Matrimonial est requis")]
        public string StatutMat { get; set; }
        //[Required(ErrorMessage = "Le champ Type d'agent est requis")]
        //public string Type { get; set; }
        [Required(ErrorMessage = "Le champ Affectation est requis")]
        public int Affectation { get; set; }
        //[Required(ErrorMessage = "Le champ Chemin de la photo est requis")]
        public string CheminPhoto { get; set; }
        [Required(ErrorMessage = "Le champ Login est requis")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Le champ Mot de passe est requis")]
        public string Mdp { get; set; }
    }
}
