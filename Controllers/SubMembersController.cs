using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class SubMembersController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        public ActionResult Index(int? id, int? page = 1)
        {
            var userID = (int)Session["UserID"];

            var baseQuery = db.subsidy_member
                .Join(db.member, sm => sm.sm_mb_id, m => m.mb_id, (sm, m) => new { SubMember = sm, Member = m })
                .Join(db.subsidy, joined => joined.SubMember.sm_s_id, s => s.s_id, (joined, s) => new SubMembersEdit
                {
                    mb_name = joined.Member.mb_name,
                    sm_advance_money = joined.SubMember.sm_advance_money,
                    sm_agree_start = joined.SubMember.sm_agree_start,
                    sm_agree_end = joined.SubMember.sm_agree_end,
                    sm_review = joined.SubMember.sm_review,
                    sm_s_id = (int)joined.SubMember.sm_s_id,
                    sm_id = joined.SubMember.sm_id,
                    SubsidyNo = s.s_no,
                    //modify by v0.6
                    //mb_income_certificate_name = joined.Member.mb_income_certificate_name,
                    sm_income_certificate_name = joined.SubMember.sm_income_certificate_name,
                    //modify by v0.6
                    mb_contract_name = joined.Member.mb_contract_name,
                    sm_id_id = joined.SubMember.sm_id_id
                });

            var filterQuery = id.HasValue
                ? baseQuery.Where(z => z.sm_s_id == id)
                : baseQuery.Where(z => z.sm_id_id == userID);

            var query = filterQuery.OrderBy(x => x.sm_agree_start);

            ViewBag.SubsityID = id;

            var result = query.ToPagedList((int)page, 10);

            if (id != null)
            {
                ViewBag.SubNo = db.subsidy.Find(id).s_no;
                ViewBag.s_review = db.subsidy_member.Where(x => x.sm_s_id == id && x.sm_review == "待補件").FirstOrDefault()?.sm_review;
            }

            return View(result);
        }

        public ActionResult IndexQ(int? id, int? page = 1)
        {
            var userID = (int)Session["UserID"];

            var baseQuery = db.subsidy_member
                .Join(db.member, sm => sm.sm_mb_id, m => m.mb_id, (sm, m) => new { SubMember = sm, Member = m })
                .Join(db.subsidy, joined => joined.SubMember.sm_s_id, s => s.s_id, (joined, s) => new SubMembersEdit
                {
                    mb_name = joined.Member.mb_name,
                    sm_advance_money = joined.SubMember.sm_advance_money,
                    sm_agree_start = joined.SubMember.sm_agree_start,
                    sm_agree_end = joined.SubMember.sm_agree_end,
                    sm_review = joined.SubMember.sm_review,
                    sm_s_id = (int)joined.SubMember.sm_s_id,
                    sm_id = joined.SubMember.sm_id,
                    SubsidyNo = s.s_no,
                    mb_income_certificate_name = joined.SubMember.sm_income_certificate_name,
                    mb_contract_name = joined.Member.mb_contract_name,
                    sm_id_id = joined.SubMember.sm_id_id
                });

            var filterQuery = id.HasValue
                ? baseQuery.Where(z => z.sm_s_id == id)
                : baseQuery.Where(z => z.sm_id_id == userID);

            var query = filterQuery.OrderBy(x => x.sm_agree_start);


            ViewBag.SubsityID = id;

            var result = query.ToPagedList((int)page, 10);

            if (id != null)
            {
                ViewBag.SubNo = db.subsidy.Find(id).s_no;
            }

            return View(result);
        }

        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Subsidy");
            }

            var result = new SubMembersEdit
            {
                sm_s_id = (int)id
            };

            int userID = (int)Session["UserID"];
            var members = db.member.Where(x => x.mb_id_id == userID).ToList();
            ViewBag.Members = new SelectList(members, "mb_id", "mb_name");

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubMembersEdit data)
        {
            if (ModelState.IsValid)
            {
                var newdata = db.subsidy_member.Where(a => a.sm_mb_id == data.sm_mb_id & a.sm_s_id == data.sm_s_id).FirstOrDefault();

                if (newdata == null)
                {
                    var insertData = new subsidy_member()
                    {
                        sm_s_id = data.sm_s_id,
                        sm_agree_start = data.sm_agree_start,
                        sm_agree_end = data.sm_agree_end,
                        sm_advance_money = data.sm_advance_money,
                        sm_mb_id = data.sm_mb_id,
                        sm_review = "待補件"
                    };
                    db.subsidy_member.Add(insertData);
                    db.SaveChanges();

                    Session["msg"] = "新增成功";
                }
                else
                {
                    Session["msg"] = "人員已存在";
                }
                return RedirectToAction("Index", new { id = data.sm_s_id });
            }
            return View(data);
        }

        public ActionResult Edit(int id)
        {
            var model = db.subsidy_member.Where(sm => sm.sm_id == id).Join(db.member, sm => sm.sm_mb_id, mb => mb.mb_id, (sm, mb) => new SubsidyMemberViewModel
            {
                Subsidy_Member = sm,
                Member = mb
            }).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubsidyMemberViewModel data)
        {
            if (!ModelState.IsValid) return View(data);

            var updateData = db.subsidy_member.Find(data.Subsidy_Member.sm_id);
            var updateMember = db.member.Find(data.Subsidy_Member.sm_mb_id);

            if (updateData != null && updateMember != null)
            {
                updateData.sm_agree_start = data.Subsidy_Member.sm_agree_start;
                updateData.sm_agree_end = data.Subsidy_Member.sm_agree_end;
                updateData.sm_advance_money = data.Subsidy_Member.sm_advance_money;

                string path = Server.MapPath("~/assets/upload/Members");

                updateMember.mb_contract = Utility.UploadAndDeleteFile(data.ContractFile, data.Member.mb_contract, path);
                updateMember.mb_contract_name = data.ContractFile != null ? data.ContractFile.FileName : updateMember.mb_contract_name;
                updateData.sm_income_certificate = Utility.UploadAndDeleteFile(data.IncomeCertificateFile, data.Subsidy_Member.sm_income_certificate, path);
                updateData.sm_income_certificate_name = data.IncomeCertificateFile != null ? data.IncomeCertificateFile.FileName : updateData.sm_income_certificate_name;

                db.SaveChanges();
                Session["msg"] = "修改成功";

                return RedirectToAction("Index", new { id = data.Subsidy_Member.sm_s_id });
            }

            return View(data);
        }

        public ActionResult Edit_Manager(int subMemberID, bool viewMode)
        {
            var data = GetSubsidyMemberData(subMemberID, viewMode);
            data.FullTimeOrNotString = data.Member.mb_full_time_or_not.Value ? "1" : "0";
            var memberIdCard = db.member.Find(data.Subsidy_Member.sm_mb_id)?.mb_id_card;

            if (memberIdCard != null)
            {
                data.OtherCompany = db.member
                                     .Where(x => x.mb_id_card == memberIdCard)
                                     .Select(x => x.mb_id_id)
                                     .Distinct()
                                     .Count() > 1 ? "是" : "否";
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Manager(SubsidyMemberViewModel data)
        {
            if (ModelState.IsValid)
            {
                var updateSubMember = db.subsidy_member.Find(data.Subsidy_Member.sm_id);
                var updateMember = db.member.Find(data.Subsidy_Member.sm_mb_id);

                #region 上傳檔案

                string path = Server.MapPath("~/assets/upload/Members");

                var contract = ajax.UploadFile(data.ContractFile, path);
                var incom = ajax.UploadFile(data.IncomeCertificateFile, path);

                if (!string.IsNullOrEmpty(contract)) ajax.DeleteFile($"{path}/{updateMember.mb_contract}");
                if (!string.IsNullOrEmpty(incom)) ajax.DeleteFile($"{path}/{updateSubMember.sm_income_certificate}");

                updateMember.mb_contract = contract ?? updateMember.mb_contract;
                updateMember.mb_contract_name = data.ContractFile != null ? data.ContractFile.FileName : updateMember.mb_contract_name;
                updateSubMember.sm_income_certificate = incom ?? updateSubMember.sm_income_certificate;
                updateSubMember.sm_income_certificate_name =
                data.IncomeCertificateFile != null ? data.IncomeCertificateFile.FileName : updateSubMember.sm_income_certificate_name;

                #endregion

                #region 更新資料

                updateMember.mb_name = data.Member.mb_name;
                updateMember.mb_birthday = data.Member.mb_birthday;
                updateMember.mb_id_card = data.Member.mb_id_card;
                updateMember.mb_add_insur = data.Member.mb_add_insur;
                updateMember.mb_insur_salary = data.Member.mb_insur_salary;
                updateMember.mb_position = data.Member.mb_position;
                updateMember.mb_insurance_id = data.Member.mb_insurance_id;
                updateMember.mb_add_insur_date = data.Member.mb_add_insur_date;
                updateMember.mb_surrender_date = data.Member.mb_surrender_date;
                updateMember.mb_full_time_or_not = data.FullTimeOrNot;
                updateMember.mb_full_time_date = data.Member.mb_full_time_date;
                updateMember.mb_review_contract = data.Member.mb_review_contract;
                updateSubMember.sm_agree_start = data.Subsidy_Member.sm_agree_start;
                updateSubMember.sm_agree_end = data.Subsidy_Member.sm_agree_end;
                updateSubMember.sm_advance_money = data.Subsidy_Member.sm_advance_money;
                updateSubMember.sm_review = data.Subsidy_Member.sm_review;
                updateSubMember.sm_approved_amount = data.Subsidy_Member.sm_approved_amount;
                updateSubMember.sm_review_income_certificate = data.Subsidy_Member.sm_review_income_certificate;
                updateSubMember.sm_qualifications = data.Subsidy_Member.sm_qualifications;

                #endregion

                #region 更新審核人員

                var perm = (int)Session["perm"];
                var managerID = (int)Session["UserID"];

                switch (perm)
                {
                    case 0:
                        updateSubMember.sm_mg_id_association = managerID;
                        break;

                    case 1:
                        updateSubMember.sm_mg_id_snd = managerID;
                        break;

                    case 2:
                        updateSubMember.sm_mg_id_fst = managerID;
                        break;

                    default:
                        break;
                }

                #endregion

                #region 新增審核紀錄

                var insertReview = new subsidy_member_review();
                var reviewer = db.manager.Find(managerID).mg_name;

                insertReview.smr_sm_id = data.Subsidy_Member.sm_id;
                insertReview.smr_date = DateTime.Now.Date;
                insertReview.smr_time = DateTime.Now.TimeOfDay;
                insertReview.smr_reviewer = reviewer;
                insertReview.smr_approved_amount = data.Subsidy_Member.sm_approved_amount;
                insertReview.smr_review_contract = data.Member.mb_review_contract;
                insertReview.smr_review_income_certificate = data.Subsidy_Member.sm_review_income_certificate;
                insertReview.smr_reveiw_status = data.Subsidy_Member.sm_review;

                db.subsidy_member_review.Add(insertReview);

                #endregion

                db.SaveChanges();
                Session["msg"] = "存檔成功";
            }

            return RedirectToAction("Edit_Manager", new { subMemberID = data.Subsidy_Member.sm_id, viewMode = data.ViewMode });
        }

        public ActionResult Detail(int id)
        {
            var model = db.subsidy_member.Where(sm => sm.sm_id == id).Join(db.member, sm => sm.sm_mb_id, mb => mb.mb_id, (sm, mb) => new SubsidyMemberViewModel
            {
                Subsidy_Member = sm,
                Member = mb
            }).FirstOrDefault();

            return View(model);
        }

        public ActionResult Delete(int subMemberID, int subID, bool? fromSubsidy = false)
        {
            if (subMemberID == 0)
            {
                return HttpNotFound();
            }

            var data = db.subsidy_member.Find(subMemberID);

            if (data == null)
            {
                if ((bool)fromSubsidy)
                {
                    return RedirectToAction("Edit_Manager", "Subsidy", new { subsidyID = subID, isView = false, title = 1 });
                }
                return RedirectToAction("Index", new { id = subID });
            }

            db.subsidy_member.Remove(data);
            db.SaveChanges();

            if ((bool)fromSubsidy)
            {
                return RedirectToAction("Edit_Manager", "Subsidy", new { subsidyID = subID, isView = false, title = 1 });
            }
            return RedirectToAction("Index", new { id = subID });
        }

        public ActionResult AllCaseSubmit(int id)
        {
            db.subsidy.Where(x => x.s_id == id).ToList().ForEach(x => x.s_review_fst = "待審核");
            db.SaveChanges();
            db.subsidy_member.Where(x => x.sm_s_id == id).ToList().ForEach(x => x.sm_review = "待審核");
            db.SaveChanges();

            Session["msg"] = "本次案件送出成功";
            return RedirectToAction("Index", "Subsidy");
        }

        public ActionResult ReviewList(int subMemberID, int? page, string title)
        {
            var data = new SubsidyMemberViewModel();
            int pageNum = page ?? 1;

            data.Subsidy_Member = db.subsidy_member.Find(subMemberID);
            data.Subsidy = db.subsidy.Find(data.Subsidy_Member.sm_s_id);
            data.Member = db.member.Find(data.Subsidy_Member.sm_mb_id);
            data.ReviewList = db.subsidy_member_review
                .Where(x => x.smr_sm_id == subMemberID)
                .OrderByDescending(x => x.smr_date)
                .ThenByDescending(x => x.smr_time)
                .ToPagedList(pageNum, 10);

            ViewBag.Title = title + ">補助人員審核記錄";

            return View(data);
        }

        private SubsidyMemberViewModel GetSubsidyMemberData(int subMemberID, bool viewMode)
        {
            var data = new SubsidyMemberViewModel();

            data.Subsidy_Member = db.subsidy_member.Find(subMemberID);
            data.Member = db.member.Find(data.Subsidy_Member.sm_mb_id);
            data.Industry = db.industry.Find(data.Subsidy_Member.sm_id_id);
            data.InitialReviewer = db.manager.Find(data.Subsidy_Member.sm_mg_id_fst);
            data.SecondaryReviewer = db.manager.Find(data.Subsidy_Member.sm_mg_id_snd);
            data.AssociationReviewer = db.manager.Find(data.Subsidy_Member.sm_mg_id_association);
            data.MemberApplyList = db.member.Where(x => x.mb_id_card == data.Member.mb_id_card)
                  .Join(db.subsidy_member, mb => mb.mb_id, sm => sm.sm_mb_id, (mb, sm) => new { Member = mb, SubMember = sm })
                  .Join(db.subsidy, joined => joined.SubMember.sm_s_id, s => s.s_id, (joined, s) => new SubsidyMemberViewModel.MemberApply()
                  {
                      Member = joined.Member,
                      SubsidyNo = s.s_no,
                      Subsidy_Member = joined.SubMember
                  })
                  .ToList();

            //modify by 2次修改=====

            ////計算在職月份
            //var lastData = db.employment_insurance
            //    .Where(x => x.ei_id_card == data.Member.mb_id_card && x.ei_id_id == data.Subsidy_Member.sm_id_id)
            //    .OrderByDescending(x => x.ei_year)
            //    .ThenByDescending(x => x.ei_month)
            //    .FirstOrDefault();

            ////系統試算資格
            //if (lastData != null)
            //{
            //    var lastChangeDate = lastData.ei_last_change_date;

            //    ROCDateToACDate(lastChangeDate, out DateTime acChangeDate);

            //    var diffDate = acChangeDate.AddMonths(6);

            //    data.SystemQualifications = diffDate < DateTime.Today ? "是" : "否";
            //}
            //else
            //{
            //    data.SystemQualifications = "否";
            //}

            //======================

            data.SystemQualifications = EvaluateQualification(data);

            //======================

            //系統試算
            data.Subsidy_Member.sm_calculation = data.Subsidy_Member.sm_review == "審核完成" ? data.Subsidy_Member.sm_advance_money : 0;

            data.ViewMode = viewMode;

            return data;
        }

        private string EvaluateQualification(SubsidyMemberViewModel data)
        {
            var qualifyingPositions = new[] { "房務", "清潔" };
            var startDate = new DateTime(2023, 4, 1);
            var endDate = new DateTime(2024, 3, 31);
            int limitMoney = GetSalaryLimit(data.Industry.id_city);

            var eiRecords = db.employment_insurance
                .Where(x => x.ei_id_card == data.Member.mb_id_card && x.ei_id_id == data.Industry.id_id)
                .AsEnumerable()
                .Select(x => new
                {
                    x.ei_type,
                    x.ei_salary,
                    ChangeDate = DateTime.TryParse(x.ei_last_change_date, out DateTime parsedDate) ? parsedDate : (DateTime?)null,
                    Date = new DateTime(x.ei_year, x.ei_month, 1)
                })
                .Where(x => x.ChangeDate.HasValue && x.ChangeDate.Value >= startDate && x.ChangeDate.Value <= endDate)
                .Where(x => qualifyingPositions.Contains(x.ei_type) && x.ei_salary >= limitMoney)
                .ToList();

            if (!eiRecords.Any())
                return "否";

            var dates = eiRecords.Select(x => x.Date).OrderBy(x => x);
            return HasConsecutiveMonths(dates, 6) ? "是" : "否";
        }

        private int GetSalaryLimit(string city)
        {
            var highCostCities = new[] { "臺北市", "台北市", "新北市", "基隆市", "桃園市", "新竹縣", "新竹市" };
            return highCostCities.Contains(city) ? 33000 : 31000;
        }

        private bool HasConsecutiveMonths(IEnumerable<DateTime> dates, int requiredConsecutiveMonths)
        {
            var orderedDates = dates.OrderBy(x => x).ToList();
            int consecutiveMonths = 1;
            DateTime previousDate = orderedDates.First();

            for (int i = 1; i < orderedDates.Count; i++)
            {
                DateTime currentDate = orderedDates[i];
                if (currentDate.Year == previousDate.Year && currentDate.Month == previousDate.Month + 1 ||
                    currentDate.Year == previousDate.Year + 1 && currentDate.Month == 1 && previousDate.Month == 12)
                {
                    consecutiveMonths++;
                }
                else
                {
                    consecutiveMonths = currentDate.Month == 1 && previousDate.Month == 12 ? 2 : 1;
                }

                if (consecutiveMonths >= requiredConsecutiveMonths)
                    return true;

                previousDate = currentDate;
            }

            return false;
        }

        //父置chat
    }
}