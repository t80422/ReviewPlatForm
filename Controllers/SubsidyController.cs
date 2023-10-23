using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class SubsidyController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        public ActionResult Index(string key, string review, int? page = 1)
        {
            //撈出所有資料
            var subsidies = db.subsidy.Join(
                db.industry,
                s => s.s_id_id,
                i => i.id_id,
                (s, i) => new SubsidyIndustry
                {
                    s_no = s.s_no,
                    s_date_time = s.s_date_time,
                    id_owner = i.id_owner,
                    s_grant_date = s.s_grant_date,
                    s_money = s.s_money ?? 0,
                    s_review = s.s_review_fst,
                    id_name = i.id_name,
                    s_id = s.s_id,
                    id_id = i.id_id,
                    s_mg_id_fst = s.s_mg_id_fst,
                    id_review = i.id_review,
                });

            //權限控制
            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            int perm = Session["perm"] != null ? (int)Session["perm"] : -1;

            switch (perm)
            {
                case 3:
                    subsidies = subsidies.Where(s => s.id_id == userID);
                    break;
                case 2:
                    subsidies = subsidies.Where(s => s.s_mg_id_fst == userID);
                    break;
            }

            //搜尋功能=====
            //關鍵字搜尋
            if (!string.IsNullOrWhiteSpace(key) || !string.IsNullOrWhiteSpace(review))
            {
                subsidies = subsidies.Where(x => x.s_no.Contains(key) || x.id_name.Contains(key));
            }

            //審核狀態搜尋
            if (!string.IsNullOrWhiteSpace(review))
            {
                subsidies = subsidies.Where(x => x.s_review == review);
            }
            //=============

            var industry = db.industry.Find(userID);
            double room = industry?.id_room ?? 0;

            ViewBag.Maxmember = Math.Max(1, Math.Ceiling(room / 8));
            ViewBag.MemberCount = db.member.Count(x => x.mb_id_id == userID);
            ViewBag.IndustryId = userID;

            //審核人員
            var joinResult = db.user_accounts.Where(x => x.ua_perm == 2).Join(
                db.manager,
                ua => ua.ua_user_id,
                m => m.mg_id,
                (ua, m) => new ReviewerViewModel
                {
                    Name = m.mg_name,
                    ID = m.mg_id,
                }).ToList();

            joinResult.Insert(0, new ReviewerViewModel { Name = "審核人員", ID = -1 });

            ViewBag.Reviewers = new SelectList(joinResult, "ID", "Name", string.Empty);

            var data = new SubsidyIndexViewModel()
            {
                Subsidies = subsidies.OrderByDescending(s => s.s_id).ToPagedList((int)page, 100),
                Reviewers = joinResult
            };

            return View(data);
        }

        public ActionResult ChangePerson(string id, int? person_id = 0)
        {
            id = id.Replace("Review", "");

            var updateSubsidy = db.subsidy.Find(Int32.Parse(id));
            updateSubsidy.s_mg_id_fst = person_id;
            updateSubsidy.s_review_fst = "審核中";

            var updateIndustry = db.industry.Find(updateSubsidy.s_id_id);
            updateIndustry.id_mg_id_fst = (int)person_id;
            updateIndustry.id_review = "審核中";

            db.SaveChanges();
            return Json("分案完成!!", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(int id_id)
        {
            var data = new SubsidyEdit
            {
                id_id = id_id
            };

            var industry = db.industry.Find(id_id);

            if (industry == null)
            {
                string errMsg = "No industry found for the given id: " + id_id;
                return View("Error", errMsg);
            }

            double room = industry.id_room ?? 0;
            double maxMember = Math.Ceiling(room / 8);
            ViewBag.Maxmember = maxMember == 0 ? 1 : maxMember;
            ViewBag.MemberCount = db.member.Count(x => x.mb_id_id == id_id);

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

                #region 上傳檔案

                string path = Server.MapPath($"~/assets/upload/Subsidy/{IndustryData.id_name + "_" + IndustryData.id_id}");
                var Application = ajax.UploadFile(data.Application, path) ?? "";
                var Labor = ajax.UploadFile(data.Labor, path) ?? "";
                var ApplicantsList = ajax.UploadFile(data.ApplicantsList, path) ?? "";
                var Affidavit = ajax.UploadFile(data.Affidavit, path) ?? "";
                var Receipt = ajax.UploadFile(data.Receipt, path) ?? "";
                var EmployeeList = ajax.UploadFile(data.EmployeeList, path) ?? "";
                var OtherFile = ajax.UploadFile(data.OtherFile, path) ?? "";
                var OtherFile2 = ajax.UploadFile(data.s_else_two, path) ?? "";
                var OtherFile3 = ajax.UploadFile(data.s_else_three, path) ?? "";

                #endregion 上傳檔案

                #region 加入資料

                var insertData = new subsidy()
                {
                    s_id_id = data.id_id,
                    s_empcount = data.EmpCount,
                    s_date_time = data.Date,
                    s_money = data.Money ?? 0,
                    s_last_time = DateTime.Now,
                    s_application = Application,
                    s_application_name = data.Application != null ? data.Application.FileName : "",
                    s_insur_member = Labor,
                    s_insur_member_name = data.Labor != null ? data.Labor.FileName : "",
                    s_emp_lst = EmployeeList,
                    s_emp_lst_name = data.EmployeeList != null ? data.EmployeeList.FileName : "",
                    s_affidavit = Affidavit,
                    s_affidavit_name = data.Affidavit != null ? data.Affidavit.FileName : "",
                    s_receipt = Receipt,
                    s_receipt_name = data.Receipt != null ? data.Receipt.FileName : "",
                    s_else = OtherFile,
                    s_else_name = data.OtherFile != null ? data.OtherFile.FileName : "",
                    s_else_two = OtherFile2,
                    s_else_two_name = data.s_else_two != null ? data.s_else_two.FileName : "",
                    s_else_three = OtherFile3,
                    s_else_three_name = data.s_else_three != null ? data.s_else_three.FileName : "",
                    s_applicants = ApplicantsList,
                    s_applicants_name = data.ApplicantsList != null ? data.ApplicantsList.FileName : "",
                    s_review_fst = "待審核",
                    s_date_time_end = data.s_date_time_end
                };

                db.subsidy.Add(insertData);
                db.SaveChanges();

                int pkValue = insertData.s_id;
                insertData.s_no = "A" + pkValue.ToString("D5");
                db.SaveChanges();

                #endregion 加入資料

                #region 加入申請人員清冊

                var AppList = ajax.ReadFile(data.ApplicantsList);
                List<subsidy_member> insertSMembers = new List<subsidy_member>();

                foreach (var sheet in AppList)
                {
                    // 第一列為標題
                    for (var i = 1; i < sheet.Value.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(sheet.Value[i][3])) continue;

                        int hire;

                        switch (sheet.Value[i][10])
                        {
                            case "新聘":
                                hire = 1; break;
                            case "部份工時轉正職":
                                hire = 2; break;
                            default:
                                hire = 0; break;
                        }

                        string IDCard = sheet.Value[i][3] ?? "";

                        if (IDCard != "")
                        {
                            //modify by v0.9=====
                            //member memberData = db.member.Where(x => x.mb_id_card == IDCard).FirstOrDefault() ?? new member();
                            //===================
                            member memberData = db.member.Where(x => x.mb_id_card == IDCard && x.mb_id_id == data.id_id).FirstOrDefault() ?? new member();
                            //===================

                            memberData.mb_id = memberData.mb_id > 0 ? memberData.mb_id : 0;
                            memberData.mb_id_id = data.id_id;
                            memberData.mb_insurance_id = sheet.Value[i][0] ?? "";
                            memberData.mb_name = sheet.Value[i][1] ?? "";
                            memberData.mb_birthday = sheet.Value[i][2] ?? "";
                            memberData.mb_id_card = sheet.Value[i][3] ?? "";
                            memberData.mb_insur_salary = Convert.ToInt32(sheet.Value[i][4] ?? "0");
                            memberData.mb_add_insur_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][5] ?? ""));
                            memberData.mb_surrender_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][6] ?? ""));
                            memberData.mb_full_time_or_not = sheet.Value[i][7] == "是";
                            memberData.mb_full_time_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][8] ?? ""));
                            memberData.mb_arrive_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][9] ?? ""));
                            memberData.mb_hire_type = hire;
                            memberData.mb_position = sheet.Value[i][11] ?? "";
                            memberData.mb_last_time = DateTime.Now;
                            memberData.mb_s_no = "A" + pkValue.ToString("D5");

                            //modyfy by v0.9=====
                            //db.member.AddOrUpdate(x => x.mb_id_card, memberData);
                            //===================
                            if (memberData.mb_id <= 0)
                            {
                                db.member.Add(memberData);
                            }
                            //===================

                            db.SaveChanges();

                            insertSMembers.Add(new subsidy_member()
                            {
                                sm_s_id = insertData.s_id,
                                sm_agree_start = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][12] ?? "")),
                                sm_agree_end = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][13] ?? "")),

                                sm_advance_money = Convert.ToInt32(sheet.Value[i][14] ?? "0"),
                                sm_id_id = data.id_id,
                                sm_mb_id = memberData.mb_id,
                                sm_review = "待補件"
                            });
                        }
                    }
                }

                db.subsidy_member.AddRange(insertSMembers);
                db.SaveChanges();

                #endregion 加入申請人員清冊

                Session["msg"] = "本次案件申請案號成功,請按確定後,至下一步上傳申請補助人員相關資料";

                return RedirectToAction("Index", "SubMembers", new { id = pkValue });

            }
            Session["msg"] = "資料錯誤";
            return View(data);
        }

        public ActionResult Edit(int? id)
        {
            if (id == 0 || id == null) return HttpNotFound();

            var data = db.subsidy.Where(x => x.s_id == id).Join(db.industry, x => x.s_id_id, y => y.id_id, (x, y) => new SubsidyEdit
            {
                s_id = id,
                id_id = x.s_id_id,
                EmpCount = x.s_empcount,
                Money = x.s_money,
                Date = x.s_date_time,
                ApplicationName = x.s_application_name,
                LaborName = x.s_insur_member_name,
                ApplicantsListName = x.s_applicants_name,
                AffidavitName = x.s_affidavit_name,
                ReceiptName = x.s_receipt_name,
                EmployeeListName = x.s_emp_lst_name,
                OtherFileName = x.s_else_name,
                s_else_two_name = x.s_else_two_name,
                s_else_three_name = x.s_else_three_name,
                id_name = y.id_name,
                s_no = x.s_no,
                s_date_time_end = x.s_date_time_end
            }).FirstOrDefault();

            if (data == null) return HttpNotFound();

            ViewBag.SubMemberList = db.subsidy_member.Where(x => x.sm_s_id == data.s_id).Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubMembersEdit
            {
                mb_name = y.mb_name,
                mb_id_card = y.mb_id_card,
                mb_birthday = y.mb_birthday,
                mb_add_insur_date = y.mb_add_insur_date,
                mb_surrender_date = y.mb_surrender_date,
                mb_insur_salary = (int)y.mb_insur_salary,
                mb_memo = y.mb_memo
            }).ToList();

            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            var industry = db.industry.Find(userID);

            //modify by v0.7 20230928
            //ViewBag.Maxmember = (industry != null ? (int)industry.id_room : 0) / 8;
            double room = industry?.id_room ?? 0;
            double maxMember = Math.Ceiling(room / 8);
            ViewBag.Maxmember = maxMember;
            //modify by v0.7 20230928

            if (ViewBag.Maxmember == 0) { ViewBag.Maxmember = 1; }
            ViewBag.SubMemberCount = db.member.Where(x => x.mb_id_id == userID).Count();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubsidyEdit data)
        {
            if (ModelState.IsValid && data.id_id > 0)
            {
                var IndustryData = db.industry.Find(data.id_id);
                if (IndustryData == null) return HttpNotFound();

                var updateData = db.subsidy.Find(data.s_id);
                if (updateData == null) return HttpNotFound();

                #region 上傳檔案

                string path = Server.MapPath($"~/assets/upload/Subsidy/{IndustryData.id_name + "_" + IndustryData.id_id}");
                var Application = ajax.UploadFile(data.Application, path) ?? "";
                var Labor = ajax.UploadFile(data.Labor, path) ?? "";
                var ApplicantsList = ajax.UploadFile(data.ApplicantsList, path) ?? "";
                var Affidavit = ajax.UploadFile(data.Affidavit, path) ?? "";
                var Receipt = ajax.UploadFile(data.Receipt, path) ?? "";
                var EmployeeList = ajax.UploadFile(data.EmployeeList, path) ?? "";
                var OtherFile = ajax.UploadFile(data.OtherFile, path) ?? "";
                var OtherFile2 = ajax.UploadFile(data.s_else_two, path) ?? "";
                var OtherFile3 = ajax.UploadFile(data.s_else_three, path) ?? "";

                #endregion 上傳檔案

                #region 有新檔案刪除舊檔案

                if (!String.IsNullOrEmpty(Application)) ajax.DeleteFile($"{path}/{updateData.s_application}");
                if (!String.IsNullOrEmpty(Labor)) ajax.DeleteFile($"{path}/{updateData.s_insur_member}");
                if (!String.IsNullOrEmpty(ApplicantsList)) ajax.DeleteFile($"{path}/{updateData.s_applicants}");
                if (!String.IsNullOrEmpty(Affidavit)) ajax.DeleteFile($"{path}/{updateData.s_affidavit}");
                if (!String.IsNullOrEmpty(Receipt)) ajax.DeleteFile($"{path}/{updateData.s_receipt}");
                if (!String.IsNullOrEmpty(EmployeeList)) ajax.DeleteFile($"{path}/{updateData.s_emp_lst}");
                if (!String.IsNullOrEmpty(OtherFile)) ajax.DeleteFile($"{path}/{updateData.s_else}");
                if (!String.IsNullOrEmpty(OtherFile2)) ajax.DeleteFile($"{path}/{updateData.s_else_two}");
                if (!String.IsNullOrEmpty(OtherFile3)) ajax.DeleteFile($"{path}/{updateData.s_else_three}");

                #endregion 有新檔案刪除舊檔案

                #region 更新資料

                updateData.s_empcount = data.EmpCount;
                updateData.s_date_time = data.Date;
                updateData.s_money = data.Money ?? 0;
                updateData.s_last_time = DateTime.Now;
                updateData.s_application = Application;
                updateData.s_application_name = data.Application != null ? data.Application.FileName : updateData.s_application_name;
                updateData.s_insur_member = Labor;
                updateData.s_insur_member_name = data.Labor != null ? data.Labor.FileName : updateData.s_insur_member_name;
                updateData.s_emp_lst = EmployeeList;
                updateData.s_emp_lst_name = data.EmployeeList != null ? data.EmployeeList.FileName : updateData.s_emp_lst_name;
                updateData.s_affidavit = Affidavit;
                updateData.s_affidavit_name = data.Affidavit != null ? data.Affidavit.FileName : updateData.s_affidavit_name;
                updateData.s_receipt = Receipt;
                updateData.s_receipt_name = data.Receipt != null ? data.Receipt.FileName : updateData.s_receipt_name;
                updateData.s_else = OtherFile;
                updateData.s_else_name = data.OtherFile != null ? data.OtherFile.FileName : updateData.s_else_name;
                updateData.s_else_two = OtherFile2;
                updateData.s_else_two_name = data.s_else_two != null ? data.s_else_two.FileName : updateData.s_else_two_name;
                updateData.s_else_three = OtherFile3;
                updateData.s_else_three_name = data.s_else_three != null ? data.s_else_three.FileName : updateData.s_else_three_name;
                updateData.s_applicants = ApplicantsList;
                updateData.s_applicants_name = data.ApplicantsList != null ? data.ApplicantsList.FileName : updateData.s_applicants_name;

                db.SaveChanges();

                #endregion 更新資料

                #region 更新人員清冊

                if (data.ApplicantsList != null)
                {
                    var AppList = ajax.ReadFile(data.ApplicantsList);

                    List<subsidy_member> insertSMembers = new List<subsidy_member>();

                    foreach (var sheet in AppList)
                    {
                        // 第一列為標題
                        for (var i = 1; i < sheet.Value.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet.Value[i][3])) continue;

                            int hire;

                            switch (sheet.Value[i][10])
                            {
                                case "新聘":
                                    hire = 1; break;
                                case "部份工時轉正職":
                                    hire = 2; break;
                                default:
                                    hire = 0; break;
                            }

                            string IDCard = sheet.Value[i][3] ?? "";

                            if (IDCard != "")
                            {
                                member memberData = db.member.FirstOrDefault(x => x.mb_id_card == IDCard && x.mb_id_id == data.id_id) ?? new member();

                                memberData.mb_id = memberData.mb_id > 0 ? memberData.mb_id : 0;
                                memberData.mb_id_id = data.id_id;
                                memberData.mb_insurance_id = sheet.Value[i][0] ?? "";
                                memberData.mb_name = sheet.Value[i][1] ?? "";
                                memberData.mb_birthday = sheet.Value[i][2] ?? "";
                                memberData.mb_id_card = sheet.Value[i][3] ?? "";
                                memberData.mb_insur_salary = Convert.ToInt32(sheet.Value[i][4] ?? "0");
                                memberData.mb_add_insur_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][5] ?? ""));
                                memberData.mb_surrender_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][6] ?? ""));
                                memberData.mb_full_time_or_not = sheet.Value[i][7] == "是";
                                memberData.mb_full_time_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][8] ?? ""));
                                memberData.mb_arrive_date = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][9] ?? ""));
                                memberData.mb_hire_type = hire;
                                memberData.mb_position = sheet.Value[i][11] ?? "";
                                memberData.mb_last_time = DateTime.Now;
                                memberData.mb_s_no = data.s_no;

                                //modyfy by v0.9=====
                                if (memberData.mb_id <= 0)
                                {
                                    db.member.Add(memberData);
                                }
                                //===================
                                //db.member.AddOrUpdate(x => new { x.mb_id_card, x.mb_id_id }, memberData);
                                //===================

                                db.SaveChanges();

                                subsidy_member subMemberData = db.subsidy_member
                                    .Where(x => x.sm_s_id.HasValue && x.sm_s_id.Value == data.s_id
                                             && x.sm_mb_id.HasValue && x.sm_mb_id.Value == memberData.mb_id)
                                    .FirstOrDefault();

                                if (subMemberData != null)
                                {
                                    db.subsidy_member.Remove(subMemberData);
                                }

                                insertSMembers.Add(new subsidy_member()
                                {
                                    sm_s_id = data.s_id,
                                    sm_agree_start = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][12] ?? "")),
                                    sm_agree_end = DateTime.FromOADate(Convert.ToDouble(sheet.Value[i][13] ?? "")),
                                    sm_advance_money = Convert.ToInt32(sheet.Value[i][14] ?? "0"),
                                    sm_id_id = data.id_id,
                                    sm_mb_id = memberData.mb_id,
                                    sm_review = "待補件"
                                });
                            }
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

        public ActionResult Edit_Manager(int subsidyID)
        {
            var subsidy = db.subsidy.Find(subsidyID);
            var industry = db.industry.First(x => x.id_id == subsidy.s_id_id);
            var initialReviewer = db.manager.Find(subsidy.s_mg_id_fst);
            var secondaryReviewer = db.manager.Find(subsidy.s_mg_id_snd);
            var associationReviewer = db.manager.Find(subsidy.s_mg_id_association);

            var fileReviewOptions = new List<SelectListItem>
            {
                new SelectListItem{Text="待補件",Value="待補件"},
                new SelectListItem{Text="審核中",Value="審核中"},
                new SelectListItem{Text="通過",Value="通過"},
            };
            var reviewOptions = new List<SelectListItem>
            {
                new SelectListItem{Text="待審核",Value="待審核"},
                new SelectListItem{Text="審核中",Value="審核中"},
                new SelectListItem{Text="待補件",Value="待補件"},
                new SelectListItem{Text="退件",Value="退件"},
                new SelectListItem{Text="審核完成",Value="審核完成"},
            };

            var subsidyData = new Subsidy_EditManager
            {
                Subsidy = subsidy,
                Industry = industry,
                ReviewOptions = reviewOptions,
                FileReviewOptions = new SelectList(fileReviewOptions, "Value", "Text", string.Empty),
                InitialReviewer = initialReviewer,
                SecondaryReviewer = secondaryReviewer,
                AssociationReviewer = associationReviewer,
                SubMemberList = db.subsidy_member.
                    Where(x => x.sm_s_id == subsidyID).
                    Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubMemberList
                    {
                        Subsidy_Member = x,
                        Member = y
                    }).ToList()
            };

            int sumAdvanceMoney = subsidyData.SubMemberList
                .Where(x => x.Member.mb_review == "通過")
                .Sum(x => x.Subsidy_Member.sm_advance_money.Value);

            subsidyData.TrialAmount = sumAdvanceMoney;

            double room = industry?.id_room ?? 0;
            double maxMember = Math.Ceiling(room / 8);
            ViewBag.Maxmember = maxMember;

            if (ViewBag.Maxmember == 0) { ViewBag.Maxmember = 1; }
            ViewBag.SubMemberCount = db.member.Where(x => x.mb_id_id == industry.id_id).Count();

            return View(subsidyData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Manager(Subsidy_EditManager data)
        {
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var updateSubsidy = db.subsidy.Find(data.Subsidy.s_id);

            #region 上傳檔案

            string path = Server.MapPath($"~/assets/upload/Subsidy/{data.Industry.id_name + "_" + data.Industry.id_id}");

            var Application = ajax.UploadFile(data.ApplicationFile, path) ?? "";
            var InsurMember = ajax.UploadFile(data.InsurMemberFile, path) ?? "";
            var Applicants = ajax.UploadFile(data.ApplicantsFile, path) ?? "";
            var EmployeeList = ajax.UploadFile(data.EmpListFile, path) ?? "";
            var ElseOne = ajax.UploadFile(data.ElseOneFile, path) ?? "";
            var ElseTwo = ajax.UploadFile(data.ElseTwoFile, path) ?? "";
            var ElseThree = ajax.UploadFile(data.ElseThreeFile, path) ?? "";

            if (!string.IsNullOrEmpty(Application)) ajax.DeleteFile($"{path}/{data.Subsidy.s_application}");
            if (!string.IsNullOrEmpty(InsurMember)) ajax.DeleteFile($"{path}/{data.Subsidy.s_insur_member}");
            if (!string.IsNullOrEmpty(Applicants)) ajax.DeleteFile($"{path}/{data.Subsidy.s_applicants}");
            if (!string.IsNullOrEmpty(EmployeeList)) ajax.DeleteFile($"{path}/{data.Subsidy.s_emp_lst}");
            if (!string.IsNullOrEmpty(ElseOne)) ajax.DeleteFile($"{path}/{data.Subsidy.s_else}");
            if (!string.IsNullOrEmpty(ElseTwo)) ajax.DeleteFile($"{path}/{data.Subsidy.s_else_two}");
            if (!string.IsNullOrEmpty(ElseThree)) ajax.DeleteFile($"{path}/{data.Subsidy.s_else_three}");

            updateSubsidy.s_application = Application;
            updateSubsidy.s_application_name = data.ApplicationFile != null ? data.ApplicationFile.FileName : updateSubsidy.s_application_name;
            updateSubsidy.s_insur_member = InsurMember;
            updateSubsidy.s_insur_member_name = data.InsurMemberFile != null ? data.InsurMemberFile.FileName : updateSubsidy.s_insur_member_name;
            updateSubsidy.s_emp_lst = EmployeeList;
            updateSubsidy.s_emp_lst_name = data.EmpListFile != null ? data.EmpListFile.FileName : updateSubsidy.s_emp_lst_name;
            updateSubsidy.s_applicants = Applicants;
            updateSubsidy.s_applicants_name = data.ApplicantsFile != null ? data.ApplicantsFile.FileName : updateSubsidy.s_applicants_name;
            updateSubsidy.s_else = ElseOne;
            updateSubsidy.s_else_name = data.ElseOneFile != null ? data.ElseOneFile.FileName : updateSubsidy.s_else_name;
            updateSubsidy.s_else_two = ElseTwo;
            updateSubsidy.s_else_two_name = data.ElseTwoFile != null ? data.ElseTwoFile.FileName : updateSubsidy.s_else_two_name;
            updateSubsidy.s_else_three = ElseThree;
            updateSubsidy.s_else_three_name = data.ElseThreeFile != null ? data.ElseThreeFile.FileName : updateSubsidy.s_else_three_name;

            #endregion 上傳檔案

            #region 更新補助申請            

            updateSubsidy.s_review_application = data.Subsidy.s_review_application;
            updateSubsidy.s_review_insur_member = data.Subsidy.s_review_insur_member;
            updateSubsidy.s_review_emp_lst = data.Subsidy.s_review_emp_lst;
            updateSubsidy.s_review_applicants = data.Subsidy.s_review_applicants;
            updateSubsidy.s_money = data.Subsidy.s_money;
            updateSubsidy.s_empcount = data.Subsidy.s_empcount;
            updateSubsidy.s_review_else = data.Subsidy.s_review_else;
            updateSubsidy.s_review_else2 = data.Subsidy.s_review_else2;
            updateSubsidy.s_review_else3 = data.Subsidy.s_review_else3;
            updateSubsidy.s_review_fst = data.Subsidy.s_review_fst;
            updateSubsidy.s_grant_date = data.Subsidy.s_grant_date;
            updateSubsidy.s_approved_amount = data.Subsidy.s_approved_amount;

            var perm = (int)Session["perm"];
            var managerID = (int)Session["UserID"];

            switch (perm)
            {
                case 0:
                    updateSubsidy.s_mg_id_association = managerID;
                    break;

                case 1:
                    updateSubsidy.s_mg_id_snd = managerID;
                    break;

                case 2:
                    updateSubsidy.s_mg_id_fst = managerID;
                    break;

                default:
                    break;
            }

            #endregion

            #region 新增審核紀錄

            var insertReview = new subsidy_review();
            var reviewer = db.manager.Find(managerID).mg_name;

            insertReview.sr_s_id = data.Subsidy.s_id;
            insertReview.sr_reviewer = reviewer;
            insertReview.sr_date = DateTime.Now.Date;
            insertReview.sr_time = DateTime.Now.TimeOfDay;
            insertReview.sr_application_amount = data.Subsidy.s_money;
            insertReview.sr_review_application = data.Subsidy.s_review_application;
            insertReview.sr_review_insur_member = data.Subsidy.s_review_insur_member;
            insertReview.sr_review_emp_lst = data.Subsidy.s_review_emp_lst;
            insertReview.sr_review_applicants = data.Subsidy.s_review_applicants;
            insertReview.sr_review_else = data.Subsidy.s_review_else;
            insertReview.sr_review_else2 = data.Subsidy.s_review_else2;
            insertReview.sr_review_else3 = data.Subsidy.s_review_else3;

            db.subsidy_review.Add(insertReview);

            #endregion

            db.SaveChanges();

            Session["msg"] = "存檔成功";

            return RedirectToAction("Index");
        }

        public ActionResult Detail(int id)
        {
            var data = db.subsidy.Where(x => x.s_id == id).Join(db.industry, x => x.s_id_id, y => y.id_id, (x, y) => new SubsidyEdit
            {
                s_id = id,
                id_id = x.s_id_id,
                EmpCount = x.s_empcount,
                Money = x.s_money,
                Date = x.s_date_time,
                ApplicationFile = x.s_application,
                ApplicationName = x.s_application_name,
                LaborFile = x.s_insur_member,
                LaborName = x.s_insur_member_name,
                ApplicantsListFile = x.s_applicants,
                ApplicantsListName = x.s_applicants_name,
                AffidavitFile = x.s_affidavit,
                AffidavitName = x.s_affidavit_name,
                ReceiptFile = x.s_receipt,
                ReceiptName = x.s_receipt_name,
                EmployeeListFile = x.s_emp_lst,
                EmployeeListName = x.s_emp_lst_name,
                OtherFileFile = x.s_else,
                OtherFileName = x.s_else_name,
                id_name = y.id_name,
                s_no = x.s_no
            }).FirstOrDefault();

            return View(data);
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

            data.s_review_fst = "審核中";

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult InsurList(int? page = 1)
        {
            var id = (int)Session["UserID"];

            //var data = db.subsidy.Where(x => x.s_id_id == id).OrderByDescending(x => x.s_date_time).ToPagedList((int)page, 10);

            var data = db.subsidy.Where(x => x.s_id_id == id).Join(db.industry, x => x.s_id_id, y => y.id_id, (x, y) => new SubsidyEdit
            {
                //s_id = id,
                id_id = x.s_id_id,
                //EmpCount = x.s_empcount,
                //Money = x.s_money,
                Date = x.s_date_time,
                //ApplicationFile = x.s_application,
                //ApplicationName = x.s_application_name,
                LaborFile = x.s_insur_member,
                LaborName = x.s_insur_member_name,
                //ApplicantsListFile = x.s_applicants,
                //ApplicantsListName = x.s_applicants_name,
                //AffidavitFile = x.s_affidavit,
                //AffidavitName = x.s_affidavit_name,
                //ReceiptFile = x.s_receipt,
                //ReceiptName = x.s_receipt_name,
                //EmployeeListFile = x.s_emp_lst,
                //EmployeeListName = x.s_emp_lst_name,
                //OtherFileFile = x.s_else,
                //OtherFileName = x.s_else_name,
                id_name = y.id_name,
                s_no = x.s_no
            }).OrderByDescending(x => x.Date).ToPagedList((int)page, 10);

            return View(data);
        }

        public ActionResult ReviewList(int subsidyID, int? page)
        {
            var data = new Subsidy_ReviewIndex();
            int pageNum = page ?? 1;

            data.Subsidy = db.subsidy.Find(subsidyID);
            data.Subsidy_ReviewList = db.subsidy_review
                .Where(x => x.sr_s_id == subsidyID)
                .OrderByDescending(x => x.sr_date)
                .ThenByDescending(x => x.sr_time)
                .ToPagedList(pageNum, 10);

            return View(data);
        }
    }
}