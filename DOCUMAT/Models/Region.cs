using System.ComponentModel.DataAnnotations;


namespace DOCUMAT.Models
{
    public class Region : TableModel
    {
        public int RegionID { get; set; }
        [Required]
        public string Nom { get; set; }
    }
}
