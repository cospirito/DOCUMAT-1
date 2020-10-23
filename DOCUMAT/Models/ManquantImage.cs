using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class ManquantImage : TableModel
    {
        public int ManquantImageID { get; set; }
        public int IdRegistre { get; set; }
        public int? IdImage { get; set; }
        public int NumeroPage { get; set; }
        public int DebutSequence { get; set; }
        public int FinSequence { get; set; }
        public int statutManquant { get; set; }
        public DateTime? DateSequenceDebut { get; set; }
        public DateTime DateDeclareManquant { get; set; }
        public DateTime? DateCorrectionManquant { get; set; }
    }
}
