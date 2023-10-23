using DocumentFormat.OpenXml.Spreadsheet;
using PagedList;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Models
{
    public class SubsidyMemberData
    {
        //todo 參考IndustryModelView 修改
        public subsidy_member Subsidy_Member { get; set; }
        public member Member { get; set; }
        public manager InitialReviewer { get; set; }
        public manager SecondaryReviewer { get; set; }
        public manager AssociationReviewer { get; set; }
        public subsidy Subsidy { get; set; }
        public SelectList InsuranceStatusOptions { get; set; }
        public SelectList ReviewOptions { get; set; }
        public SelectList FileReviewOptions { get; set; }
        public SelectList YesOrNo { get; set; }
        /// <summary>
        /// 勞動契約檔案
        /// </summary>
        public HttpPostedFileBase ContractFile { get; set; }
        /// <summary>
        /// 薪資證明檔案
        /// </summary>
        public HttpPostedFileBase IncomeCertificateFile { get; set; }
        public IPagedList<subsidy_member_review> ReviewList { get; set; }
        public string FullTimeOrNotString { get; set; }
        public string QualificationsString { get; set; }
        /// <summary>
        /// 是否符合資格
        /// </summary>
        //public bool Qualifications {  get; set; }
        /// <summary>
        /// 試算補助金額
        /// </summary>
        //public int Calculation {  get; set; }

        public SubsidyMemberData()
        {
            var insureOptions = new List<SelectListItem>
            {
                new SelectListItem{Text="加保",Value="1"},
                new SelectListItem{Text="退保",Value="2"},
                new SelectListItem{Text="調薪",Value="3"},
            };
            var reviewOptions = new List<SelectListItem>()
            {
                new SelectListItem{Text="待審核",Value="待審核"},
                new SelectListItem{Text="審核中",Value="審核中"},
                new SelectListItem{Text="待補件",Value="待補件"},
                new SelectListItem{Text="退件",Value="退件"},
                new SelectListItem{Text="審核完成",Value="審核完成"},
            };
            var fileOptions = new List<SelectListItem>()
            {
                new SelectListItem{Text="審核中",Value="審核中"},
                new SelectListItem{Text="待補件",Value="待補件"},
                new SelectListItem{Text="通過",Value="通過"},
            };
            var yesOrNo = new List<SelectListItem>
            {
                new SelectListItem{Text="是",Value="1"},
                new SelectListItem{Text="否",Value="0"},
            };

            InsuranceStatusOptions = new SelectList(insureOptions, "Value", "Text", string.Empty);
            ReviewOptions = new SelectList(reviewOptions, "Value", "Text", string.Empty);
            YesOrNo = new SelectList(yesOrNo, "Value", "Text", string.Empty);
            FileReviewOptions = new SelectList(fileOptions, "Value", "Text", string.Empty);
        }

        public bool FullTimeOrNot
        {
            get { return FullTimeOrNotString == "1"; }
        }

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
}