using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using PagedList;
using WebApplication1.Models.Infomation;

namespace WebApplication1.Controllers
{

    public class QAController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        private int formation_group = 3;  // QA

        public ActionResult Index(int? page = 1)
        {
            page = page ?? 1;

            var data = db.information.Where(x => x.if_ig_id == formation_group && x.if_visible).OrderByDescending(x => x.if_date).ToList();

            var result = data.ToPagedList((int)page, 8);

            return View(result);
        }

        public ActionResult Create()
        {
            var result = new information();
            result.if_visible = true;
            result.if_ig_id = formation_group;

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(information data)
        {
            if (ModelState.IsValid)
            {
                var insert = new information()
                {
                    if_title = data.if_title,
                    if_content = data.if_content,
                    if_visible = data.if_visible,
                    if_ig_id = formation_group,
                    if_date = DateTime.Now,
                };
                db.information.Add(insert);
                db.SaveChanges();
                Session["msg"] = "新增成功";
                return RedirectToAction("Index");
            }

            Session["msg"] = "資料有誤";
            return View(data);
        }

        public ActionResult Edit(int? id)
        {
            if (id == 0 || id == null) return HttpNotFound();
            var data = db.information.Find(id);
            if (data == null) return HttpNotFound();

            return View(data);
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(information data)
        {
            if (ModelState.IsValid)
            {
                var update = db.information.Find(data.if_id);
                if (update != null)
                {
                    update.if_title = data.if_title;
                    update.if_content = data.if_content;
                    db.SaveChanges();
                    Session["msg"] = "修改成功";
                    return RedirectToAction("Index");
                }
            }
            Session["msg"] = "資料有誤";
            return View(data);
        }

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return HttpNotFound();
            }
            var data = db.information.Find(id);
            if (data == null)
            {
                return HttpNotFound();
            }
            db.information.Remove(data);
            db.SaveChanges();
            Session["msg"] = "刪除成功";
            return RedirectToAction("Index");
        }

        public ActionResult Detail(int id)
        {
            if (id == 0 || id == null) return HttpNotFound();
            var data = db.information.Find(id);
            if (data == null) return HttpNotFound();

            return View(data);
        }
    }
}