using System;
using System.ComponentModel.DataAnnotations;

namespace DOCUMAT.Models
{
    public class StatutRegistre : TableModel
    { 
        public int StatutRegistreID { get; set; }
        public int Code { get; set; }
        public DateTime DateDebut { get; set; }        
        public DateTime? DateFin { get; set; }
        public int RegistreID { get; set; }
        public virtual Registre Registre { get; set; }
    }
}
