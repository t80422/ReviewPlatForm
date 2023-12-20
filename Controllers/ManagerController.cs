using PagedList;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class ManagerController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        public ActionResult Index(int? page = 1)
        {
            var managers = db.user_accounts.Where(x => x.ua_perm != 3).Join(db.manager, u => u.ua_user_id, m => m.mg_id, (u, m) => new ManagerViewModel.ManagerAccounts
            {
                Manager = m,
                User_Accounts = u,
            })
            .OrderBy(x => x.Manager.mg_name)
            .ToPagedList((int)page, 10);

            return View(new ManagerViewModel.Index { ManagerAccounts = managers });
        }

        public ActionResult Create()
        {
            return View("CreateAndEdit", new ManagerViewModel.CreateOrEdit());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ManagerViewModel.CreateOrEdit data)
        {
            if (!ModelState.IsValid) { return View("CreateAndEdit", data); }

            if (UpdateDatabase(data, true))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View("CreateAndEdit", data);
            }
        }

        public ActionResult Edit(int managerID, int? permission)
        {
            return View("CreateAndEdit", GetCreateOrEdit(managerID, permission));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ManagerViewModel.CreateOrEdit data)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateAndEdit", GetCreateOrEdit(data.Manager.mg_id, data.User_Accounts.ua_perm));
            }

            if (UpdateDatabase(data, false))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View("CreateAndEdit", data);
            }
        }

        public ActionResult Delete(int managerID, int? permission)
        {
            var manager = db.manager.Find(managerID);

            if (manager == null)
                return RedirectToAction("Index");

            var account = db.user_accounts.Where(x => x.ua_user_id == managerID && x.ua_perm == permission).FirstOrDefault();

            db.manager.Remove(manager);
            db.user_accounts.Remove(account);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        private ManagerViewModel.CreateOrEdit GetCreateOrEdit(int managerID, int? permission)
        {
            var manager = db.manager.Find(managerID);
            var account = db.user_accounts.Where(x => x.ua_user_id == managerID && x.ua_perm == permission).FirstOrDefault();

            return new ManagerViewModel.CreateOrEdit
            {
                Manager = manager,
                User_Accounts = account,
            };
        }

        private bool UpdateDatabase(ManagerViewModel.CreateOrEdit data, bool isCreating)
        {
            manager manager;
            user_accounts userAccounts;

            if (isCreating)
            {
                manager = new manager();
                userAccounts = new user_accounts();
                db.manager.Add(manager);
                db.user_accounts.Add(userAccounts);
            }
            else
            {
                manager = db.manager.Find(data.Manager.mg_id);
                userAccounts = db.user_accounts.Find(data.User_Accounts.ua_id);
            }

            manager.mg_name = data.Manager.mg_name;
            manager.mg_phone = data.Manager.mg_phone;
            manager.mg_mail = data.Manager.mg_mail;
            manager.mg_job_title = data.Manager.mg_job_title;

            db.SaveChanges();

            userAccounts.ua_acct = data.User_Accounts.ua_acct;

            if (userAccounts.ua_psw == null && data.User_Accounts.ua_psw == null)
            {
                Session["msg"] = "請設定密碼";
                return false;
            }
            else if (data.User_Accounts.ua_psw != null)
            {
                userAccounts.ua_psw = ajax.ConvertToSHA256(data.User_Accounts.ua_psw);
            }

            userAccounts.ua_perm = data.User_Accounts.ua_perm;
            userAccounts.ua_user_id = manager.mg_id;

            db.SaveChanges();

            return true;
        }
    }
}