using System;

namespace DOCUMAT.Models
{
    public class Correction : TableModel
    {
        public int? CorrectionID { get; set; }
        public int? RegistreId { get; set; }
        public int? ImageID { get; set; }
        public int? SequenceID { get; set; }

        // Index Registre
        public int? Numero_idx { get; set; }
        public int? DateDepotDebut_idx { get; set; }
        public int? DateDepotFin_idx { get; set; }
        public int? NumeroDebut_idx { get; set; }
        public int? NumeroDepotFin_idx { get; set; }
        public int? NombrePage_idx { get; set; }

        // Indexes Images
        public int? NumeroPageImage_idx { get; set; }
        public int? NomPageImage_idx { get; set; }
        public int? RejetImage_idx { get; set; }
        public int? RejetImage_scan { get; set; }
        public string MotifRejetImage_idx { get; set; }

        //Indexes Séquence
        public int? OrdreSequence_idx { get; set; }
        public int? DateSequence_idx { get; set; }
        public int? RefSequence_idx { get; set; }
        public string RefRejetees_idx { get; set; }

        // Element de Correction
        public int? PhaseCorrection { get; set; }
        public int? StatutCorrection { get; set; }
        public int? ASupprimer { get; set; } //1 : Supprimer
        public DateTime? DateCorrection { get; set; }
    }
}
