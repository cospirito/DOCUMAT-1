using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class TableModel
    {
        // Contient les propriétés commune de la BD
        public DateTime DateCreation { get; set; }        
        public DateTime DateModif { get; set; }
        [Column("TIMESTAMP")]
        [Timestamp]
        public virtual byte[] Timestamp { get; set; } // Permet à entityFramework de gérer les accès concurrents
    }
}
