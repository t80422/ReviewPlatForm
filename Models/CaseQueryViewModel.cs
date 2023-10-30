using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}