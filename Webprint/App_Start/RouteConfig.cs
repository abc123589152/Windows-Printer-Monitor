using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Webprint
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // 添加一個特定的路由，將 /Jobsprint 映射到 Home/Jobsprint
            routes.MapRoute(
                name: "Jobsprint",
                url: "Jobsprint",
                defaults: new { controller = "Home", action = "Jobsprint" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Jobsprint", id = UrlParameter.Optional }
            );
        }
    }
}
