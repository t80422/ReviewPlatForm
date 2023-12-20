using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static WebApplication1.Models.CaseQueryViewModel;

namespace WebApplication1.Models
{
    public class CaseQueryViewModel
    {
        public class CaseQueryData
        {
            public subsidy Subsidy { get; set; }
            public industry Industry { get; set; }
            /// <summary>
            /// 可申請人數
            /// </summary>
            public int EligibleApplicants { get; set; }
            /// <summary>
            /// 本次申請人數
            /// </summary>
            public int CurrentApplicants { get; set; }
            /// <summary>
            /// 申請通過人數
            /// </summary>
            public int ApprovedApplicants { get; set; }
        }

        public class Index
        {
            public IPagedList<CaseQueryData> caseQueryList { get; set; }
        }

        public class CaseStatistics
        {
            public IPagedList<CaseStatisticsData> CaseStatisticsList { get; set; }
        }

        public class CaseStatisticsData
        {
            public int Id { get; set; }
            public string IndustryName { get; set; }
            /// <summary>
            /// 可申請人數
            /// </summary>
            public double EligibleApplicants { get; set; }
            /// <summary>
            /// 本次申請人數
            /// </summary>
            public int CurrentApplicants { get; set; }
            public int SixMonth {  get; set; }
            public int SevenMonth { get; set; }
            public int EightMonth {  get; set; }
            public int NightMonth { get; set; }
            public int TenMonth { get; set; }
            public int ElevenMonth { get; set; }
            public int TwelveMonth { get; set; }
            public int ApplicationAmount { get; set; }
            public int GrantAmount { get; set; }
            public string Memo { get; set; }
        }
    }
}