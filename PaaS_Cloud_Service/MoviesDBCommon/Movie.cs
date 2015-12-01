using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MoviesDBCommon
{
   public  class Movie
    {
     
        public int MovieId { get; set; }
        [Required]
        public string Title { get; set; }

        [Required]
        public string Year { get; set; }
        [Required]
        public string imdbID { get; set; }
        [Required]
        public string Type { get; set; }
        public string Poster { get; set; }

        public string Thumbnail { get; set; }



    }
}
