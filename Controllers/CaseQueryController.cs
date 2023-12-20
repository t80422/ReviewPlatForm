using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using PagedList;
using System;
using System.Data.Entity.Core.Common.CommandTrees;
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

            #region 搜尋

            if (!string.IsNullOrWhiteSpace(key))
            {
                query = query.Where(x => x.s.s_no.Contains(key) || x.i.id_name.Contains(key));
            }

            //審核狀態
            if (!string.IsNullOrWhiteSpace(review))
            {
                query = query.Where(x => x.s.s_review_fst == review);
            }

            #endregion

            var list = query.OrderByDescending(x => x.s.s_id).ToList();
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

        public ActionResult RequestPaymentList(DateTime? startDay, DateTime? endDay, int? page = 1)
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

            var list = data.OrderByDescending(x => x.s.s_id).ToList();
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

        //public ActionResult CaseStatistics(string review, DateTime? startDay, DateTime? endDay, int? page = 1)
        //{

        //    //var datas = db.industry.Join(db.subsidy, i => i.id_id, s => s.s_id_id, (i, s) => new { i, s })
        //    //                       .Join(db.subsidy_member, joined => joined.s.s_id, sm => sm.sm_s_id, (joined, sm) => new { joined.i, joined.s, sm });
        //    int index = 1;

        //    foreach (var item in db.industry)
        //    {
        //        var data = new CaseQueryViewModel.CaseStatisticsData();
        //        data.Id = index;
        //        index++;
        //        data.IndustryName = item.id_name;
        //        data.EligibleApplicants = Utility.GetEligibleApplicantCount(item.id_room.Value);

        //        var subsidies = db.subsidy.Where(x => x.s_id_id == item.id_id);

        //        #region 搜尋

        //        //審核狀態
        //        if (!string.IsNullOrWhiteSpace(review))
        //            subsidies = subsidies.Where(x => x.s_review_fst == review);

        //        //申請日期
        //        if (startDay.HasValue && endDay.HasValue)
        //        {
        //            subsidies = subsidies.Where(x => x.s_date_time >= startDay.Value && x.s_date_time <= endDay.Value);
        //        }
        //        else if (startDay.HasValue)
        //        {
        //            subsidies = subsidies.Where(x => x.s_date_time >= startDay.Value);
        //        }
        //        else if (endDay.HasValue)
        //        {
        //            subsidies = subsidies.Where(x => x.s_date_time <= endDay.Value);
        //        }

        //        #endregion

        //        var subMembers = subsidies.Join(db.subsidy_member, s => s.s_id, sm => sm.sm_s_id, (s, sm) => new { s, sm })
        //                                  .Join(db.member, joined => joined.sm.sm_mb_id, m => m.mb_id, (joined, m) => new { joined.s, joined.sm, m });
        //        data.CurrentApplicants = subMembers.Select(x => x.m.mb_id).Distinct().Count();

        //        var members = subMembers.GroupBy(x => x.m.mb_id_card);

        //        foreach(var group in members)
        //        {
        //            var idCard = group.Key;
        //            int count = 0;

        //            foreach (var g in group)
        //            {
        //                count += db.employment_insurance.Where(x => x.ei_id_card == idCard && x.ei_subsidy_no == g.s.s_no).Count();
        //            }

        //            switch (count)
        //            {
        //                case 6:
        //                    data.SixMonth += 1;
        //                    break;
        //                case 7:
        //                    data.SevenMonth += 1;
        //                    break;
        //                case 8:
        //                    data.EightMonth += 1;
        //                    break;
        //                case 9:
        //                    data.NightMonth += 1;
        //                    break;
        //                case 10:
        //                    data.TenMonth += 1;
        //                    break;
        //                case 11:
        //                    data.ElevenMonth += 1;
        //                    break;
        //                case 12:
        //                    data.TwelveMonth += 1;
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
                
        //            data.ApplicationAmount
                

        //    }

        //    return View();
        //}
    }
}