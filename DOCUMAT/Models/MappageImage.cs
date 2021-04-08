using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class MappageImage : TableModel
    {
        public int MappageImageID { get; set; }
        public int new_id { get; set; }
        public int old_id { get; set; }
    }
}
