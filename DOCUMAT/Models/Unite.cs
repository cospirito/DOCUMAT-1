using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOCUMAT.Models
{
    public class Unite : TableModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UniteID { get; set; }
        public string Libelle { get; set; }
        public int TrancheID { get; set; }
        public Tranche Tranche { get; set; }
        public DateTime? DateDebutControle { get; set; }
        public DateTime? DateFinControle { get; set; }
    }
}
