using System.Web.Mvc;
using System.Web.Routing;

namespace FaxOut
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Html",
                url: "html/{id}",
                defaults: new { controller = "Home", action = "Html", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Send",
                url: "send/{id}",
                defaults: new { controller = "Home", action = "Send", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
