// Filters/LoadCinemaBranchesAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Web.Service;

namespace Web.Filters
{
    public class LoadCinemaBranchesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var service = context.HttpContext.RequestServices.GetService<ICinemaBranchService>();

            if (service == null)
            {
                base.OnActionExecuting(context);
                return;
            }

            if (context.Controller is Controller controller)
            {
                var branches = service.GetBranches();
                controller.ViewBag.CinemaBranches = branches;
            }

            base.OnActionExecuting(context);
        }
    }
}
