using System;

namespace DOCUMAT.Models
{
    public class Facture : TableModel
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FactureID { get; set; }
        public string Numero_Facture { get; set; }
        public string Type { get; set; }
        public DateTime? DateFacture { get; set; }
        public int RegistreID { get; set; }
        public Registre Registre { get; set; }
    }
}
