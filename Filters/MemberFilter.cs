using System;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class MemberFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var perm = filterContext.HttpContext.Session["perm"] == null || String.IsNullOrEmpty(filterContext.HttpContext.Session["perm"].ToString()) ? -1
                            : (int)filterContext.HttpContext.Session["perm"];
            if (perm < 0)
            {
                filterContext.HttpContext.Session["msg"] = "請先登入";

                UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                filterContext.Result = new RedirectResult(urlHelper.Action("Index", "Login"));
            }
            else if (perm < 3)
            {
                filterContext.HttpContext.Session["msg"] = "無此權限";

                UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                filterContext.Result = new RedirectResult(urlHelper.Action("Index", "Login"));
            }

        }
    }
}