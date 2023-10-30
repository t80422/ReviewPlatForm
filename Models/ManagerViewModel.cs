using PagedList;
using System.Collections.Generic;
using System.Web.Mvc;

namespace WebApplication1.Models
{
    public class ManagerViewModel
    {
        public class ManagerAccounts
        {
            public manager Manager { get; set; }
            public user_accounts User_Accounts { get; set; }
            public string PermissionsText
            {
                get
                {
                    switch (User_Accounts.ua_perm)
                    {
                        case 0:
                            return "觀光協會";
                        case 1:
                            return "PM";
                        case 2:
                            return "審核人員";
                        case 4:
                            return "觀察員";
                        default:
                            return "未知";
                    }
                }
            }
        }

        public class Index
        {
            public IPagedList<ManagerAccounts> ManagerAccounts { get; set; }
        }

        public class CreateOrEdit
        {
            public manager Manager { get; set; }
            public user_accounts User_Accounts { get; set; }
            public SelectList PermissionsOptions { get; set; }

            public CreateOrEdit()
            {
                var options = new List<SelectListItem>
                {
                    new SelectListItem{Text="觀光協會",Value="0"},
                    new SelectListItem{Text="PM",Value= "1"},
                    new SelectListItem{Text="審核人員",Value = "2"},
                    new SelectListItem{Text="觀察員",Value = "4"},
                };
                PermissionsOptions = new SelectList(options, "Value", "Text", string.Empty);
            }
        }
    }
}