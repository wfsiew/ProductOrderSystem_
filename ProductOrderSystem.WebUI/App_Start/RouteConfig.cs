﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ProductOrderSystem.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ProductOrderSystem.WebUI.Controllers" }
            );

            routes.MapRoute(
                name: "Order",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Order", action = "Create", id = UrlParameter.Optional },
                namespaces: new[] { "ProductOrderSystem.WebUI.Controllers" }
            );
        }
    }
}