using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class Sequence : TableModel
    {
        public int SequenceID { get; set; }
        public int NUmeroOdre { get; set; }
        public DateTime DateSequence { get; set; }
        public string References { get; set; }
        public int NombreDeReferences { get; set; }
        public string isSpeciale { get; set; }
        public int ImageID { get; set; }
        public Image Image { get; set; }
        public int PhaseActuelle { get; set; }
    }
}
