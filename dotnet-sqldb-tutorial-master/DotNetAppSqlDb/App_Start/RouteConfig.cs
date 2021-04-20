using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DotNetAppSqlDb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "CreateSingleAccount",
                url: "Create",
                defaults: new { controller = "Accounts", action = "Create", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "HomePage",                                           // Route name
                url: "",                                                    // URL with parameters
                new { controller = "Accounts", action = "Index", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                name: "GetAccounts",                                               // Route name
                url: "GetAccounts",                                                // URL with parameters
                new { controller = "Accounts", action = "GetAccounts", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
               name: "UpsertAccounts",                                               // Route name
               url: "Upsert",                                                // URL with parameters
               new { controller = "Accounts", action = "Upsert", id = "" }  // Parameter defaults
           );
        }
    }
}
