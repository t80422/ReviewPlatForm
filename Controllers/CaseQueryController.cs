using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class CaseQueryController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        public ActionResult Index(string key, string review, int? page = 1)
        {
            //取得所有案件列表
            var query = db.subsidy.Join(db.industry, s => s.s_id_id, i => i.id_id, (s, i) => new { s, i });

            //搜尋功能=====            
            //關鍵字
            if (!string.IsNullOrWhiteSpace(key))
            {
                query = query.Where(x => x.s.s_no.Contains(key) || x.i.id_name.Contains(key));
            }

            //審核狀態
            if (!string.IsNullOrWhiteSpace(review))
            {
                query = query.Where(x => x.s.s_review_fst == review);
            }
            //=============

            var list = query.OrderBy(x => x.s.s_id).ToList();
            var data = list.Select(x => new CaseQueryViewModel.CaseQueryData
            {
                Subsidy = x.s,
                Industry = x.i,
                EligibleApplicants = Math.Max(1, (int)Math.Ceiling((x.i.id_room ?? 0) / 8.0)),
                CurrentApplicants = db.subsidy_member.Where(sm => sm.sm_s_id == x.s.s_id).Count(),
                ApprovedApplicants = db.subsidy_member.Where(sm => sm.sm_s_id == x.s.s_id && sm.sm_review == "審核完成").Count(),
            })
            .ToPagedList((int)page, 10);

            return View(new CaseQueryViewModel.Index
            {
                caseQueryList = data
            });
        }

        public ActionResult RequestPaymentList(DateTime? startDay, DateTime? endDay, int? page=1)
        {
            var data = db.subsidy.Join(db.industry, s => s.s_id_id, i => i.id_id, (s, i) => new { s, i });

            if (startDay.HasValue && endDay.HasValue)
            {
                data = data.Where(x => x.s.s_date_time >= startDay.Value && x.s.s_date_time <= endDay.Value);
            }
            else if (startDay.HasValue)
            {
                data = data.Where(x => x.s.s_date_time >= startDay.Value);
            }
            else if (endDay.HasValue)
            {
                data = data.Where(x => x.s.s_date_time <= endDay.Value);
            }

            var list = data.OrderBy(x => x.s.s_id).ToList();
            var model = list.Select(x => new CaseQueryViewModel.CaseQueryData
            {
                Subsidy = x.s,
                Industry = x.i,
                EligibleApplicants = Math.Max(1, (int)Math.Ceiling((x.i.id_room ?? 0) / 8.0)),
                CurrentApplicants = db.subsidy_member.Where(sm => sm.sm_s_id == x.s.s_id).Count(),
                ApprovedApplicants = db.subsidy_member.Where(sm => sm.sm_s_id == x.s.s_id && sm.sm_review == "審核完成").Count(),
            })
            .ToPagedList((int)page, 10);

            return View(new CaseQueryViewModel.Index
            {
                caseQueryList = model
            });
        }
    }
}