using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOCUMAT.Models
{
    public class Tranche : TableModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TrancheID { get; set; }
        public string Libelle { get; set; }
        public virtual List<Unite> Unites { get; set; }
        public DateTime? DateDebutControle { get; set; }
        public DateTime? DateFinControle { get; set; }
        public int NbRegistreLivre { get; set; }
        public int NbVuesLivre { get; set; }
    }
}
