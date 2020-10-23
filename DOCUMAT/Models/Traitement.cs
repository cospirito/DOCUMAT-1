using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class Traitement : TableModel
    {
        public int TraitementID { get; set; }
        public int TableID { get; set; } //Sera utilsé principalement pour la Table : REGISTRE
        public int AgentID { get; set; }
        public int TypeTraitement { get; set; } //Enumaration de type de traitement   
        public string TableSelect { get; set; } //Enumaration de type de traitement   
        public string Observation { get; set; } //  Note d'observation falcultative sur le traitement
    }
}
