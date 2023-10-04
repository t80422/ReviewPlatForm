using PagedList;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class MembersController : Controller
    {
        private AjaxsController ajax = new AjaxsController();
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        // GET: Members
        public ActionResult Index(int? page = 1)
        {
            var userID = (int)Session["UserID"];
            var query = db.member.Where(x => x.mb_id_id == userID).ToList();
            var result = query.ToPagedList((int)page, 10);

            return View(result);
        }

        public ActionResult Create()
        {
            var mb_add_insur = new List<SelectListItem>()
            {
                new SelectListItem {Text="請選擇", Value="" },
                new SelectListItem {Text="加保", Value="1" },
                new SelectListItem {Text="退保", Value="2" },
                new SelectListItem {Text="調薪", Value="3" },
                new SelectListItem {Text="在職", Value="4" },
            };
            ViewBag.mb_add_insur = new SelectList(mb_add_insur, "Value", "Text", string.Empty);

            var mb_position = new List<SelectListItem>()
            {
                new SelectListItem {Text="請選擇", Value="" },
                new SelectListItem {Text="房務", Value="房務" },
                new SelectListItem {Text="清潔人員", Value="清潔人員" },
            };
            ViewBag.mb_position = new SelectList(mb_position, "Value", "Text", string.Empty);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Members data)
        {
            if (ModelState.IsValid)
            {
                //檢查身分證
                var peopleID = db.member.Where(x=>x.mb_id_card== data.mb_id_card).FirstOrDefault();
                if (peopleID != null)
                {
                    Session["msg"] = "此身分證已被登入";
                    return View(data);
                }

                string path = Server.MapPath("~/assets/upload/Members");

                var contract = ajax.UploadFile(data.mb_contract, path);
                var incom = ajax.UploadFile(data.mb_income_certificate, path);

                var insertData = new member()
                {
                    mb_name = data.mb_name,
                    mb_id_card = data.mb_id_card,
                    mb_add_insur = data.mb_add_insur,
                    mb_memo = data.mb_memo,
                    mb_insurance_id = data.mb_insurance_id,
                    mb_contract = contract,
                    mb_contract_name = data.mb_contract != null ? data.mb_contract.FileName : "",
                    mb_income_certificate = incom,
                    mb_income_certificate_name = data.mb_income_certificate != null ? data.mb_income_certificate.FileName : "",
                    mb_birthday = data.mb_birthday,
                    mb_insur_salary = data.mb_insur_salary,
                    mb_add_insur_date = data.mb_add_insur_date,
                    mb_surrender_date = data.mb_surrender_date,
                    mb_full_time_or_not = data.mb_full_time_or_not,
                    mb_full_time_date = data.mb_full_time_date,
                    mb_id_id = (int)Session["UserID"]
                };

                db.member.Add(insertData);
                db.SaveChanges();

                Session["msg"] = "新增成功";

                return RedirectToAction("Index", "Members");
            }
            return View(data);
        }

        public ActionResult Edit(int id)
        {
            if (id == 0)
            {
                return HttpNotFound();
            }

            var data = db.member.Find(id);

            if (data == null)
            {
                return HttpNotFound();
            }

            var result = new Members()
            {
                mb_id = data.mb_id,
                mb_name = data.mb_name,
                mb_id_card = data.mb_id_card,
                mb_add_insur = data.mb_add_insur,
                mb_memo = data.mb_memo,
                mb_insurance_id = data.mb_insurance_id,
                mb_contract_name = data.mb_contract != null ? data.mb_contract_name : "",
                mb_income_certificate_name = data.mb_income_certificate != null ? data.mb_income_certificate_name : "",
                mb_birthday = data.mb_birthday,
                mb_insur_salary = data.mb_insur_salary,
                mb_add_insur_date = data.mb_add_insur_date,
                mb_surrender_date = data.mb_surrender_date,
                mb_full_time_or_not = data.mb_full_time_or_not,
                mb_full_time_date = data.mb_full_time_date,
                mb_position = data.mb_position,
            };

            var mb_add_insur = new List<SelectListItem>()
            {
                new SelectListItem {Text="加保", Value="1" },
                new SelectListItem {Text="退保", Value="2" },
                new SelectListItem {Text="調薪", Value="3" },
                new SelectListItem {Text="在職", Value="4" },               
            };
            ViewBag.mb_add_insur = new SelectList(mb_add_insur, "Value", "Text", result.mb_add_insur);

            var mb_position = new List<SelectListItem>()
            {
                new SelectListItem {Text="房務", Value="房務" },
                new SelectListItem {Text="清潔人員", Value="清潔人員" },                
            };
            ViewBag.mb_position = new SelectList(mb_position, "Value", "Text", result.mb_position);

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Members data)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/assets/upload/Members");

                var contract = ajax.UploadFile(data.mb_contract, path);
                var incom = ajax.UploadFile(data.mb_income_certificate, path);

                var updateData = db.member.Find(data.mb_id);

                if (updateData != null)
                {
                    if (!string.IsNullOrEmpty(contract)) ajax.DeleteFile($"{path}/{updateData.mb_contract}");
                    if (!string.IsNullOrEmpty(incom)) ajax.DeleteFile($"{path}/{updateData.mb_income_certificate}");

                    updateData.mb_contract = contract ?? updateData.mb_contract;
                    updateData.mb_contract_name = data.mb_contract != null ? data.mb_contract.FileName : updateData.mb_contract_name;
                    updateData.mb_income_certificate = incom ?? updateData.mb_income_certificate;
                    updateData.mb_income_certificate_name = data.mb_income_certificate != null ? data.mb_income_certificate.FileName : updateData.mb_income_certificate_name;
                    updateData.mb_name = data.mb_name;
                    updateData.mb_id_card = data.mb_id_card;
                    updateData.mb_add_insur = data.mb_add_insur;
                    updateData.mb_memo = data.mb_memo;
                    updateData.mb_insurance_id = data.mb_insurance_id;
                    updateData.mb_birthday = data.mb_birthday;
                    updateData.mb_insur_salary = data.mb_insur_salary;
                    updateData.mb_add_insur_date = data.mb_add_insur_date;
                    updateData.mb_surrender_date = data.mb_surrender_date;
                    updateData.mb_full_time_or_not = data.mb_full_time_or_not;
                    updateData.mb_full_time_date = data.mb_full_time_date;
                    updateData.mb_position = data.mb_position;
                    updateData.mb_id_id = (int)Session["UserID"];

                    db.SaveChanges();
                    Session["msg"] = "修改成功";
                    return RedirectToAction("Index");
                }
            }
            return View(data);
        }

        public ActionResult Detail(int id)
        {
            var data = db.member.First(x => x.mb_id == id);

            if (data == null)
            {
                return HttpNotFound();
            }
            switch (int.Parse(data.mb_add_insur))
            {
                case 1:
                    ViewBag.mb_add_insur = "加保";
                    break;
                case 2:
                    ViewBag.mb_add_insur = "退保";
                    break;
                case 3:
                    ViewBag.mb_add_insur = "調薪";
                    break;
                case 4:

                default:
                    ViewBag.mb_add_insur = "在職";
                    break;
            }
            return View(data);
        }

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return HttpNotFound();
            }

            var data = db.member.Find(id);

            if (data == null)
            {
                return HttpNotFound();
            }

            db.member.Remove(data);
            db.SaveChanges();

            Session["msg"] = "刪除成功";
            return RedirectToAction("Index");
        }
    }
}