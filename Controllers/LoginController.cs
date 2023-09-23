using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        // GET: Login
        public ActionResult Index()
        {
            Session["title"] = "交通部觀光署協助審查旅宿業者申請穩定接待國際旅客服務量能補助-旅宿業者"; ;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string account, string password)
        {
            if (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(password))
            {
                var hasPassowrd = ajax.ConvertToSHA256(password);
                var user = db.user_accounts.Where(x => x.ua_acct == account && x.ua_psw == hasPassowrd).FirstOrDefault();

                if (user != null)
                {
                    Session["ua_id"] = user.ua_id;
                    Session["UserID"] = user.ua_user_id;
                    Session["perm"] = user.ua_perm;
                    Session["acct"] = user.ua_acct;
                    Session["msg"] = "登入成功";
                    Session["logintime"] = DateTime.Now.ToString("yyyy/MM/dd");
                    //test test2 test3  test4
                    //perm=3,密碼與統編相同表示第一次登入,須將畫面導向"修改密碼"
                    if ((int)Session["perm"] == 3)
                    {
                        int userID = (int)Session["UserID"];
                        var taxID = db.industry.Find(userID).id_tax_id;

                        if (password == taxID)
                        {
                            return RedirectToAction("ResetPwd");
                        }
                        else
                        {
                            Session["ResetPW"] = "N";
                            return RedirectToAction("Index", "Subsidy");
                        }
                    }
                    Session["ResetPW"] = "N";
                    return RedirectToAction("Index", "Industry");
                }
                else
                {
                    Session["msg"] = "帳號或密碼錯誤";
                }
            }
            else
            {
                Session["msg"] = "帳號或密碼不得為空";
            }

            return RedirectToAction("Index");
        }

        public ActionResult Logout()
        {
            Session["user_id"] = null;
            Session["perm"] = null;
            Session["acct"] = null;
            Session["msg"] = "登出成功";
            Session["logintime"] = null;
            return RedirectToAction("Index");
        }

        // 重設密碼
        public ActionResult ResetPwd()
        {
            Session["ResetPW"] = "Y";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPwd(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword == confirmPassword)
            {
                if (ModelState.IsValid)
                {
                    var update = db.user_accounts.Find(Session["ua_id"]);
                    if (update.ua_psw == ajax.ConvertToSHA256(oldPassword))
                    {
                        update.ua_psw = ajax.ConvertToSHA256(newPassword);

                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Message = "舊密碼輸入錯誤";
                    }
                }
            }
            else
            {
                ViewBag.Message = "新密碼與確認密碼不相符";
            }
            
            return View();
        }
    }
}