using PagedList;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using static WebApplication1.Models.SubsidyViewModel;

namespace WebApplication1.Models
{
    public class SubsidyViewModel
    {
        public class SubsidyDetails : CommonViewModel.ReviewModelBase
        {
            public industry Industry { get; set; }
            public subsidy Subsidy { get; set; }
            public member Member { get; set; }
            public List<SubMemberList> SubMemberList { get; set; }
            public int TrialAmount { get; set; }
            /// <summary>
            /// 申請文件檔案
            /// </summary>
            public HttpPostedFileBase ApplicationFile { get; set; }
            /// <summary>
            /// 全公司勞保明細
            /// </summary>
            public HttpPostedFileBase InsurMemberFile { get; set; }
            /// <summary>
            /// 建立全公司勞保清冊檔案
            /// </summary>
            public HttpPostedFileBase EmpListFile { get; set; }
            /// <summary>
            /// 申請補助人員清冊檔案
            /// </summary>
            public HttpPostedFileBase ApplicantsFile { get; set; }
            /// <summary>
            /// 補件資料1
            /// </summary>
            public HttpPostedFileBase ElseOneFile { get; set; }
            /// <summary>
            /// 補件資料2
            /// </summary>
            public HttpPostedFileBase ElseTwoFile { get; set; }
            /// <summary>
            /// 補件資料3
            /// </summary>
            public HttpPostedFileBase ElseThreeFile { get; set; }
            /// <summary>
            /// 員工清冊
            /// </summary>
            public HttpPostedFileBase EmployeeInventoryFile { get; set; }
            public bool ViewMode { get; set; }
        }

        public class SubMemberList
        {
            public subsidy_member Subsidy_Member { get; set; }
            public member Member { get; set; }
        }
    }

    public class SubsidyBase
    {
        public subsidy Subsidy { get; set; }
    }

    public class Subsidy_ReviewIndex : SubsidyBase
    {
        public IPagedList<subsidy_review> Subsidy_ReviewList { get; set; }
    }

    public class SubsidyIndexViewModel
    {
        public IPagedList<SubsidyDetails> Subsidies { get; set; }
        public List<ReviewerViewModel> Reviewers { get; set; }
    }

    public class ReviewerViewModel
    {
        public int? ID { get; set; }
        public string Name { get; set; }
    }

    public class SubsidyIndexQViewModel
    {
        public IPagedList<SubsidyDetails> Subsidies { get; set; }
        public industry Industry { get; set; }
    }
}