using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Teema {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Thread",
                url: "t/{Teema}/{LinkId}/{ParentPostId}",
                defaults: new { controller = "Thread", action = "Show", ParentPostId = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Teema",
                url: "t/{Teema}",
                defaults: new { controller = "Teema", action = "Show", Teema = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "User",
                url: "u/{Username}",
                defaults: new { controller = "Account", action = "ShowUser", Username = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Verification",
                url: "verify/{id}",
                defaults: new { controller = "Account", action = "VerifyAccount"}
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Teema", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
