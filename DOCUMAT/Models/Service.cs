using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class Service : TableModel
    {
        public int ServiceID { get; set; }
        //, StringLength(2), Index(IsUnique = true)
        [Required(ErrorMessage = "Le champ code du service est requis")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Le champ Nom du service est requis")]
        public string Nom { get; set; }
        public string Description { get; set; }
        public string NomChefDeService { get; set; }
        public string PrenomChefDeService { get; set; }
        [Required(ErrorMessage = "Le champ du Nom du Dossier du service est requis")]
        public string CheminDossier { get; set; }
        [Required(ErrorMessage = "Le Champ Nombre de registre R3 ne peut être vide")]
        public int NombreR3 { get; set; }
        [Required(ErrorMessage = "Le Champ Nombre de registre R4 ne peut être vide")]
        public int NombreR4 { get; set; }
        [Required(ErrorMessage ="Aucune region n'est spécifiée")]
        public int RegionID { get; set; }
        public virtual Region Region { get; set;}
    }
}