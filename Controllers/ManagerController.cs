using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class ManagerController : Controller
    {
        // GET: Manager
        public ActionResult Index()
        {
            Session["title"] = "交通部觀光局協助審查旅宿業者申請穩定接待國際旅客服務量能補助-管理者"; 
            return View();
        }
    }
}