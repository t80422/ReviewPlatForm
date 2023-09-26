using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SubMembersController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();
        private AjaxsController ajax = new AjaxsController();

        // GET: SubMembers
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
                ViewBag.s_review = db.subsidy_member.Where(x=>x.sm_s_id==id && x.sm_review=="待補件").FirstOrDefault()?.sm_review;
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
            string path = Server.MapPath("~/assets/upload/Members");

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
                mb_contractFile=b.mb_contract,
                mb_contract_name = b.mb_contract_name,
                mb_income_certificateFile=b.mb_income_certificate,
                mb_income_certificate_name = b.mb_income_certificate_name,
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

            switch (int.Parse("0"+data.mb_add_insur))
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

                default :
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

                if (string.IsNullOrEmpty(reloadMember.mb_contract) && data.mb_contract==null)
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
                    data.mb_income_certificate_name = reloadMember.mb_income_certificate_name;
                    data.mb_income_certificateFile=reloadMember.mb_income_certificate;
                    
                    Session["msg"] = "請上傳勞動契約";
                    return View(data);
                }

                if (string.IsNullOrEmpty(data.mb_income_certificateFile) && data.mb_income_certificate == null )
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
                    data.mb_income_certificate_name = reloadMember.mb_income_certificate_name;
                    data.mb_income_certificateFile = reloadMember.mb_income_certificate;

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
                    var incom = ajax.UploadFile(data.mb_income_certificate, path);

                    if (!string.IsNullOrEmpty(contract)) ajax.DeleteFile($"{path}/{updateMember.mb_contract}");
                    if (!string.IsNullOrEmpty(incom)) ajax.DeleteFile($"{path}/{updateMember.mb_income_certificate}");

                    updateMember.mb_contract = contract ?? updateMember.mb_contract;
                    updateMember.mb_contract_name = data.mb_contract != null ? data.mb_contract.FileName : updateMember.mb_contract_name;
                    updateMember.mb_income_certificate = incom ?? updateMember.mb_income_certificate;
                    updateMember.mb_income_certificate_name = data.mb_income_certificate != null ? data.mb_income_certificate.FileName : updateMember.mb_income_certificate_name;

                    db.SaveChanges();
                    Session["msg"] = "修改成功";
                    var industryID = db.subsidy.Find(data.sm_s_id).s_id;
                    return RedirectToAction("Index", new { id = industryID });
                }
            }
            return View(data);
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
                mb_contractFile=b.mb_contract,
                mb_income_certificateFile=b.mb_income_certificate,                
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
            db.subsidy.Where(x => x.s_id == id).ToList().ForEach(x => x.s_review = "審核中");
            db.SaveChanges();
            db.subsidy_member.Where(x => x.sm_s_id == id).ToList().ForEach(x => x.sm_review = "審核中");
            db.SaveChanges();

            Session["msg"] = "本次案件送出成功";
            return RedirectToAction("Index", "Subsidy");
        }
    }
}