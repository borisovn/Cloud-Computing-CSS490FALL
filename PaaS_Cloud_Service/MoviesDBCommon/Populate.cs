using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
