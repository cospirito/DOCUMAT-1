using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class Controle : TableModel
    {        
        public int? ControleID { get; set; }
        public int? RegistreId { get; set; }
        public int? ImageID { get; set; }
        public int? SequenceID { get; set; }

        // Indexes Images
        public int? NumeroPageImage_idx { get; set; }
        public int? NomPageImage_idx { get; set; }
        public int? RejetImage_idx { get; set; }
        public string MotifRejetImage_idx { get; set; }

        //Indexes Séquence
        public int? OrdreSequence_idx { get; set; }
        public int? DateSequence_idx { get; set; }
        public int? RefSequence_idx { get; set; }
        public string RefRejetees_idx { get; set; }

        // Element de controle
        public int? PhaseControle { get; set; } //phase 1 ou 3
        public int? StatutControle { get; set; }// 0 : Validé / 1 : Rejeté / 2 : 2 fois Réjétés
        public int? ASupprimer { get; set; } //1 : Supprimer
        public DateTime? DateControle { get; set; }
    }
}
