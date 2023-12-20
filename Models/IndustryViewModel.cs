using PagedList;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Models
{
    public class IndustryViewModel
    {
        public class Index
        {
            public IPagedList<industry> Industries { get; set; }
        }

        public class IndustryModel : CommonViewModel.ReviewModelBase
        {
            public industry Industry { get; set; }
            public SelectList IndustryTypeOptions { get; set; }
            /// <summary>
            /// 營業狀態
            /// </summary>
            public SelectList BusinessStatus { get; set; }
            public HttpPostedFileBase PassBookFile { get; set; }
            public HttpPostedFileBase RegisterFile { get; set; }
            public HttpPostedFileBase LicenseFile { get; set; }

            public IndustryModel()
            {
                var industryType = new List<SelectListItem>()
                {
                    new SelectListItem{Text="觀光旅館",Value="1"},
                    new SelectListItem{Text="旅館",Value="2"},
                    new SelectListItem{Text="民宿",Value="3"},
                };
                var businessStatus = new List<SelectListItem>()
                {
                    new SelectListItem{Text="營業",Value="True"},
                    new SelectListItem{Text="停業",Value="False"},
                };

                IndustryTypeOptions = new SelectList(industryType, "Value", "Text", string.Empty);
                BusinessStatus = new SelectList(businessStatus, "Value", "Text", string.Empty);
            }
        }

    }
}