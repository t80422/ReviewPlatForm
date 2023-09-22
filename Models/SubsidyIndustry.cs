using System;
using System.Web;

namespace WebApplication1.Models
{
    public class SubsidyIndustry
    {
        //申請月份(迄)
        public Nullable<System.DateTime> s_date_time_end { get; set; }
        public int s_id { get; set; }

        public int s_id_id { get; set; }

        public int? s_new_in { get; set; }

        public DateTime s_date_time { get; set; }

        public int? s_emp_count { get; set; }

        public int s_money { get; set; }

        public DateTime? s_grant_date { get; set; }

        public string s_review { get; set; }

        public Nullable<int> s_mg_id_fst { get; set; }

        public Nullable<int> s_mg_id_snd { get; set; }

        public Nullable<System.DateTime> s_last_time { get; set; }

        public HttpPostedFileBase s_insur_member { get; set; }

        public string s_insur_member_name { get; set; }

        public HttpPostedFileBase s_emp_lst { get; set; }

        public string s_emp_lst_name { get; set; }

        public HttpPostedFileBase s_salary_prove { get; set; }

        public string s_salary_prove_name { get; set; }

        public HttpPostedFileBase s_affidavit { get; set; }

        public string s_affidavit_name { get; set; }

        public HttpPostedFileBase s_receipt { get; set; }

        public string s_receipt_name { get; set; }

        public HttpPostedFileBase s_else { get; set; }

        public string s_else_name { get; set; }

        public int id_id { get; set; }

        public Nullable<int> id_it_id { get; set; }

        public string id_name { get; set; }

        public string id_address { get; set; }

        public string id_tel { get; set; }

        public string id_fax { get; set; }

        public string id_company { get; set; }

        public string id_tax_id { get; set; }

        public string id_owner { get; set; }

        public string id_tel_owner { get; set; }

        public Nullable<int> id_room { get; set; }

        public Nullable<System.DateTime> id_last_time { get; set; }

        public string id_license { get; set; }

        public string id_license_name { get; set; }

        public string id_register { get; set; }

        public string id_register_name { get; set; }

        public string id_email { get; set; }

        public string id_bank_code { get; set; }

        public string id_bank_acct { get; set; }

        public string id_bank_acct_name { get; set; }

        public string id_passbook { get; set; }

        public string id_passbook_name { get; set; }

        public Nullable<int> id_mg_id_fst { get; set; }

        public Nullable<int> id_mg_id_snd { get; set; }

        public string id_review { get; set; }

        public string id_city { get; set; }

        public string id_tax_registration { get; set; }

        public string id_tax_registration_name { get; set; }

        public string s_no { get; set; }

        public string s_submit { get; set; }
    }
}