using System;
using System.Web;

namespace WebApplication1.Models
{
    public class SubMembersMembers
    {
        public int sm_id { get; set; }

        public int sm_s_id { get; set; }

        public Nullable<System.DateTime> sm_agree_start { get; set; }

        public Nullable<System.DateTime> sm_agree_end { get; set; }

        public Nullable<int> sm_advance_money { get; set; }

        public string sm_review { get; set; }

        public Nullable<System.DateTime> sm_date_review { get; set; }

        public Nullable<int> sm_mg_id_fst { get; set; }

        public Nullable<int> sm_mg_id_snd { get; set; }

        public Nullable<System.DateTime> sm_last_time { get; set; }

        public Nullable<int> sm_id_id { get; set; }

        public Nullable<int> sm_mb_id { get; set; }
        public int mb_id { get; set; }

        public int mb_id_id { get; set; }

        public string mb_name { get; set; }

        public string mb_id_card { get; set; }

        public string mb_birthday { get; set; }

        public int mb_insur_salary { get; set; }

        public string mb_add_insur { get; set; }

        public Nullable<System.DateTime> mb_add_insur_date { get; set; }

        public Nullable<System.DateTime> mb_surrender_date { get; set; }

        public string mb_memo { get; set; }

        public System.DateTime mb_last_time { get; set; }

        public string mb_contract { get; set; }

        public string mb_contract_name { get; set; }

    }
}