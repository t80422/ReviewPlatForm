using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// 每月請款
        /// </summary>
        /// <param name="startDay"></param>
        /// <param name="endDay"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult RequestPaymentList(string review, DateTime? payStartDay, DateTime? payEndDay, DateTime? applyStartDay, DateTime? applyEndDay, bool? paied, int? page = 1)
        {
            //1.以案號為單位，顯示資料(號案狀態為審核完成才會被撈出來)
            var datas = from i in db.industry
                        join s in db.subsidy on i.id_id equals s.s_id_id
                        where s.s_review_fst == "審核完成"
                        join sm in db.subsidy_member on s.s_id equals sm.sm_s_id into smGroup
                        from sm in smGroup.DefaultIfEmpty()
                        join m in db.member on sm.sm_mb_id equals m.mb_id into mGroup
                        from m in mGroup.DefaultIfEmpty()
                        orderby s.s_id
                        select new { i, s, sm, m };

            #region 搜尋

            //審核狀態
            if (!string.IsNullOrWhiteSpace(review))
                datas = datas.Where(x => x.s.s_review_fst == review);

            //撥款日期
            if (payStartDay.HasValue && payEndDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time >= payStartDay.Value && x.s.s_date_time <= payEndDay.Value);
            }
            else if (payStartDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time >= payStartDay.Value);
            }
            else if (payEndDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time <= payEndDay.Value);
            }

            //申請日期
            if (applyStartDay.HasValue && applyEndDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time >= applyStartDay.Value && x.s.s_date_time <= applyEndDay.Value);
            }
            else if (applyStartDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time >= applyStartDay.Value);
            }
            else if (applyEndDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time <= applyEndDay.Value);
            }

            //付款狀況
            if (paied.HasValue)
            {
                if (paied.Value)
                {
                    datas = datas.Where(x => x.s.s_grant_date != null && x.s.s_grant_date <= DateTime.Now);
                }
                else
                {
                    datas = datas.Where(x => x.s.s_grant_date == null || x.s.s_grant_date >= DateTime.Now);
                }
            }

            #endregion

            int index = 1;
            var model = new CaseQueryViewModel.RequestPayment();
            var list = new List<CaseQueryViewModel.RequestPaymentData>();

            foreach (var sGroup in datas.GroupBy(x => x.s.s_id))
            {
                var data = new CaseQueryViewModel.RequestPaymentData();
                data.ID = index;
                index++;
                data.IndustryName = sGroup.First().i.id_name;
                data.EligibleApplicants = Utility.GetEligibleApplicantCount(sGroup.First().i.id_room);
                data.BankAcctName = sGroup.First().i.id_bank_acct_name;
                data.BankName = sGroup.First().i.id_bank_name;
                data.BankCode = sGroup.First().i.id_bank_code;
                data.BankAcct = sGroup.First().i.id_bank_acct;

                //4.申請人數 - 計算案號的申請補助人員(審核完成)
                data.CurrentApplicants = sGroup.Where(x => x.m != null && x.sm.sm_review == "審核完成") // 检查m不是null
                                               .Select(x => x.m.mb_id)
                                               .Distinct()
                                               .Count();

                data.ApplicationAmount = sGroup.Where(x => x.sm != null) // 检查sm不是null
                                               .Select(x => x.sm.sm_advance_money ?? 0)
                                               .Sum();
                data.GrantAmount = sGroup.Where(x => x.sm != null)
                                         .Select(x => x.sm.sm_approved_amount ?? 0)
                                         .Sum();

                //2.撥款狀況：已撥款(有填寫撥款日期且當天為撥款日期以後，含當日)
                var grantDate = sGroup.First().s.s_grant_date;
                data.PayStatus = grantDate != null && grantDate <= DateTime.Now ? "已撥款" : "未撥款";

                data.SubsidyNo = sGroup.First().s.s_no;

                foreach (var mGroup in sGroup.Where(x => x.m != null).GroupBy(x => x.m.mb_id_card))
                {
                    var idCard = mGroup.Key;
                    int count = 0;

                    foreach (var g in mGroup)
                    {
                        count += db.employment_insurance.Where(x => x.ei_id_card == idCard && x.ei_subsidy_no == g.s.s_no).Count();
                    }

                    switch (count)
                    {
                        case 1:
                            data.OneMonth += 1;
                            break;
                        case 2:
                            data.TwoMonth += 1;
                            break;
                        case 3:
                            data.ThreeMonth += 1;
                            break;
                        case 4:
                            data.FourMonth += 1;
                            break;
                        case 5:
                            data.FiveMonth += 1;
                            break;
                        case 6:
                            data.SixMonth += 1;
                            break;
                        case 7:
                            data.SevenMonth += 1;
                            break;
                        case 8:
                            data.EightMonth += 1;
                            break;
                        case 9:
                            data.NineMonth += 1;
                            break;
                        case 10:
                            data.TenMonth += 1;
                            break;
                        case 11:
                            data.ElevenMonth += 1;
                            break;
                        case 12:
                            data.TwelveMonth += 1;
                            break;
                        default:
                            break;
                    }
                }
                list.Add(data);
            }

            Session["RequestPaymentList"] = list;
            model.RequestPaymentList = list.ToPagedList(page.Value, 10);

            return View(model);
        }

        /// <summary>
        /// 案件統計
        /// </summary>
        /// <param name="review"></param>
        /// <param name="startDay"></param>
        /// <param name="endDay"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult CaseStatistics(string review, DateTime? startDay, DateTime? endDay, int? page = 1)
        {
            var datas = db.industry.Join(db.subsidy, i => i.id_id, s => s.s_id_id, (i, s) => new { i, s })
                                   .GroupJoin(db.subsidy_member, joined => joined.s.s_id, sm => sm.sm_s_id, (joined, sm) => new { joined.i, joined.s, sm })
                                   .SelectMany(joined => joined.sm.DefaultIfEmpty(), (joined, sm) => new { joined.i, joined.s, sm })
                                   .GroupJoin(db.member, joined => joined.sm.sm_mb_id, m => m.mb_id, (joined, m) => new { joined.i, joined.s, joined.sm, m })
                                   .SelectMany(joined => joined.m.DefaultIfEmpty(), (joined, m) => new { joined.i, joined.s, joined.sm, m })
                                   .Where(x => x.sm.sm_mb_id != 0);

            #region 搜尋

            //審核狀態
            if (!string.IsNullOrWhiteSpace(review))
                datas = datas.Where(x => x.s.s_review_fst == review);

            //申請日期
            if (startDay.HasValue && endDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time >= startDay.Value && x.s.s_date_time <= endDay.Value);
            }
            else if (startDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time >= startDay.Value);
            }
            else if (endDay.HasValue)
            {
                datas = datas.Where(x => x.s.s_date_time <= endDay.Value);
            }

            #endregion

            int index = 1;
            var model = new CaseQueryViewModel.CaseStatistics();
            var list = new List<CaseQueryViewModel.CaseStatisticsData>();

            foreach (var iGroup in datas.GroupBy(x => x.i.id_id))
            {
                var data = new CaseQueryViewModel.CaseStatisticsData();

                data.Id = index;
                index++;
                data.IndustryName = iGroup.First().i.id_name;
                data.EligibleApplicants = Utility.GetEligibleApplicantCount(iGroup.First().i.id_room);
                data.CurrentApplicants = iGroup.Where(x => x.m != null) // 检查m不是null
                   .Select(x => x.m.mb_id)
                   .Distinct()
                   .Count();
                data.ApplicationAmount = iGroup.Where(x => x.sm != null) // 检查sm不是null
                                   .Select(x => x.sm.sm_advance_money ?? 0)
                                   .Sum();
                data.GrantAmount = iGroup.Where(x => x.sm != null)
                                .Select(x => x.sm.sm_approved_amount ?? 0)
                                .Sum();

                string content = "";

                foreach (var sGroup in iGroup.GroupBy(x => x.s.s_id))
                {
                    if (sGroup.First().s.s_review_fst != "審核完成" && !string.IsNullOrEmpty(sGroup.First().s.s_memo))
                    {
                        content += "案號:" + sGroup.First().s.s_no + ":" + sGroup.First().s.s_memo + "\r\n";
                    }

                    foreach (var mGroup in sGroup.Where(x => x.m != null).GroupBy(x => x.m.mb_id_card))
                    {
                        var idCard = mGroup.Key;
                        int count = 0;

                        foreach (var g in mGroup)
                        {
                            count += db.employment_insurance.Where(x => x.ei_id_card == idCard && x.ei_subsidy_no == g.s.s_no).Count();
                        }

                        switch (count)
                        {
                            case 6:
                                data.SixMonth += 1;
                                break;
                            case 7:
                                data.SevenMonth += 1;
                                break;
                            case 8:
                                data.EightMonth += 1;
                                break;
                            case 9:
                                data.NineMonth += 1;
                                break;
                            case 10:
                                data.TenMonth += 1;
                                break;
                            case 11:
                                data.ElevenMonth += 1;
                                break;
                            case 12:
                                data.TwelveMonth += 1;
                                break;
                            default:
                                break;
                        }
                    }
                }
                data.Memo = content;
                list.Add(data);
            }

            Session["CaseStatistics"] = list;
            model.CaseStatisticsList = list.ToPagedList(page.Value, 10);

            return View(model);
        }

        /// <summary>
        /// 匯出案件統計
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportCaseStatistics()
        {
            string fileName = "案件統計.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            List<CaseQueryViewModel.CaseStatisticsData> model = (List<CaseQueryViewModel.CaseStatisticsData>)Session["CaseStatistics"];
            using (var memoryStream = new MemoryStream())
            {
                using (var xml = new OpenXmlExcel())
                {
                    //欄位
                    xml.WriteToCell(0, 0, "序號");
                    xml.WriteToCell(0, 1, "旅宿業名稱");
                    xml.WriteToCell(0, 2, "可申請人數");
                    xml.WriteToCell(0, 3, "已提出申請人數");
                    xml.WriteToCell(0, 4, "6個月");
                    xml.WriteToCell(0, 5, "7個月");
                    xml.WriteToCell(0, 6, "8個月");
                    xml.WriteToCell(0, 7, "9個月");
                    xml.WriteToCell(0, 8, "10個月");
                    xml.WriteToCell(0, 9, "11個月");
                    xml.WriteToCell(0, 10, "12個月");
                    xml.WriteToCell(0, 11, "申請金額");
                    xml.WriteToCell(0, 12, "撥款金額");
                    xml.WriteToCell(0, 13, "審核狀態/補件說明");

                    for (int row = 1; row <= model.Count(); row++)
                    {
                        var data = model[row - 1];
                        xml.WriteToCell(row, 0, data.Id.ToString());
                        xml.WriteToCell(row, 1, data.IndustryName);
                        xml.WriteToCell(row, 2, data.EligibleApplicants.ToString());
                        xml.WriteToCell(row, 3, data.CurrentApplicants.ToString());
                        xml.WriteToCell(row, 4, data.SixMonth.ToString());
                        xml.WriteToCell(row, 5, data.SevenMonth.ToString());
                        xml.WriteToCell(row, 6, data.EightMonth.ToString());
                        xml.WriteToCell(row, 7, data.NineMonth.ToString());
                        xml.WriteToCell(row, 8, data.TenMonth.ToString());
                        xml.WriteToCell(row, 9, data.ElevenMonth.ToString());
                        xml.WriteToCell(row, 10, data.TwelveMonth.ToString());
                        xml.WriteToCell(row, 11, data.ApplicationAmount.ToString());
                        xml.WriteToCell(row, 12, data.GrantAmount.ToString());
                        xml.WriteToCell(row, 13, data.Memo);
                    }

                    // 保存 Excel 文件到 MemoryStream
                    xml.SaveToExcel(memoryStream);
                }

                // 设置响应的内容类型
                Response.ContentType = contentType;
                // 添加 'Content-Disposition' header，触发下载
                Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
                // 写入数据流
                Response.BinaryWrite(memoryStream.ToArray());
            }

            return new EmptyResult();
        }

        /// <summary>
        /// 匯出每月請款
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportRequestPayment()
        {
            string fileName = "每月請款.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            List<CaseQueryViewModel.RequestPaymentData> model = (List<CaseQueryViewModel.RequestPaymentData>)Session["RequestPaymentList"];

            using (var memoryStream = new MemoryStream())
            {
                using (var xml = new OpenXmlExcel())
                {
                    //欄位
                    xml.WriteToCell(0, 0, "序號");
                    xml.WriteToCell(0, 1, "撥款狀況");
                    xml.WriteToCell(0, 2, "案號");
                    xml.WriteToCell(0, 3, "旅宿業名稱");
                    xml.WriteToCell(0, 4, "可申請人數");
                    xml.WriteToCell(0, 5, "1個月");
                    xml.WriteToCell(0, 6, "2個月");
                    xml.WriteToCell(0, 7, "3個月");
                    xml.WriteToCell(0, 8, "4個月");
                    xml.WriteToCell(0, 9, "5個月");
                    xml.WriteToCell(0, 10, "6個月");
                    xml.WriteToCell(0, 11, "7個月");
                    xml.WriteToCell(0, 12, "8個月");
                    xml.WriteToCell(0, 13, "9個月");
                    xml.WriteToCell(0, 14, "10個月");
                    xml.WriteToCell(0, 15, "11個月");
                    xml.WriteToCell(0, 16, "12個月");
                    xml.WriteToCell(0, 17, "申請人數");
                    xml.WriteToCell(0, 18, "申請金額");
                    xml.WriteToCell(0, 19, "實撥金額");
                    xml.WriteToCell(0, 20, "收款人戶名");
                    xml.WriteToCell(0, 21, "銀行名稱");
                    xml.WriteToCell(0, 22, "銀行代碼");
                    xml.WriteToCell(0, 23, "銀行帳號");

                    for (int row = 1; row <= model.Count(); row++)
                    {
                        var data = model[row - 1];
                        xml.WriteToCell(row, 0, data.ID.ToString());
                        xml.WriteToCell(row, 1, data.PayStatus);
                        xml.WriteToCell(row, 2, data.SubsidyNo.ToString());
                        xml.WriteToCell(row, 3, data.IndustryName);
                        xml.WriteToCell(row, 4, data.EligibleApplicants.ToString());
                        xml.WriteToCell(row, 5, data.OneMonth.ToString());
                        xml.WriteToCell(row, 6, data.TwoMonth.ToString());
                        xml.WriteToCell(row, 7, data.ThreeMonth.ToString());
                        xml.WriteToCell(row, 8, data.FourMonth.ToString());
                        xml.WriteToCell(row, 9, data.FiveMonth.ToString());
                        xml.WriteToCell(row, 10, data.SixMonth.ToString());
                        xml.WriteToCell(row, 11, data.SevenMonth.ToString());
                        xml.WriteToCell(row, 12, data.EightMonth.ToString());
                        xml.WriteToCell(row, 13, data.NineMonth.ToString());
                        xml.WriteToCell(row, 14, data.TenMonth.ToString());
                        xml.WriteToCell(row, 15, data.ElevenMonth.ToString());
                        xml.WriteToCell(row, 16, data.TwelveMonth.ToString());
                        xml.WriteToCell(row, 17, data.CurrentApplicants.ToString());
                        xml.WriteToCell(row, 18, data.ApplicationAmount.ToString());
                        xml.WriteToCell(row, 19, data.GrantAmount.ToString());
                        xml.WriteToCell(row, 20, data.BankAcctName);
                        xml.WriteToCell(row, 21, data.BankName);
                        xml.WriteToCell(row, 22, data.BankCode);
                        xml.WriteToCell(row, 23, data.BankAcct);
                    }

                    // 保存 Excel 文件到 MemoryStream
                    xml.SaveToExcel(memoryStream);
                }

                // 设置响应的内容类型
                Response.ContentType = contentType;
                // 添加 'Content-Disposition' header，触发下载
                Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
                // 写入数据流
                Response.BinaryWrite(memoryStream.ToArray());
            }

            return new EmptyResult();
        }

    }
}