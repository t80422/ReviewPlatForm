using DocumentFormat.OpenXml.Office2010.Excel;
using PagedList;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SubMembersController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        // GET: SubMembers
        public ActionResult Index(int? id, int? page = 1)
        {
            var query = db.subsidy_member.Join(db.member, x => x.sm_mb_id, y => y.mb_id, (x, y) => new { SubMember = x, Member = y }).
                Join(db.subsidy, xy => xy.SubMember.sm_s_id, z => z.s_id, (xy, z) => new SubMembersEdit
                {
                    mb_name = xy.Member.mb_name,
                    sm_advance_money = xy.SubMember.sm_advance_money,
                    sm_agree_start = xy.SubMember.sm_agree_start,
                    sm_agree_end = xy.SubMember.sm_agree_end,
                    sm_review = xy.SubMember.sm_review,
                    sm_s_id = (int)xy.SubMember.sm_s_id,
                    sm_id = xy.SubMember.sm_id,
                    SubsidyNo = z.s_no
                });

            if (id.HasValue && id.Value != 0)
            {
                query = query.Where(z => z.sm_s_id == id.Value);
                ViewBag.SubsityID = id.Value;
            }

            query = query.OrderBy(x => x.sm_agree_start);
            var result = query.ToPagedList((int)page, 10);

            ViewBag.SubNo = db.subsidy.Find(id).s_no;

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
                var insertData = new subsidy_member()
                {
                    sm_s_id = data.sm_s_id,
                    sm_agree_start = data.sm_agree_start,
                    sm_agree_end = data.sm_agree_end,
                    sm_advance_money = data.sm_advance_money,
                    sm_mb_id = data.sm_mb_id,
                    sm_review = "審核中"
                };

                db.subsidy_member.Add(insertData);
                db.SaveChanges();

                Session["msg"] = "新增成功";

                return RedirectToAction("Index", new { id = data.sm_s_id });
            }
            return View(data);
        }

        public ActionResult Edit(int id)
        {
            var data = db.subsidy_member.Join(db.member, a => a.sm_mb_id, b => b.mb_id, (a, b) => new SubMembersEdit
            {
                sm_agree_start = a.sm_agree_start,
                sm_agree_end = a.sm_agree_end,
                sm_advance_money = a.sm_advance_money,
                sm_mb_id = a.sm_mb_id,
                sm_id = id,
                mb_name = b.mb_name,
                sm_s_id=(int)a.sm_s_id
            }).FirstOrDefault(z => z.sm_id == id);

            //Debug.Print(data.sm_mb_id.ToString());
            //int userID = (int)Session["UserID"];
            //var members = db.member.Where(x => x.mb_id_id == userID).ToList();
            //ViewBag.Members = new SelectList(members, "mb_id", "mb_name");

            //return View(data);

            //var query = db.subsidy_member.Find(id);

            //var data = new SubMembersEdit
            //{
            //    sm_agree_start = query.sm_agree_start,
            //    sm_agree_end = query.sm_agree_end,
            //    sm_advance_money = query.sm_advance_money,
            //    sm_mb_id = query.sm_mb_id,
            //    sm_id = id,
            //    mb_name=query
            //};

            if (data == null)
            {
                return HttpNotFound();
            }

            //// 取得所有的人員名稱
            //int userID = (int)Session["UserID"];
            //var members = db.member.Where(x => x.mb_id_id == userID).ToList();
            //ViewBag.Members = new SelectList(members, "mb_id", "mb_name", string.Empty); // 設定選定的值

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubMembersEdit data)
        {
            if (ModelState.IsValid)
            {
                var updateData = db.subsidy_member.Find(data.sm_id);

                if (updateData != null)
                {
                    updateData.sm_agree_start = data.sm_agree_start;
                    updateData.sm_agree_end = data.sm_agree_end;
                    updateData.sm_advance_money = data.sm_advance_money;

                    db.SaveChanges();
                    Session["msg"] = "修改成功";

                    return RedirectToAction("Index", new { id = data.sm_s_id });
                }
            }
            return View(data);
        }

        public ActionResult Detail()
        {
            return View();
        }

        public ActionResult Delete(int smID)
        {
            if (smID == 0)
            {
                return HttpNotFound();
            }

            var data = db.subsidy_member.Find(smID);
            var subID = data.sm_s_id;
            if (data == null)
            {
                return HttpNotFound();
            }

            db.subsidy_member.Remove(data);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = subID });
        }
    }
}