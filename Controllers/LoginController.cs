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

        public ActionResult HomePage()
        {
            return Redirect("index.html");
            
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

                    if (user.ua_perm == 3)
                    {
                        var review = db.industry.Find(user.ua_user_id);
                        if (review != null)
                        {
                            Session["userReview"] = review.id_review;
                        }
                        else
                        {
                            Session["msg"] = "帳號不存在";
                            return RedirectToAction("Index");
                        }
                    }

                    Session["msg"] = "登入成功";
                    Session["logintime"] = DateTime.Now.ToString("yyyy/MM/dd");

                    //perm=3,密碼與統編相同表示第一次登入,須將畫面導向"修改密碼"
                    if ((int)Session["perm"] == 3)
                    {
                        int userID = (int)Session["UserID"];
                        var taxID = db.industry.Find(userID).id_tax_id;

                        if (password == taxID)
                        {
                            Session["First"] = "Y";
                            return RedirectToAction("ResetPwd", new { id = userID });
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
        public ActionResult ResetPwd(int id)
        {
            Session["ResetPW"] = "Y";
            return View(id);
        }
        //modify by v0.8
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ResetPwd(string oldPassword, string newPassword, string confirmPassword, int id)
        //{
        //    if (newPassword == confirmPassword)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var update = db.user_accounts.Where(x => x.ua_user_id == id).First();
        //            if ((int)Session["perm"] < 3 || update.ua_psw == ajax.ConvertToSHA256(oldPassword))
        //            {
        //                update.ua_psw = ajax.ConvertToSHA256(newPassword);

        //                db.SaveChanges();

        //                //modify by v0.8
        //                //return RedirectToAction("Index");
        //                //==============
        //                Session["ResetPW"] = "N";
        //                return RedirectToAction("Index","Subsidy");
        //                //modify by v0.8
        //            }
        //            else
        //            {
        //                ViewBag.Message = "舊密碼輸入錯誤";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.Message = "新密碼與確認密碼不相符";
        //    }

        //    return View(id);
        //}
        //==============        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPwd(string oldPassword, string newPassword, string confirmPassword, int id)
        {
            // 直接檢查 ModelState
            if (!ModelState.IsValid)
            {
                return View(id);
            }

            // 檢查新密碼與確認密碼
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "新密碼與確認密碼不相符";

                return View(id);
            }

            var update = db.user_accounts.SingleOrDefault(x => x.ua_user_id == id);

            if (update == null)
            {
                ViewBag.Message = "找不到指定用戶";

                return View(id);
            }

            // 使用者是管理員或者輸入的舊密碼是正確的
            if ((int)Session["perm"] < 3 || update.ua_psw == ajax.ConvertToSHA256(oldPassword))
            {
                update.ua_psw = ajax.ConvertToSHA256(newPassword);

                db.SaveChanges();

                Session["ResetPW"] = "N";

                return RedirectToAction("Index", "Subsidy");
            }

            ViewBag.Message = "舊密碼輸入錯誤";

            return View(id);
        }
        //modify by v0.8
    }
}