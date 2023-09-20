using PagedList;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using System;

namespace WebApplication1.Controllers
{
    public class VideoController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private int formation_group = 1;  // 影音


        public ActionResult Index(int? page = 1)
        {
            page = page ?? 1;

            var data = db.information.Where(x => x.if_ig_id == formation_group && x.if_visible).OrderByDescending(x => x.if_date).ToList();

            var result = data.ToPagedList((int)page, 6);

            return View(result);
        }

        // GET: News/Create
        public ActionResult Create()
        {
            var data = new information();
            data.if_visible = true;
            data.if_ig_id = formation_group;

            return View(data);
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(information data)
        {
            if (ModelState.IsValid)
            {
                var insertData = new information();
                insertData.if_date = DateTime.Now;
                insertData.if_title = data.if_title ?? "";
                insertData.if_connect = data.if_connect;
                insertData.if_content = data.if_content;
                insertData.if_ig_id = formation_group;
                insertData.if_visible = true;

                db.information.Add(insertData);
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
            if (id == 0 || id == null) return HttpNotFound();
            var data = db.information.Find(id);
            if (data == null) return HttpNotFound();
            return View(data);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(information data)
        {
            if (ModelState.IsValid)
            {
                var update = db.information.Find(data.if_id);

                update.if_title = data.if_title;
                update.if_connect = data.if_connect;
                update.if_content = data.if_content;

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
            var data = db.information.Find(id);
            if (data == null) return HttpNotFound();
            db.information.Remove(data);
            db.SaveChanges();
            Session["msg"] = "刪除成功";
            return RedirectToAction("Index");
        }
    }
}
