using System;
using System.Web;

namespace WebApplication1.Models
{
    public class Members
    {
        public int mb_id { get; set; }

        public Nullable<int> mb_id_id { get; set; }

        public string mb_name { get; set; }

        //身分證字號
        public string mb_id_card { get; set; }

        public string mb_birthday { get; set; }

        public Nullable<int> mb_insur_salary { get; set; }

        public string mb_add_insur { get; set; }

        public Nullable<System.DateTime> mb_add_insur_date { get; set; }

        public Nullable<System.DateTime> mb_surrender_date { get; set; }

        public string mb_memo { get; set; }

        public Nullable<System.DateTime> mb_last_time { get; set; }

        public HttpPostedFileBase mb_contract { get; set; }

        public string mb_contract_name { get; set; }

        public string mb_insurance_id { get; set; }

        public Nullable<System.DateTime> mb_full_time_date { get; set; }

        public HttpPostedFileBase mb_income_certificate { get; set; }

        public string mb_income_certificate_name { get; set; }
        public Nullable<bool> mb_full_time_or_not { get; set; }

        public string mb_position { get; set; }
    }
}