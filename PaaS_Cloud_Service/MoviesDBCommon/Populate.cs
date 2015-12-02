using System.ComponentModel.DataAnnotations;

namespace MoviesDBCommon
{
   public class Populate
    {
        [Required]
        public string MovieTitle { get; set; }
        [Required]
        public string Protocol { get; set; }
    }
}
