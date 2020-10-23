using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class Configs : TableModel
    {
        public int ConfigsID { get; set; }
        public string NomApp { get; set; }
        public string NomBD { get; set; }
        public string NomEntreprise { get; set; }
        public string VersionApp { get; set; }
        public string VersionBD { get; set; }
    }
}
