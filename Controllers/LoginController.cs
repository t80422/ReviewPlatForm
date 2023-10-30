using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Industry;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        private enum LogInOut
        {
            Login,
            Logout,
        }


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

                        LogLogInOutTime((int)user.ua_user_id, LogInOut.Login);

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

                    if ((int)Session["perm"] == 4)
                    {
                        Session["ResetPW"] = "N";
                        return RedirectToAction("Index", "CaseQuery");
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
            if ((int)Session["perm"] == 3) LogLogInOutTime((int)Session["UserID"], LogInOut.Logout);

            Session["UserID"] = null;
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

        public ActionResult ResetPassword(int industryID, int perm)
        {
            return View(GetLoginData(industryID, perm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(LoginViewModel data)
        {
            if (!ModelState.IsValid)
            {
                return View(GetLoginData((int)data.User_Accounts.ua_user_id, (int)data.User_Accounts.ua_perm));
            }

            if (data.NewPassword != data.CheckNewPassword)
            {
                Session["msg"] = "新密碼 與 確認密碼 不相同!";
                return View(GetLoginData((int)data.User_Accounts.ua_user_id, (int)data.User_Accounts.ua_perm));
            }

            var update = db.user_accounts.Find(data.User_Accounts.ua_id);

            update.ua_psw = ajax.ConvertToSHA256(data.NewPassword);

            db.SaveChanges();
            Session["msg"] = "儲存成功";

            return RedirectToAction("Edit_Manager", "Industry", new { industryID = data.User_Accounts.ua_user_id });
        }

        private LoginViewModel GetLoginData(int industryID, int perm)
        {
            var data = new LoginViewModel();
            data.User_Accounts = db.user_accounts.First(x => x.ua_user_id == industryID && x.ua_perm == perm);

            return data;
        }

        private void LogLogInOutTime(int industryID, LogInOut logInOut)
        {

            switch (logInOut)
            {
                case LogInOut.Login:
                    var insert = new login_out();

                    insert.l_id_id = industryID;
                    insert.l_in = DateTime.Now;

                    db.login_out.Add(insert);
                    db.SaveChanges();

                    Session["LogTimeID"] = insert.l_id;

                    break;

                case LogInOut.Logout:
                    var id = (int)Session["LogTimeID"];
                    var update = db.login_out.First(x => x.l_id == id);

                    update.l_out = DateTime.Now;

                    db.SaveChanges();

                    break;

                default:
                    break;
            }
        }
    }
}