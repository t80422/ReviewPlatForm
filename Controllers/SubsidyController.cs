using DocumentFormat.OpenXml.EMMA;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            //檢查業者基本資料是否審核通過的flag
            ViewBag.Tip = false;

            //撈出所有資料
            var subsidies = db.subsidy.Join(
                db.industry,
                s => s.s_id_id,
                i => i.id_id,
                (s, i) => new SubsidyViewModel.SubsidyDetails
                {
                    Subsidy=s,
                    Industry=i,
                });

            //權限控制
            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            int perm = Session["perm"] != null ? (int)Session["perm"] : -1;

            switch (perm)
            {
                case 3:
                    subsidies = subsidies.Where(s => s.Industry.id_id == userID);

                    ViewBag.Tip = true;
                    break;
                case 2:
                    subsidies = subsidies.Where(s => s.Subsidy.s_division_case == userID);
                    break;
            }

            //搜尋功能=====
            //關鍵字搜尋
            if (!string.IsNullOrWhiteSpace(key) || !string.IsNullOrWhiteSpace(review))
            {
                subsidies = subsidies.Where(x => x.Subsidy.s_no.Contains(key) || x.Industry.id_name.Contains(key));
            }

            //審核狀態搜尋
            if (!string.IsNullOrWhiteSpace(review))
            {
                subsidies = subsidies.Where(x => x.Subsidy.s_review_fst== review);
            }
            //=============

            var industry = db.industry.Find(userID);
            double room = industry?.id_room ?? 0;

            //計算可申請人數
            if (perm == 3)
            {
                ViewBag.IndustryId = userID;

                ViewBag.SubMemberCount = GetApplicationSuccessful(userID);
                ViewBag.Remaining = Utility.GetEligibleApplicantCount(industry?.id_room ?? 0) - ViewBag.SubMemberCount;
            }

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

            ViewBag.Reviewers = new SelectList(joinResult, "ID", "Name", string.Empty);

            var data = new SubsidyIndexViewModel()
            {
                Subsidies = subsidies.OrderByDescending(s => s.Subsidy.s_id).ToPagedList((int)page, 100),
                Reviewers = joinResult
            };

            return View(data);
        }

        public ActionResult ChangePerson(string id, int? person_id = 0)
        {
            id = id.Replace("Review", "");

            var updateSubsidy = db.subsidy.Find(Int32.Parse(id));
            updateSubsidy.s_division_case = person_id;
            updateSubsidy.s_review_fst = "審核中";

            var updateIndustry = db.industry.Find(updateSubsidy.s_id_id);
            updateIndustry.id_division_case = person_id;
            updateIndustry.id_review = "審核中";

            db.SaveChanges();
            return Json("分案完成!!", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(int id_id)
        {
            var model = new SubsidyViewModel.SubsidyDetails();
            model.Industry = db.industry.Find(id_id);
            model.Subsidy = new subsidy();

            if (model.Industry == null)
            {
                string errMsg = "No industry found for the given id: " + id_id;
                return View("Error", errMsg);
            }

            ViewBag.SubMemberCount = GetApplicationSuccessful(model.Industry.id_id);
            ViewBag.Remaining = Utility.GetEligibleApplicantCount(model.Industry?.id_room ?? 0) - ViewBag.SubMemberCount;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubsidyViewModel.SubsidyDetails data)
        {
            if (!ModelState.IsValid)
            {
                Session["msg"] = "資料錯誤";
                return View(data);
            }
            var insertData = new subsidy();

            #region 上傳檔案

            string path = Server.MapPath($"~/assets/upload/Subsidy/{data.Industry.id_name + "_" + data.Industry.id_id}");
            HandleFileUploads(data, insertData, path);

            #endregion 上傳檔案

            UpdateData(data, insertData);

            db.subsidy.Add(insertData);
            db.SaveChanges();

            int pkValue = insertData.s_id;
            insertData.s_no = "A" + pkValue.ToString("D5");
            db.SaveChanges();


            if (InsertMemberList(data.ApplicantsFile, data.Industry.id_id, insertData.s_no, insertData.s_id))
            {
                Session["msg"] = "本次案件申請案號成功,請按確定後,至下一步上傳申請補助人員相關資料";

                return RedirectToAction("Index", "SubMembers", new { id = pkValue });
            }
            else
            {
                db.subsidy.Remove(insertData);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(int subsidyID)
        {
            var model = new SubsidyViewModel.SubsidyDetails();

            model.Subsidy = db.subsidy.Find(subsidyID);
            model.Industry = db.industry.First(x => x.id_id == model.Subsidy.s_id_id);
            model.InitialReviewer = db.manager.Find(model.Subsidy.s_mg_id_fst);
            model.SecondaryReviewer = db.manager.Find(model.Subsidy.s_mg_id_snd);
            model.AssociationReviewer = db.manager.Find(model.Subsidy.s_mg_id_association);
            model.SubMemberList = db.subsidy_member.
                Where(x => x.sm_s_id == subsidyID).
                Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubsidyViewModel.SubMemberList
                {
                    Subsidy_Member = x,
                    Member = y
                }).ToList();

            model.TrialAmount = model.SubMemberList
                .Where(x => x.Member.mb_review == "通過")
                .Sum(x => x.Subsidy_Member.sm_advance_money.Value);

            int userID = Session["UserID"] != null ? (int)Session["UserID"] : 0;
            var industry = db.industry.Find(userID);

            ViewBag.SubMemberCount = GetApplicationSuccessful(model.Industry.id_id);
            ViewBag.Remaining = Utility.GetEligibleApplicantCount(model.Industry?.id_room ?? 0) - ViewBag.SubMemberCount;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubsidyViewModel.SubsidyDetails data)
        {
            if (!ModelState.IsValid)
            {
                data.SubMemberList = db.subsidy_member.
                    Where(x => x.sm_s_id == data.Subsidy.s_id).
                    Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubsidyViewModel.SubMemberList
                    {
                        Subsidy_Member = x,
                        Member = y
                    }).ToList();

                return View(data);
            }

            var updateSubsidy = db.subsidy.Find(data.Subsidy.s_id);

            string path = Server.MapPath($"~/assets/upload/Subsidy/{data.Industry.id_name + "_" + data.Industry.id_id}");
            HandleFileUploads(data, updateSubsidy, path);

            UpdateData(data, updateSubsidy);

            if (data.ApplicantsFile != null)
            {
                if (!InsertMemberList(data.ApplicantsFile, data.Industry.id_id, data.Subsidy.s_no, data.Subsidy.s_id))
                {
                    data.SubMemberList = db.subsidy_member.
                        Where(x => x.sm_s_id == data.Subsidy.s_id).
                        Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubsidyViewModel.SubMemberList
                        {
                            Subsidy_Member = x,
                            Member = y
                        }).ToList();

                    db.SaveChanges();

                    return RedirectToAction("Edit", new { subsidyID = data.Subsidy.s_id });
                }
            }

            db.SaveChanges();

            Session["msg"] = "存檔成功";

            return RedirectToAction("Index");
        }

        public ActionResult Edit_Manager(int subsidyID, bool isView, int title)
        {
            var model = new SubsidyViewModel.SubsidyDetails();

            model.Subsidy = db.subsidy.Find(subsidyID);
            model.Industry = db.industry.First(x => x.id_id == model.Subsidy.s_id_id);
            model.InitialReviewer = db.manager.Find(model.Subsidy.s_mg_id_fst);
            model.SecondaryReviewer = db.manager.Find(model.Subsidy.s_mg_id_snd);
            model.AssociationReviewer = db.manager.Find(model.Subsidy.s_mg_id_association);
            model.SubMemberList = db.subsidy_member.
                    Where(x => x.sm_s_id == subsidyID).
                    Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubsidyViewModel.SubMemberList
                    {
                        Subsidy_Member = x,
                        Member = y
                    }).ToList();

            model.TrialAmount = model.SubMemberList
                .Where(x => x.Subsidy_Member.sm_review == "審核完成")
                .Sum(x => x.Subsidy_Member.sm_advance_money.Value); ;

            ViewBag.SubMemberCount = GetApplicationSuccessful(model.Industry.id_id);
            ViewBag.Remaining = Utility.GetEligibleApplicantCount(model.Industry?.id_room ?? 0) - ViewBag.SubMemberCount;

            //判斷是否觀看模式
            if (isView) model.ViewMode = true;

            switch (title)
            {
                case 1:
                    ViewBag.Title = "首頁＞補助申請＞補助申請詳細資料";
                    break;

                case 2:
                    ViewBag.Title = "首頁＞案件查詢＞詳細資料";
                    break;

                default:
                    break;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Manager(SubsidyViewModel.SubsidyDetails data)
        {
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var updateSubsidy = db.subsidy.Find(data.Subsidy.s_id);

            string path = Server.MapPath($"~/assets/upload/Subsidy/{data.Industry.id_name + "_" + data.Industry.id_id}");
            HandleFileUploads(data, updateSubsidy, path);

            #region 更新補助申請 

            UpdateData(data, updateSubsidy);
            updateSubsidy.s_review_application = data.Subsidy.s_review_application;
            updateSubsidy.s_review_insur_member = data.Subsidy.s_review_insur_member;
            updateSubsidy.s_review_emp_lst = data.Subsidy.s_review_emp_lst;
            updateSubsidy.s_review_applicants = data.Subsidy.s_review_applicants;
            updateSubsidy.s_review_else = data.Subsidy.s_review_else;
            updateSubsidy.s_review_else2 = data.Subsidy.s_review_else2;
            updateSubsidy.s_review_else3 = data.Subsidy.s_review_else3;
            updateSubsidy.s_review_fst = data.Subsidy.s_review_fst;
            updateSubsidy.s_grant_date = data.Subsidy.s_grant_date;
            updateSubsidy.s_approved_amount = data.Subsidy.s_approved_amount;

            #endregion

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

            if (data.ApplicantsFile != null)
            {
                if (!InsertMemberList(data.ApplicantsFile, data.Industry.id_id, data.Subsidy.s_no, data.Subsidy.s_id))
                {
                    data.SubMemberList = db.subsidy_member.
                        Where(x => x.sm_s_id == data.Subsidy.s_id).
                        Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new SubsidyViewModel.SubMemberList
                        {
                            Subsidy_Member = x,
                            Member = y
                        }).ToList();

                    db.SaveChanges();

                    return RedirectToAction("Edit_Manager", new { subsidyID = data.Subsidy.s_id, isView = false, title = 1 });
                }
            }

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
            var data = db.subsidy.Where(x => x.s_id == id).Join(db.industry, x => x.s_id_id, y => y.id_id, (x, y) => new SubsidyViewModel.SubsidyDetails
            {
                Subsidy = x,
                Industry = y,
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

            db.subsidy_member.RemoveRange(db.subsidy_member.Where(x => x.sm_s_id == id));
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

        public ActionResult ReviewList(int subsidyID, int title, int? page = 1)
        {
            var data = new Subsidy_ReviewIndex();

            data.Subsidy = db.subsidy.Find(subsidyID);
            data.Subsidy_ReviewList = db.subsidy_review
                .Where(x => x.sr_s_id == subsidyID)
                .OrderByDescending(x => x.sr_date)
                .ThenByDescending(x => x.sr_time)
                .ToPagedList((int)page, 10);

            //Title設定
            switch (title)
            {
                case 1:
                    ViewBag.Title = "首頁＞補助申請>審核紀錄";
                    break;

                case 2:
                    ViewBag.Title = "首頁＞案件查詢>審核紀錄";
                    break;

                default:
                    break;
            }

            return View(data);
        }

        /// <summary>
        /// 更新人員清冊
        /// </summary>
        /// <param name="file"></param>
        /// <param name="industryID"></param>
        /// <param name="subsidyNumber"></param>
        /// <param name="subsidyID"></param>
        private bool InsertMemberList(HttpPostedFileBase file, int industryID, string subsidyNumber, int subsidyID)
        {
            try
            {
                var AppList = ajax.ReadFile(file);

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
                                Session["msg"] = $"第{i}列的聘用類別輸入錯誤";
                                return false;
                        }

                        string IDCard = sheet.Value[i][3] ?? "";

                        if (IDCard != "")
                        {
                            member memberData = db.member.FirstOrDefault(x => x.mb_id_card == IDCard && x.mb_id_id == industryID) ?? new member();

                            memberData.mb_id = memberData.mb_id > 0 ? memberData.mb_id : 0;
                            memberData.mb_id_id = industryID;

                            var insurID = sheet.Value[i][0];
                            if (string.IsNullOrEmpty(insurID))
                            {
                                Session["msg"] = $"第{i}列的勞保序號不能空白";
                                return false;
                            }
                            memberData.mb_insurance_id = insurID;

                            var name = sheet.Value[i][1];
                            if (string.IsNullOrEmpty(name))
                            {
                                Session["msg"] = $"第{i}列的姓名不能空白";
                                return false;
                            }
                            memberData.mb_name = name;

                            var birthday = sheet.Value[i][2];

                            if (string.IsNullOrEmpty(birthday) || birthday.Length != 7)
                            {
                                Session["msg"] = $"第{i}列的生日格式錯誤 格式:年年年月月日日";
                                return false;
                            }

                            memberData.mb_birthday = birthday;

                            var idCard = sheet.Value[i][3];

                            if (string.IsNullOrEmpty(idCard))
                            {
                                Session["msg"] = $"第{i}列的身份證號碼不能空白";
                                return false;
                            }
                            memberData.mb_id_card = idCard;

                            var insurSalary = sheet.Value[i][4];

                            if (string.IsNullOrEmpty(insurSalary))
                            {
                                Session["msg"] = $"第{i}列的投保薪資不能空白";
                                return false;
                            }
                            memberData.mb_insur_salary = Convert.ToInt32(insurSalary);

                            if (string.IsNullOrEmpty(sheet.Value[i][5]) || !CheckExcelDate(sheet.Value[i][5], out DateTime? d))
                            {
                                Session["msg"] = $"第{i}列的加保日期格式錯誤 格式:年年年年/月月/日日";
                                return false;
                            }
                            memberData.mb_add_insur_date = d;

                            d = null;

                            var surrenderDate = sheet.Value[i][6];
                            if (!string.IsNullOrEmpty(surrenderDate) && !CheckExcelDate(surrenderDate, out d))
                            {
                                Session["msg"] = $"第{i}列的退保日期格式錯誤 格式:年年年年/月月/日日";
                                return false;
                            }
                            memberData.mb_surrender_date = d;

                            var fullTimeOrNot = sheet.Value[i][7];
                            if (string.IsNullOrEmpty(fullTimeOrNot))
                            {
                                Session["msg"] = $"第{i}列的是否到職轉正不能空白";
                                return false;
                            }
                            memberData.mb_full_time_or_not = fullTimeOrNot == "是";

                            var fullTimeDate = sheet.Value[i][8];
                            if (string.IsNullOrEmpty(fullTimeDate) || !CheckExcelDate(fullTimeDate, out d))
                            {
                                Session["msg"] = $"第{i}列的到職轉正日期格式錯誤 格式:年年年年/月月/日日";
                                return false;
                            }
                            memberData.mb_full_time_date = d;

                            var arriveDate = sheet.Value[i][9];
                            if (string.IsNullOrEmpty(arriveDate) || !CheckExcelDate(arriveDate, out d))
                            {
                                Session["msg"] = $"第{i}列的到職日期格式錯誤 格式:年年年年/月月/日日";
                                return false;
                            }
                            memberData.mb_arrive_date = d;

                            memberData.mb_hire_type = hire;

                            var position = sheet.Value[i][11];
                            if (string.IsNullOrEmpty(position))
                            {
                                Session["msg"] = $"第{i}列的職位不能空白";
                                return false;
                            }
                            memberData.mb_position = position;

                            memberData.mb_last_time = DateTime.Now;
                            memberData.mb_s_no = subsidyNumber;

                            if (memberData.mb_id <= 0)
                            {
                                db.member.Add(memberData);
                            }

                            db.SaveChanges();

                            subsidy_member subMemberData = db.subsidy_member
                                .Where(x => x.sm_s_id.HasValue && x.sm_s_id.Value == subsidyID
                                         && x.sm_mb_id.HasValue && x.sm_mb_id.Value == memberData.mb_id)
                                .FirstOrDefault();

                            if (subMemberData != null)
                            {
                                db.subsidy_member.Remove(subMemberData);
                            }

                            var subMember = new subsidy_member();

                            subMember.sm_s_id = subsidyID;

                            var agreeStart = sheet.Value[i][12];
                            if (string.IsNullOrEmpty(agreeStart) || !CheckExcelDate(agreeStart, out d))
                            {
                                Session["msg"] = $"第{i}列的符合申請(起)格式錯誤 格式:年年年年/月月/日日";
                                return false;
                            }
                            subMember.sm_agree_start = d;

                            var agreeEnd = sheet.Value[i][13];
                            if (string.IsNullOrEmpty(agreeEnd) || !CheckExcelDate(agreeEnd, out d))
                            {
                                Session["msg"] = $"第{i}列的符合申請(迄)格式錯誤 格式:年年年年/月月/日日";
                                return false;
                            }
                            subMember.sm_agree_end = d;

                            var advanceMoney = sheet.Value[i][14];
                            if (string.IsNullOrEmpty(advanceMoney))
                            {
                                Session["msg"] = $"第{i}列的申請金額不能空白";
                                return false;
                            }
                            subMember.sm_advance_money = Convert.ToInt32(advanceMoney);

                            subMember.sm_id_id = industryID;
                            subMember.sm_mb_id = memberData.mb_id;
                            subMember.sm_review = "待補件";

                            insertSMembers.Add(subMember);
                        }
                    }
                }
                db.subsidy_member.AddRange(insertSMembers);
                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                Session["msg"] = "文件格式有誤,請檢查是否選擇正確文件";
                return false;
            }
        }

        private bool CheckExcelDate(string excelDate, out DateTime? date)
        {
            try
            {
                date = DateTime.FromOADate(Convert.ToDouble(excelDate));
            }
            catch (Exception)
            {
                date = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 取得已申請成功人數
        /// </summary>
        /// <param name="industryID"></param>
        /// <returns></returns>
        private int GetApplicationSuccessful(int industryID)
        {
            return db.subsidy_member.Where(
                                            x => x.sm_id_id == industryID &&
                                            x.sm_mb_id != 0 &&
                                            x.sm_review == "審核完成"
                                           )
                                    .Select(x => x.sm_mb_id)
                                    .Distinct()
                                    .Count();
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="data"></param>
        /// <param name="updateData"></param>
        /// <param name="path"></param>
        private void HandleFileUploads(SubsidyViewModel.SubsidyDetails data, subsidy updateData, string path)
        {
            updateData.s_application = Utility.UploadAndDeleteFile(data.ApplicationFile, data.Subsidy.s_application, path);
            updateData.s_application_name = data.ApplicationFile != null ? data.ApplicationFile.FileName : updateData.s_application_name;
            updateData.s_insur_member = Utility.UploadAndDeleteFile(data.InsurMemberFile, data.Subsidy.s_insur_member, path);
            updateData.s_insur_member_name = data.InsurMemberFile != null ? data.InsurMemberFile.FileName : updateData.s_insur_member_name;
            updateData.s_emp_lst = Utility.UploadAndDeleteFile(data.EmpListFile, data.Subsidy.s_emp_lst, path);
            updateData.s_emp_lst_name = data.EmpListFile != null ? data.EmpListFile.FileName : updateData.s_emp_lst_name;
            updateData.s_applicants = Utility.UploadAndDeleteFile(data.ApplicantsFile, data.Subsidy.s_applicants, path);
            updateData.s_applicants_name = data.ApplicantsFile != null ? data.ApplicantsFile.FileName : updateData.s_applicants_name;
            updateData.s_else = Utility.UploadAndDeleteFile(data.ElseOneFile, data.Subsidy.s_else, path);
            updateData.s_else_name = data.ElseOneFile != null ? data.ElseOneFile.FileName : updateData.s_else_name;
            updateData.s_else_two = Utility.UploadAndDeleteFile(data.ElseTwoFile, data.Subsidy.s_else_two, path);
            updateData.s_else_two_name = data.ElseTwoFile != null ? data.ElseTwoFile.FileName : updateData.s_else_two_name;
            updateData.s_else_three = Utility.UploadAndDeleteFile(data.ElseThreeFile, data.Subsidy.s_else_three, path);
            updateData.s_else_three_name = data.ElseThreeFile != null ? data.ElseThreeFile.FileName : updateData.s_else_three_name;
            updateData.s_employee_inventory = Utility.UploadAndDeleteFile(data.EmployeeInventoryFile, data.Subsidy.s_employee_inventory, path);
            updateData.s_employee_inventory_name = data.EmployeeInventoryFile != null ? data.EmployeeInventoryFile.FileName : updateData.s_employee_inventory_name;
        }

        private void UpdateData(SubsidyViewModel.SubsidyDetails data, subsidy updateData)
        {
            updateData.s_money = data.Subsidy.s_money;
            updateData.s_empcount = data.Subsidy.s_empcount;
            updateData.s_memo = data.Subsidy.s_memo;
            updateData.s_id_id = data.Industry.id_id;
        }
    }
}