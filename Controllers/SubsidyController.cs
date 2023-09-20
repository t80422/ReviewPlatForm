using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SubsidyController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        // GET: Subsidy
        public ActionResult Index(int? page = 1)
        {
            page = page ?? 1;
            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            int perm = Session["perm"] != null ? (int)Session["perm"] : -1;

            List<SubsidyIndustry> query = new List<SubsidyIndustry>();

            #region 判斷是業者或是管理者
            // 業者
            if (perm == 3)
            {
                query = db.subsidy.Join(db.industry, a => a.s_id_id, b => b.id_id, (a, b) => new SubsidyIndustry
                {
                    id_room = b.id_room,
                    id_name = b.id_name,
                    s_date_time = a.s_date_time,
                    id_owner = b.id_owner,
                    s_id = a.s_id,
                    s_no = a.s_no,
                    s_review = a.s_review,
                    id_id = b.id_id,
                    s_submit = a.s_submit
                }
                ).Where(c => c.id_id == userID).OrderByDescending(c => c.s_date_time).ToList();
            }
            else
            {
                query = db.subsidy.Join(db.industry, a => a.s_id_id, b => b.id_id, (a, b) => new SubsidyIndustry
                {
                    id_room = b.id_room,
                    id_name = b.id_name,
                    s_date_time = a.s_date_time,
                    id_owner = b.id_owner,
                    s_id = a.s_id,
                    s_no = a.s_no,
                    s_review = a.s_review,
                    id_id = b.id_id,
                    s_submit = a.s_submit
                }
                ).OrderByDescending(c => c.s_date_time).ToList();
            }
            #endregion 判斷是業者或是管理者

            var result = query.ToPagedList((int)page, 10);
            var industry = db.industry.Find(userID);
            ViewBag.Name = industry != null ? industry.id_name : "";
            ViewBag.Maxmember = (industry != null ? (int)industry.id_room : 0) / 8;
            ViewBag.SubMemberCount = db.subsidy_member.Where(x => x.sm_id_id == userID).Count();
            ViewBag.IndustryId = userID;

            return View(result);
        }

        public ActionResult Create(int? id_id)
        {
            if (id_id == null || id_id == 0) return HttpNotFound();

            var data = new SubsidyEdit();
            data.id_id = (int)id_id;

            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            var industry = db.industry.Find(userID);
            ViewBag.Maxmember = (industry != null ? (int)industry.id_room : 0) / 8;
            ViewBag.SubMemberCount = db.subsidy_member.Where(x => x.sm_id_id == userID).Count();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubsidyEdit data)
        {
            if (ModelState.IsValid && data.id_id > 0)
            {
                var IndustryData = db.industry.Find(data.id_id);
                // 有無旅宿業者
                if (IndustryData == null) return HttpNotFound();
                // 同月份是否申請過
                var Month = DateTime.ParseExact(data.Date, "yyyy-MM", null);
                var SameMonth = db.subsidy.Where(x => x.s_date_time == Month && x.s_id_id == data.id_id).FirstOrDefault();
                if (SameMonth != null)
                {
                    var dateArray = data.Date.Split('-');
                    Session["msg"] = $"{dateArray[0]}年{dateArray[1]}月份已申請過";
                    return View(data);
                }

                #region 加入資料

                var insertData = new subsidy()
                {
                    s_id_id = data.id_id,
                    s_empcount = data.EmpCount,
                    s_date_time = DateTime.ParseExact(data.Date, "yyyy-MM", null),
                    s_money = data.Money,
                    s_last_time = DateTime.Now,
                    s_application = data.ApplicationName,
                    s_application_name = data.Application != null ? data.Application.FileName : "",
                    s_insur_member = data.LaborName,
                    s_insur_member_name = data.Labor != null ? data.Labor.FileName : "",
                    s_emp_lst = data.EmployeeListName,
                    s_emp_lst_name = data.EmployeeList != null ? data.EmployeeList.FileName : "",
                    s_affidavit = data.AffidavitName,
                    s_affidavit_name = data.Affidavit != null ? data.Affidavit.FileName : "",
                    s_receipt = data.ReceiptName,
                    s_receipt_name = data.Receipt != null ? data.Receipt.FileName : "",
                    s_else = data.OtherFileName,
                    s_else_name = data.OtherFile != null ? data.OtherFile.FileName : "",
                    s_applicants = data.ApplicantsListName,
                    s_applicants_name = data.ApplicantsList != null ? data.ApplicantsList.FileName : "",
                    s_review = "待補件"
                };

                db.subsidy.Add(insertData);
                db.SaveChanges();

                int pkValue = insertData.s_id;
                insertData.s_no = "A" + pkValue.ToString("D5");
                db.SaveChanges();

                #endregion 加入資料

                string path = Server.MapPath($"~/assets/upload/Subsidy/{IndustryData.id_name + "_" + IndustryData.id_id}");

                #region 上傳檔案

                data.ApplicationName = ajax.UploadFile(data.Application, path) ?? "";
                data.LaborName = ajax.UploadFile(data.Labor, path) ?? "";
                data.ApplicantsListName = ajax.UploadFile(data.ApplicantsList, path) ?? "";
                data.AffidavitName = ajax.UploadFile(data.Affidavit, path) ?? "";
                data.ReceiptName = ajax.UploadFile(data.Receipt, path) ?? "";
                data.EmployeeListName = ajax.UploadFile(data.EmployeeList, path) ?? "";
                data.OtherFileName = ajax.UploadFile(data.OtherFile, path) ?? "";

                #endregion 上傳檔案

                #region 加入申請人員清冊

                var ApplicantsList = ajax.ReadFile(data.ApplicantsList);
                List<subsidy_member> insertSMembers = new List<subsidy_member>();

                foreach (var sheet in ApplicantsList)
                {
                    // 第一列為標題
                    for (var i = 1; i < sheet.Value.Count(); i++)
                    {
                        if (String.IsNullOrEmpty(sheet.Value[i][3])) continue;

                        string insur;
                        switch (sheet.Value[i][5])
                        {
                            case "加保":
                                insur = "1"; break;
                            case "退保":
                                insur = "2"; break;
                            case "調薪":
                                insur = "3"; break;
                            default:
                                insur = ""; break;
                        }
                        int hire;
                        switch (sheet.Value[i][12])
                        {
                            case "新聘":
                                hire = 1; break;
                            case "部份工時轉正職":
                                hire = 2; break;
                            default:
                                hire = 0; break;
                        }
                        string IDCard = sheet.Value[i][3] ?? "";
                        member memberData = db.member.Where(x => x.mb_id_card == IDCard).FirstOrDefault() ?? new member();
                        memberData.mb_id = memberData.mb_id > 0 ? memberData.mb_id : 0;
                        memberData.mb_id_id = data.id_id;
                        memberData.mb_insurance_id = sheet.Value[i][0] ?? "";
                        memberData.mb_name = sheet.Value[i][1] ?? "";
                        memberData.mb_birthday = sheet.Value[i][2] ?? "";
                        memberData.mb_id_card = sheet.Value[i][3] ?? "";
                        memberData.mb_insur_salary = Convert.ToInt32(sheet.Value[i][4] ?? "0");
                        memberData.mb_add_insur = insur;
                        memberData.mb_add_insur_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][6] ?? ""));
                        memberData.mb_surrender_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][7] ?? ""));
                        memberData.mb_memo = sheet.Value[i][8] ?? "";
                        memberData.mb_full_time_or_not = sheet.Value[i][9] == "是";
                        memberData.mb_full_time_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][10] ?? ""));
                        memberData.mb_arrive_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][11] ?? ""));
                        memberData.mb_hire_type = hire;
                        memberData.mb_position = sheet.Value[i][13] ?? "";
                        memberData.mb_last_time = DateTime.Now;
                        memberData.mb_s_no = "A" + pkValue.ToString("D5");

                        db.member.AddOrUpdate(x => x.mb_id_card, memberData);
                        db.SaveChanges();

                        insertSMembers.Add(new subsidy_member()
                        {
                            sm_s_id = insertData.s_id,
                            sm_agree_start = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][14] ?? "")),
                            sm_agree_end = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][15] ?? "")),

                            sm_advance_money = Convert.ToInt32(sheet.Value[i][16] ?? "0"),
                            sm_id_id = data.id_id,
                            sm_mb_id = memberData.mb_id,
                            sm_review="待補件"
                        });
                    }
                }

                db.subsidy_member.AddRange(insertSMembers);
                db.SaveChanges();

                #endregion 加入申請人員清冊

                Session["msg"] = "本次案件申請案號成功,請按確定後,置下一步修改申請補助人員的資料";

                return RedirectToAction("Index", "SubMembers", new { id = pkValue });

            }
            Session["msg"] = "資料錯誤";
            return View(data);
        }

        public ActionResult Edit(int? id)
        {
            if (id == 0 || id == null) return HttpNotFound();

            var data = db.subsidy.Find(id);

            if (data == null) return HttpNotFound();

            var result = new SubsidyEdit()
            {
                s_id = id,
                id_id = data.s_id_id,
                EmpCount = data.s_empcount,
                Money = data.s_money,
                Date = data.s_date_time.ToString("yyyy-MM"),
                ApplicationName = data.s_application_name,
                LaborName = data.s_insur_member_name,
                ApplicantsListName = data.s_applicants_name,
                AffidavitName = data.s_affidavit_name,
                ReceiptName = data.s_receipt_name,
                EmployeeListName = data.s_emp_lst_name,
                OtherFileName = data.s_else_name
            };

            ViewBag.MemberList = db.member.Where(x => x.mb_s_no == data.s_no).ToList();

            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            var industry = db.industry.Find(userID);
            ViewBag.Maxmember = (industry != null ? (int)industry.id_room : 0) / 8;
            ViewBag.SubMemberCount = db.subsidy_member.Where(x => x.sm_id_id == userID).Count();

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubsidyEdit data)
        {
            if (ModelState.IsValid && data.id_id > 0)
            {
                var IndustryData = db.industry.Find(data.id_id);

                // 同月份是否申請過
                var Month = DateTime.ParseExact(data.Date, "yyyy-MM", null);
                var SameMonth = db.subsidy.Where(x => x.s_date_time == Month).FirstOrDefault();
                if (SameMonth != null && SameMonth.s_id_id == data.id_id && SameMonth.s_id != data.s_id)
                {
                    var dateArray = data.Date.Split('-');
                    Session["msg"] = $"{dateArray[0]}年{dateArray[1]}月份已申請過";
                    return View(data);
                }

                if (IndustryData == null) return HttpNotFound();

                string path = Server.MapPath($"~/assets/upload/Subsidy/{IndustryData.id_name + "_" + IndustryData.id_id}");

                var updateData = db.subsidy.Find(data.s_id);

                if (updateData == null) return HttpNotFound();

                #region 上傳檔案

                var NewApplocationName = ajax.UploadFile(data.Application, path) ?? "";
                var NewLaborName = ajax.UploadFile(data.Labor, path) ?? "";
                var NewApplicantsListName = ajax.UploadFile(data.ApplicantsList, path) ?? "";
                var NewAffidavitName = ajax.UploadFile(data.Affidavit, path) ?? "";
                var NewReceiptName = ajax.UploadFile(data.Receipt, path) ?? "";
                var NewEmployeeListName = ajax.UploadFile(data.EmployeeList, path) ?? "";
                var NewOtherFileName = ajax.UploadFile(data.OtherFile, path) ?? "";

                #endregion 上傳檔案

                #region 有新檔案刪除舊檔案

                if (!String.IsNullOrEmpty(NewApplocationName)) ajax.DeleteFile($"{path}/{updateData.s_application}");
                if (!String.IsNullOrEmpty(NewLaborName)) ajax.DeleteFile($"{path}/{updateData.s_insur_member}");
                if (!String.IsNullOrEmpty(NewApplicantsListName)) ajax.DeleteFile($"{path}/{updateData.s_applicants}");
                if (!String.IsNullOrEmpty(NewAffidavitName)) ajax.DeleteFile($"{path}/{updateData.s_affidavit}");
                if (!String.IsNullOrEmpty(NewReceiptName)) ajax.DeleteFile($"{path}/{updateData.s_receipt}");
                if (!String.IsNullOrEmpty(NewEmployeeListName)) ajax.DeleteFile($"{path}/{updateData.s_emp_lst}");
                if (!String.IsNullOrEmpty(NewOtherFileName)) ajax.DeleteFile($"{path}/{updateData.s_else}");

                #endregion 有新檔案刪除舊檔案

                #region 更新資料

                updateData.s_empcount = data.EmpCount;
                updateData.s_date_time = DateTime.ParseExact(data.Date, "yyyy-MM", null);
                updateData.s_money = data.Money;
                updateData.s_last_time = DateTime.Now;
                updateData.s_application = data.ApplicationName;
                updateData.s_application_name = data.Application != null ? data.Application.FileName : updateData.s_application_name;
                updateData.s_insur_member = data.LaborName;
                updateData.s_insur_member_name = data.Labor != null ? data.Labor.FileName : updateData.s_insur_member_name;
                updateData.s_emp_lst = data.EmployeeListName;
                updateData.s_emp_lst_name = data.EmployeeList != null ? data.EmployeeList.FileName : updateData.s_emp_lst_name;
                updateData.s_affidavit = data.AffidavitName;
                updateData.s_affidavit_name = data.Affidavit != null ? data.Affidavit.FileName : updateData.s_affidavit_name;
                updateData.s_receipt = data.ReceiptName;
                updateData.s_receipt_name = data.Receipt != null ? data.Receipt.FileName : updateData.s_receipt_name;
                updateData.s_else = data.OtherFileName;
                updateData.s_else_name = data.OtherFile != null ? data.OtherFile.FileName : updateData.s_else_name;
                updateData.s_applicants = data.ApplicantsListName;
                updateData.s_applicants_name = data.ApplicantsList != null ? data.ApplicantsList.FileName : updateData.s_applicants_name;

                db.SaveChanges();

                #endregion 更新資料

                #region 更新人員清冊

                string PK = updateData.s_no;

                if (data.ApplicantsList != null)
                {
                    // 先刪除舊的人員
                    db.subsidy_member.RemoveRange(db.subsidy_member.Where(x => x.sm_s_id == data.s_id).ToList());
                    db.SaveChanges();

                    var ApplicantsList = ajax.ReadFile(data.ApplicantsList);
                    List<subsidy_member> insertSMembers = new List<subsidy_member>();

                    foreach (var sheet in ApplicantsList)
                    {
                        // 第0列為標題，從1開始
                        for (var i = 1; i < sheet.Value.Count(); i++)
                        {
                            if (String.IsNullOrEmpty(sheet.Value[i][3])) continue;

                            string insur;
                            switch (sheet.Value[i][5])
                            {
                                case "加保":
                                    insur = "1"; break;
                                case "退保":
                                    insur = "2"; break;
                                case "調薪":
                                    insur = "3"; break;
                                default:
                                    insur = ""; break;
                            }
                            int hire;
                            switch (sheet.Value[i][12])
                            {
                                case "新聘":
                                    hire = 1; break;
                                case "部份工時轉正職":
                                    hire = 2; break;
                                default:
                                    hire = 0; break;
                            }
                            string IDCard = sheet.Value[i][3] ?? "";
                            member memberData = db.member.Where(x => x.mb_id_card == IDCard).FirstOrDefault() ?? new member();
                            memberData.mb_id = memberData.mb_id > 0 ? memberData.mb_id : 0;
                            memberData.mb_id_id = data.id_id;
                            memberData.mb_insurance_id = sheet.Value[i][0] ?? "";
                            memberData.mb_name = sheet.Value[i][1] ?? "";
                            memberData.mb_birthday = sheet.Value[i][2] ?? "";
                            memberData.mb_id_card = sheet.Value[i][3] ?? "";
                            memberData.mb_insur_salary = Convert.ToInt32(sheet.Value[i][4] ?? "0");
                            memberData.mb_add_insur = insur;
                            memberData.mb_add_insur_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][6] ?? ""));
                            memberData.mb_surrender_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][7] ?? ""));
                            memberData.mb_memo = sheet.Value[i][8] ?? "";
                            memberData.mb_full_time_or_not = sheet.Value[i][9] == "是";
                            memberData.mb_full_time_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][10] ?? ""));
                            memberData.mb_arrive_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][11] ?? ""));
                            memberData.mb_hire_type = hire;
                            memberData.mb_position = sheet.Value[i][13] ?? "";
                            memberData.mb_last_time = DateTime.Now;
                            memberData.mb_s_no = PK;

                            db.member.AddOrUpdate(x => x.mb_id_card, memberData);
                            db.SaveChanges();

                            insertSMembers.Add(new subsidy_member()
                            {
                                sm_s_id = data.s_id,
                                sm_agree_start = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][14] ?? "")),
                                sm_agree_end = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][15] ?? "")),

                                sm_advance_money = Convert.ToInt32(sheet.Value[i][16] ?? "0"),
                                sm_id_id = data.id_id,
                                sm_mb_id = memberData.mb_id,







                            });

                        }
                    }

                    db.subsidy_member.AddRange(insertSMembers);
                    db.SaveChanges();
                }

                #endregion 更新人員清冊

                Session["msg"] = "修改成功";
                return RedirectToAction("Index");

            }
            return View(data);
        }

        public ActionResult Detail(int id)
        {
            var data = db.subsidy.First(x => x.s_id == id);

            if (data == null)
            {
                return HttpNotFound();
            }

            var result = new SubsidyEdit()
            {
                s_id = id,
                id_id = data.s_id_id,
                EmpCount = data.s_empcount,
                Money = data.s_money,
                Date = data.s_date_time.ToString("yyyy-MM"),
                ApplicationName = data.s_application_name,
                LaborName = data.s_insur_member_name,
                ApplicantsListName = data.s_applicants_name,
                AffidavitName = data.s_affidavit_name,
                ReceiptName = data.s_receipt_name,
                EmployeeListName = data.s_emp_lst_name,
                OtherFileName = data.s_else_name,
                Review = data.s_review
            };

            return View(result);
        }

        public ActionResult Report()
        {
            return View();
        }

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return HttpNotFound();
            }

            var data = db.subsidy.Find(id);

            if (data == null)
            {
                return HttpNotFound();
            }

            string path = Server.MapPath("~/assets/upload/Subsidy");

            ajax.DeleteFile($"{path}/{data.s_insur_member}");
            ajax.DeleteFile($"{path}/{data.s_emp_lst}");
            ajax.DeleteFile($"{path}/{data.s_affidavit}");
            ajax.DeleteFile($"{path}/{data.s_receipt}");
            ajax.DeleteFile($"{path}/{data.s_else}");

            db.subsidy.Remove(data);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Status(int? id)
        {
            if (id == 0 || id == null) return HttpNotFound();

            var data = db.subsidy.Find(id);

            if (data == null) return HttpNotFound();

            data.s_review = "審核中";

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult InsurList()
        {
            var id = (int)Session["UserID"];
            var data = db.subsidy.Find(id);

            return View(data);
        }
    }
}