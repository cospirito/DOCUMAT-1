using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class MappageSequence : TableModel
    {
        public int MappageSequenceID { get; set; }
        public int new_id { get; set; }
        public int old_id { get; set; }
    }
}
