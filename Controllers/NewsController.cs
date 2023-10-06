using PagedList;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using System;

namespace WebApplication1.Controllers
{
    //訊息公告
    public class NewsController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        public ActionResult Index(int? page = 1)
        {
            page = page ?? 1;

            var data = db.News.OrderByDescending(x => x.n_id).ToList();

            var result = data.ToPagedList((int)page, 8);

            return View(result);
        }

        // GET: News/Details/5
        public ActionResult Detail(int id)
        {
            var data = db.News.Find(id);

            if (data == null) return HttpNotFound();

            return View(data);
        }

        // GET: News/Create
        public ActionResult Create()
        {
            var data = new News();
            return View(data);
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(News data)
        {
            if (ModelState.IsValid)
            {
                var inserData = new News()
                {
                    n_title = data.n_title ?? "",
                    n_content = data.n_content ?? "",
                    n_date = DateTime.Today
                };


                db.News.Add(inserData);
                db.SaveChanges();
                Session["msg"] = "新增成功";
                return RedirectToAction("Index");
            }

            Session["msg"] = "資料有誤";
            return View(data);
        }

        // GET: News/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0) return HttpNotFound();

            var data = db.News.Find(id);

            if (data == null) return HttpNotFound();

            return View(data);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News data)
        {
            if (ModelState.IsValid)
            {
                var update = db.News.Find(data.n_id);

                update.n_title = data.n_title;
                update.n_content = data.n_content;

                db.SaveChanges();

                Session["msg"] = "修改成功";
                return RedirectToAction("Index");
            }

            Session["msg"] = "資料有誤";
            return View(data);
        }

        public ActionResult Delete(int? id)
        {
            if (id == 0 || id == null) return HttpNotFound();

            var data = db.News.Find(id);

            if (data == null) return HttpNotFound();

            db.News.Remove(data);
            db.SaveChanges();
            Session["msg"] = "刪除成功";

            return RedirectToAction("Index");
        }
    }
}
