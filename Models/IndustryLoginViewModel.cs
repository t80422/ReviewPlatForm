using PagedList;
using System;

namespace WebApplication1.Models
{
    public class IndustryLoginViewModel
    {
        public class IndustryLogin
        {
            public string IndustryName { get; set; }
            public string OwnerName { get; set; }
            public DateTime LoginTime { get; set; }
            public DateTime? LogoutTime { get; set; }
        }

        public class Index
        {
            public IPagedList<IndustryLogin> LoginList { get; set; }
        }
    }
}