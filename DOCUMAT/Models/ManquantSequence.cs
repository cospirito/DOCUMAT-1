using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class ManquantSequence : TableModel
    {
        public int ManquantSequenceID { get; set; }
        public int IdImage { get; set; }
        public int? IdSequence { get; set; }
        public int NumeroOrdre { get; set; }
        public int statutManquant { get; set; }
        public DateTime? DateDeclareManquant { get; set; }
        public DateTime? DateCorrectionManquant { get; set; }
    }
}
