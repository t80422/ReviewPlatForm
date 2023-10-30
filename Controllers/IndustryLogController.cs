using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class IndustryLogController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        public ActionResult Index(int? page = 1)
        {
            var data = new IndustryLoginViewModel.Index();

            data.LoginList = db.login_out.Join(db.industry, l => l.l_id_id, i => i.id_id, (l, i) => new IndustryLoginViewModel.IndustryLogin
            {
                IndustryName = i.id_name,
                OwnerName = i.id_owner,
                LoginTime = (DateTime)l.l_in,
                LogoutTime = l.l_out,
            })
            .OrderByDescending(x => x.LoginTime)
            .ToPagedList((int)page, 5);

            return View(data);
        }
    }
}