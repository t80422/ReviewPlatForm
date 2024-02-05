using PagedList;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using static WebApplication1.Models.SubsidyViewModel;

namespace WebApplication1.Models
{
    public class SubsidyMemberViewModel : SubsidyDetails
    {
        public subsidy_member Subsidy_Member { get; set; }
        public SelectList InsuranceStatusOptions { get; set; }
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
        public string SystemQualifications { get; set; }
        public string OtherCompany { get; set; }
        public List<MemberApply> MemberApplyList { get; set; }

        public SubsidyMemberViewModel()
        {
            var insureOptions = new List<SelectListItem>
            {
                new SelectListItem{Text="加保",Value="1"},
                new SelectListItem{Text="退保",Value="2"},
                new SelectListItem{Text="薪調",Value="3"},
            };
            var yesOrNo = new List<SelectListItem>
            {
                new SelectListItem{Text="是",Value="true"},
                new SelectListItem{Text="否",Value="false"},
            };

            InsuranceStatusOptions = new SelectList(insureOptions, "Value", "Text", string.Empty);
            YesOrNo = new SelectList(yesOrNo, "Value", "Text", string.Empty);
        }

        public class MemberApply : SubMemberList
        {
            public string SubsidyNo { get; set; }
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