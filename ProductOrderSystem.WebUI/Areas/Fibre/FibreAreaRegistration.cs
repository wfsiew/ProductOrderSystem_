using System.Web.Mvc;

namespace ProductOrderSystem.WebUI.Areas.Fibre
{
    public class FibreAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Fibre";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Fibre_default",
                "Fibre/{controller}/{action}/{id}/{ordertypeid}",
                new { action = "Index", id = UrlParameter.Optional, ordertypeid = UrlParameter.Optional },
                namespaces: new[] { "ProductOrderSystem.WebUI.Areas.Fibre.Controllers" }
            );
        }
    }
}
