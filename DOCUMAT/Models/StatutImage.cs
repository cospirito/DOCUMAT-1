using System;

namespace DOCUMAT.Models
{
    public class StatutImage : TableModel
    {
        public int StatutImageID { get; set; }
        public int Code { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public int ImageID { get; set; }
        public virtual Image Image { get; set; }
    }
}
