using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace MoviesDBCommon 
{
    public class MovieContext : DbContext
    {
        public MovieContext() : base("name=MovieContext")
        {
        }

        public MovieContext(string connString)
            : base(connString)
        {
        }
        public System.Data.Entity.DbSet<Movie> Movies { get; set; }


    }
}
