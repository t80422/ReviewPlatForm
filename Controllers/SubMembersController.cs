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
                    mb_income_certificate_name = joined.Member.mb_income_certificate_name,
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
            //string path = Server.MapPath("~/assets/upload/Members");

            var data = db.subsidy_member.Where(a => a.sm_id == id).Join(db.member, a => a.sm_mb_id, b => b.mb_id, (a, b) => new SubMembersEdit
            {
                sm_agree_start = a.sm_agree_start,
                sm_agree_end = a.sm_agree_end,
                sm_advance_money = a.sm_advance_money,
                sm_mb_id = a.sm_mb_id,
                sm_id = id,
                sm_s_id = (int)a.sm_s_id,
                mb_name = b.mb_name,
                mb_id_card = b.mb_id_card,
                mb_birthday = b.mb_birthday,
                mb_insur_salary = (int)b.mb_insur_salary,
                mb_add_insur = b.mb_add_insur,
                mb_add_insur_date = b.mb_add_insur_date,
                mb_surrender_date = b.mb_surrender_date,
                mb_memo = b.mb_memo,
                mb_last_time = (DateTime)b.mb_last_time,
                mb_contractFile = b.mb_contract,
                mb_contract_name = b.mb_contract_name,
                sm_income_certificateFile = a.sm_income_certificate,
                sm_income_certificate_name = a.sm_income_certificate_name,
                mb_insurance_id = b.mb_insurance_id,
                mb_full_time_date = b.mb_full_time_date,
                mb_full_time_or_not = (bool)b.mb_full_time_or_not,
                mb_arrive_date = (DateTime)b.mb_arrive_date,
                mb_position = b.mb_position,
            }).FirstOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }

            switch (int.Parse("0" + data.mb_add_insur))
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
                    ViewBag.mb_add_insur = "加保";
                    break;
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubMembersEdit data)
        {
            if (ModelState.IsValid)
            {
                #region 必填

                var reloadMember = db.member.Find(data.sm_mb_id);
                var reloadSubMember = db.subsidy_member.Find(data.sm_id);

                if (string.IsNullOrEmpty(reloadMember.mb_contract) && data.mb_contract == null)
                {
                    switch (int.Parse("0" + data.mb_add_insur))
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
                            ViewBag.mb_add_insur = "加保";
                            break;
                    }

                    data.mb_contract_name = reloadMember.mb_contract_name;
                    data.mb_contractFile = reloadMember.mb_contract;
                    data.sm_income_certificate_name = reloadSubMember.sm_income_certificate_name;
                    data.sm_income_certificateFile = reloadSubMember.sm_income_certificate;

                    Session["msg"] = "請上傳勞動契約";

                    return View(data);
                }

                if (string.IsNullOrEmpty(data.sm_income_certificateFile) && data.sm_income_certificate == null)
                {
                    switch (int.Parse("0" + data.mb_add_insur))
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
                            ViewBag.mb_add_insur = "加保";
                            break;
                    }

                    data.mb_contract_name = reloadMember.mb_contract_name;
                    data.mb_contractFile = reloadMember.mb_contract;
                    data.sm_income_certificate_name = reloadSubMember.sm_income_certificate_name;
                    data.sm_income_certificateFile = reloadSubMember.sm_income_certificate;

                    Session["msg"] = "請上傳薪資證明";
                    return View(data);
                }

                #endregion

                var updateData = db.subsidy_member.Find(data.sm_id);
                var updateMember = db.member.Find(data.sm_mb_id);

                if (updateData != null && updateMember != null)
                {
                    updateData.sm_agree_start = data.sm_agree_start;
                    updateData.sm_agree_end = data.sm_agree_end;
                    updateData.sm_advance_money = data.sm_advance_money;

                    string path = Server.MapPath("~/assets/upload/Members");

                    var contract = ajax.UploadFile(data.mb_contract, path);
                    var incom = ajax.UploadFile(data.sm_income_certificate, path);

                    if (!string.IsNullOrEmpty(contract)) ajax.DeleteFile($"{path}/{updateMember.mb_contract}");
                    if (!string.IsNullOrEmpty(incom)) ajax.DeleteFile($"{path}/{updateData.sm_income_certificate}");

                    updateMember.mb_contract = contract ?? updateMember.mb_contract;
                    updateMember.mb_contract_name = data.mb_contract != null ? data.mb_contract.FileName : updateMember.mb_contract_name;
                    updateData.sm_income_certificate = incom ?? updateData.sm_income_certificate;
                    updateData.sm_income_certificate_name = data.sm_income_certificate != null ? data.sm_income_certificate.FileName : updateData.sm_income_certificate_name;

                    db.SaveChanges();
                    Session["msg"] = "修改成功";
                    var industryID = db.subsidy.Find(data.sm_s_id).s_id;
                    return RedirectToAction("Index", new { id = industryID });
                }
            }
            return View(data);
        }

        public ActionResult Edit_Manager(int subMemberID)
        {
            var data = GetSubsidyMemberData(subMemberID);

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Manager(SubsidyMemberData data)
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
                updateSubMember.sm_agree_start = data.Subsidy_Member.sm_agree_start;
                updateSubMember.sm_agree_end = data.Subsidy_Member.sm_agree_end;
                updateSubMember.sm_advance_money = data.Subsidy_Member.sm_advance_money;
                updateSubMember.sm_review = data.Subsidy_Member.sm_review;
                updateSubMember.sm_approved_amount = data.Subsidy_Member.sm_approved_amount;

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

            return View(GetSubsidyMemberData(data.Subsidy_Member.sm_id));
        }

        private SubsidyMemberData GetSubsidyMemberData(int subMemberID)
        {
            var data = new SubsidyMemberData();

            data.Subsidy_Member = db.subsidy_member.Find(subMemberID);
            data.Member = db.member.Find(data.Subsidy_Member.sm_mb_id);
            data.InitialReviewer = db.manager.Find(data.Subsidy_Member.sm_mg_id_fst);
            data.SecondaryReviewer = db.manager.Find(data.Subsidy_Member.sm_mg_id_snd);
            data.AssociationReviewer = db.manager.Find(data.Subsidy_Member.sm_mg_id_association);

            //計算在職月份
            var lastData = db.employment_insurance
                .Where(x => x.ei_id_card == data.Member.mb_id_card)
                .OrderByDescending(x => x.ei_year)
                .ThenByDescending(x => x.ei_month)
                .FirstOrDefault();

            if (lastData != null)
            {
                var lastChangeDate = lastData.ei_last_change_date;

                ROCDateToACDate(lastChangeDate, out DateTime acChangeDate);

                //公式 異動日期+已匯入總月份-1天,比對最近日期要大於6個月
                int enterMonth = lastData.ei_enter_month ?? 0;

                if (enterMonth == 0)
                {
                    data.Subsidy_Member.sm_qualifications= false;
                    data.Subsidy_Member.sm_calculation = 0;
                }
                else
                {
                    var diffMonth = GetMonthDiff(acChangeDate.AddMonths(enterMonth).AddDays(-1));

                    data.Subsidy_Member.sm_qualifications = diffMonth > 6;
                    data.Subsidy_Member.sm_calculation = (bool)data.Subsidy_Member.sm_qualifications ? diffMonth * 5000 : 0;
                }
            }

            return data;
        }

        public ActionResult Detail(int id)
        {
            var data = db.subsidy_member.Where(a => a.sm_id == id).Join(db.member, a => a.sm_mb_id, b => b.mb_id, (a, b) => new SubMembersEdit
            {
                sm_agree_start = a.sm_agree_start,
                sm_agree_end = a.sm_agree_end,
                sm_advance_money = a.sm_advance_money,
                sm_mb_id = a.sm_mb_id,
                sm_id = id,
                sm_s_id = (int)a.sm_s_id,
                mb_name = b.mb_name,
                mb_id_card = b.mb_id_card,
                mb_birthday = b.mb_birthday,
                mb_insur_salary = (int)b.mb_insur_salary,
                mb_add_insur = b.mb_add_insur,
                mb_add_insur_date = b.mb_add_insur_date,
                mb_surrender_date = b.mb_surrender_date,
                mb_memo = b.mb_memo,
                mb_last_time = (DateTime)b.mb_last_time,
                mb_contract_name = b.mb_contract_name,
                mb_insurance_id = b.mb_insurance_id,
                mb_full_time_date = b.mb_full_time_date,
                mb_income_certificate_name = b.mb_income_certificate_name,
                mb_full_time_or_not = (bool)b.mb_full_time_or_not,
                mb_arrive_date = (DateTime)b.mb_arrive_date,
                mb_position = b.mb_position,
                mb_contractFile = b.mb_contract,
                mb_income_certificateFile = b.mb_income_certificate,
            }).FirstOrDefault();

            return View(data);
        }

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return HttpNotFound();
            }

            var data = db.subsidy_member.Find(id);
            var subID = data.sm_s_id;
            if (data == null)
            {
                return HttpNotFound();
            }

            db.subsidy_member.Remove(data);
            db.SaveChanges();
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

        public ActionResult ReviewList(int subMemberID, int? page)
        {
            var data = new SubsidyMemberData();
            int pageNum = page ?? 1;

            data.Subsidy_Member = db.subsidy_member.Find(subMemberID);
            data.Subsidy = db.subsidy.Find(data.Subsidy_Member.sm_s_id);
            data.Member = db.member.Find(data.Subsidy_Member.sm_mb_id);
            data.ReviewList = db.subsidy_member_review
                .Where(x => x.smr_sm_id == subMemberID)
                .OrderByDescending(x => x.smr_date)
                .ThenByDescending(x => x.smr_time)
                .ToPagedList(pageNum, 10);

            return View(data);
        }

        /// <summary>
        /// 民國日期轉換西元日期
        /// </summary>
        /// <param name="rocDate">格式:yyyMMdd</param>
        /// <param name="acDate"></param>
        /// <returns></returns>
        private static bool ROCDateToACDate(string rocDate, out DateTime acDate)
        {
            if (rocDate.Length != 7)
            {
                acDate = default;
                return false;
            }

            //分解日期
            int rocYear, month, day;

            if (!int.TryParse(rocDate.Substring(0, 3), out rocYear) ||
                !int.TryParse(rocDate.Substring(3, 2), out month) ||
                !int.TryParse(rocDate.Substring(5, 2), out day))
            {
                acDate = default;
                return false;
            }

            //民國年轉換西元年
            int acYear = rocYear + 1911;

            try
            {
                acDate = new DateTime(acYear, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                acDate = default;
                return false;
            }
        }

        private static int GetMonthDiff(DateTime date)
        {
            var today = DateTime.Today;
            return (today.Year - date.Year) * 12 + today.Month - date.Month;
        }
    }
}