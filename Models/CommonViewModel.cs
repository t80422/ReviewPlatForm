using System.Collections.Generic;
using System.Web.Mvc;

namespace WebApplication1.Models
{
    public class CommonViewModel
    {
        public class ReviewModelBase
        {
            /// <summary>
            /// 審核狀態選項
            /// </summary>
            public SelectList ReviewOptions { get; set; }
            public SelectList FileReviewOptions { get; set; }
            public manager InitialReviewer { get; set; }
            public manager SecondaryReviewer { get; set; }
            public manager AssociationReviewer { get; set; }

            public ReviewModelBase()
            {
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

                ReviewOptions = new SelectList(reviewOptions, "Value", "Text", string.Empty);
                FileReviewOptions = new SelectList(fileOptions, "Value", "Text", string.Empty);
            }             
        }
    }
}