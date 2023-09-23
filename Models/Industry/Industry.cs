using System;
using System.Web;

namespace WebApplication1.Models.Industry
{
    public class Industry
    {
        //承辦人Email
        public string OwnerEmail { get; set; }
        //銀行名稱
        public string BankName { get; set; }
        //承辦人電話區碼
        public string id_owner_area_code { get; set; }
        //郵遞區號
        public string id_postal_code { get; set; }

        //電話區號
        public string id_area_code { get; set; }

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

        public string id_extension { get; set; }

        public string id_owner_phone { get; set; }

        public Nullable<int> id_room { get; set; }

        public Nullable<System.DateTime> id_last_time { get; set; }

        public HttpPostedFileBase id_license { get; set; }

        public string id_license_name { get; set; }

        public HttpPostedFileBase id_register { get; set; }

        public string id_register_name { get; set; }

        public string id_email { get; set; }

        public string id_bank_code { get; set; }

        public string id_bank_acct { get; set; }

        public string id_bank_acct_name { get; set; }

        public HttpPostedFileBase id_passbook { get; set; }

        public string id_passbook_name { get; set; }

        public Nullable<int> id_mg_id_fst { get; set; }

        public Nullable<int> id_mg_id_snd { get; set; }

        public string id_review { get; set; }

        public string id_city { get; set; }

        public string HotelTypeDescription
        {
            get
            {
                return ConvertHotelType((int)id_it_id);
            }
        }

        public string ConvertHotelType(int type)
        {
            switch (type)
            {
                case 1:
                    return "觀光旅館";
                case 2:
                    return "旅館";
                case 3:
                    return "民宿";
                default:
                    return "未知";
            }
        }
    }
}