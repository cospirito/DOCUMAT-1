using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class SessionTravail : TableModel
    {
        public int SessionTravailID { get; set; }

        //[Required(ErrorMessage ="Le champ Date de Début est requis")]
        public DateTime? DateDebut { get; set; }
        //[Required(ErrorMessage = "Le champ Date de Fin est requis")]
        public DateTime? DateFin { get; set; }

        //[Required(ErrorMessage = "Un agent doit être relié à cette session")]
        public int AgentID { get; set; }
        public virtual Agent Agent { get; set; }
    }
}
