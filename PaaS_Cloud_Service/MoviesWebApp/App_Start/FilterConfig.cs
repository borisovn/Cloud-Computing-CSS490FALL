using System.Web;
using System.Web.Mvc;

namespace MoviesWebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
        }
    }
}
