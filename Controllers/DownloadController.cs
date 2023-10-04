using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Download;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class DownloadController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();
        // GET: Download

        private int formation_group = 4;  // 下載

        public ActionResult Index(int? page = 1)
        {
            page = page ?? 1;

            var list = db.information.Where(x => x.if_ig_id == formation_group && x.if_visible).OrderByDescending(x => x.if_date).ToList();

            var result = list.ToPagedList((int)page, 8);

            return View(result);
        }

        public ActionResult Create()
        {
            var result = new DownloadEdit()
            {
                type = formation_group,
                visible = true,
            };

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DownloadEdit data)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/assets/upload/Download");

                var filename1 = ajax.UploadFile(data.file1, path);
                var filename2 = ajax.UploadFile(data.file2, path);
                var filename3 = ajax.UploadFile(data.file3, path);
                var filename4 = ajax.UploadFile(data.file4, path);
                var filename5 = ajax.UploadFile(data.file5, path);

                var insertData = new information()
                {
                    if_ig_id = formation_group,
                    if_date = DateTime.Now,
                    if_title = data.title ?? "",
                    if_content = data.content ?? "",
                    if_file_one = filename1 ?? "",
                    if_name_file_one = data.file1 != null ? data.file1.FileName : "",
                    if_file_two = filename2 ?? "",
                    if_name_file_two = data.file2 != null ? data.file2.FileName : "",
                    if_file_three = filename3 ?? "",
                    if_name_file_three = data.file3 != null ? data.file3.FileName : "",
                    if_file_four = filename4 ?? "",
                    if_name_file_four = data.file4 != null ? data.file4.FileName : "",
                    if_file_five = filename5 ?? "",
                    if_name_file_five = data.file5 != null ? data.file5.FileName : "",
                    if_visible = true,
                };

                db.information.Add(insertData);
                db.SaveChanges();

                Session["msg"] = "新增成功";
                return RedirectToAction("Index");
            }

            Session["msg"] = "資料有誤";
            return View(data);
        }

        public ActionResult Edit(int id)
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

            var result = new DownloadEdit()
            {
                id = data.if_id,
                title = data.if_title,
                content = data.if_content,
                type = data.if_ig_id ?? 4,
                visible = true,
                filename1 = data.if_name_file_one,
                filename2 = data.if_name_file_two,
                filename3 = data.if_name_file_three,
                filename4 = data.if_name_file_four,
                filename5 = data.if_name_file_five,
            };

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DownloadEdit data)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/assets/upload/Download");

                var filename1 = ajax.UploadFile(data.file1, path);
                var filename2 = ajax.UploadFile(data.file2, path);
                var filename3 = ajax.UploadFile(data.file3, path);
                var filename4 = ajax.UploadFile(data.file4, path);
                var filename5 = ajax.UploadFile(data.file5, path);

                var updateData = db.information.Find(data.id);

                if (updateData != null)
                {
                    if (!String.IsNullOrEmpty(filename1)) ajax.DeleteFile($"{path}/{updateData.if_file_one}");
                    if (!String.IsNullOrEmpty(filename2)) ajax.DeleteFile($"{path}/{updateData.if_file_two}");
                    if (!String.IsNullOrEmpty(filename3)) ajax.DeleteFile($"{path}/{updateData.if_file_three}");
                    if (!String.IsNullOrEmpty(filename4)) ajax.DeleteFile($"{path}/{updateData.if_file_four}");
                    if (!String.IsNullOrEmpty(filename5)) ajax.DeleteFile($"{path}/{updateData.if_file_five}");

                    updateData.if_title = data.title;
                    updateData.if_content = data.content;
                    updateData.if_file_one = filename1 ?? updateData.if_file_one;
                    updateData.if_name_file_one = data.file1 != null ? data.file1.FileName : updateData.if_name_file_one;
                    updateData.if_file_two = filename2 ?? updateData.if_file_two;
                    updateData.if_name_file_two = data.file2 != null ? data.file2.FileName : updateData.if_name_file_two;
                    updateData.if_file_three = filename3 ?? updateData.if_file_three;
                    updateData.if_name_file_three = data.file3 != null ? data.file3.FileName : updateData.if_name_file_three;
                    updateData.if_file_four = filename4 ?? updateData.if_file_four;
                    updateData.if_name_file_four = data.file4 != null ? data.file4.FileName : updateData.if_name_file_four;
                    updateData.if_file_five = filename5 ?? updateData.if_file_five;
                    updateData.if_name_file_five = data.file5 != null ? data.file5.FileName : updateData.if_name_file_five;
                    updateData.if_visible = data.visible;

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
            if (id == 0 || id == null)
            {
                return HttpNotFound();
            }

            var data = db.information.Find(id);

            if (data == null)
            {
                return HttpNotFound();
            }

            string path = Server.MapPath("~/assets/upload/Download");

            ajax.DeleteFile($"{path}/{data.if_file_one}");
            ajax.DeleteFile($"{path}/{data.if_file_two}");
            ajax.DeleteFile($"{path}/{data.if_file_three}");
            ajax.DeleteFile($"{path}/{data.if_file_four}");
            ajax.DeleteFile($"{path}/{data.if_file_five}");

            db.information.Remove(data);
            db.SaveChanges();
            Session["msg"] = "刪除成功";
            return RedirectToAction("Index");
        }
    }
}