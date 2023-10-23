using PagedList;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Models
{
    public class SubsidyBase
    {
        public subsidy Subsidy { get; set; }
    }

    public class Subsidy_EditManager : SubsidyBase
    {
        public industry Industry { get; set; }
        public manager InitialReviewer { get; set; }
        public manager SecondaryReviewer { get; set; }
        public manager AssociationReviewer { get; set; }
        public IEnumerable<SelectListItem> ReviewOptions { get; set; }
        public SelectList FileReviewOptions { get; set; }
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

        public string GetInitialReviewerName()
        {
            return InitialReviewer?.mg_name;
        }
        public string GetSecondaryReviewerName()
        {
            return SecondaryReviewer?.mg_name;
        }
        public string GetAssociationReviewer()
        {
            return AssociationReviewer?.mg_name;
        }
    }

    public class Subsidy_ReviewIndex : SubsidyBase
    {
        public IPagedList<subsidy_review> Subsidy_ReviewList { get; set; }
    }

    public class SubsidyIndexViewModel
    {
        public IPagedList<SubsidyIndustry> Subsidies { get; set; }
        public List<ReviewerViewModel> Reviewers { get; set; }
    }

    public class ReviewerViewModel
    {
        public int? ID { get; set; }
        public string Name { get; set; }
    }
}