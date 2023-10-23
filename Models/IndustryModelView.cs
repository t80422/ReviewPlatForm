using PagedList;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Models
{
    public class IndustryModelView
    {
        public class Index
        {
            public IPagedList<industry> Industries { get; set; }
        }

        public class Edit_Manager : CommonViewModel.ReviewModelBase
        {
            public industry Industry { get; set; }
            public SelectList IndustryTypeOptions { get; set; }
            public HttpPostedFileBase PassBookFile { get; set; }
            public HttpPostedFileBase RegisterFile { get; set; }
            public HttpPostedFileBase License { get; set; }

            public Edit_Manager()
            {
                var industryType = new List<SelectListItem>()
            {
                new SelectListItem{Text="觀光旅館",Value="1"},
                new SelectListItem{Text="旅館",Value="2"},
                new SelectListItem{Text="民宿",Value="3"},
            };

                IndustryTypeOptions = new SelectList(industryType, "Value", "Text", string.Empty);
            }
        }


    }
}