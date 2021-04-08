using System;
using System.ComponentModel.DataAnnotations;

namespace DOCUMAT.Models
{
    public class Livraison : TableModel
    {
        public int LivraisonID { get; set; }
        [Required(ErrorMessage = "Le numero de Livraison est requis")]
        public int Numero { get; set; }
        [Required(ErrorMessage = "Date de livraison est requise")]
        public DateTime DateLivraison { get; set; }
        [Required]
        public int ServiceID { get; set; }
        public virtual Service Service { get; set; }
    }
}
