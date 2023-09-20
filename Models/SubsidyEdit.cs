using System;
using System.Web;

namespace WebApplication1.Models
{
    public class SubsidyEdit
    {
        // 申請ID
        public int? s_id { get; set; }
        // 旅宿ID
        public int? id_id { get; set; }
        // 申請人數
        public int? EmpCount { get; set; }
        // 申請金額
        public int? Money { get; set; }
        // 申請日期
        public string Date { get; set; }
        // 申請書
        public HttpPostedFileBase Application { get; set; }
        // 申請書(檔案名稱)
        public string ApplicationName { get; set; }
        // 勞保名冊
        public HttpPostedFileBase Labor { get; set; }
        // 勞保名冊(檔案名稱)
        public string LaborName { get; set; }
        // 申請人員清冊
        public HttpPostedFileBase ApplicantsList { get; set; }
        // 申請人員清冊(檔案名稱)
        public string ApplicantsListName { get; set; }
        // 切結書
        public HttpPostedFileBase Affidavit { get; set; }
        // 切結書(檔案名稱)
        public string AffidavitName { get; set; }
        // 領據
        public HttpPostedFileBase Receipt { get; set; }
        // 領據(檔案名稱)
        public string ReceiptName { get; set; }
        // 員工清冊
        public HttpPostedFileBase EmployeeList { get; set; }
        // 員工清冊(檔案名稱)
        public string EmployeeListName { get; set; }
        // 其他檔案
        public HttpPostedFileBase OtherFile { get; set; }
        // 其他檔案(檔案名稱)
        public string OtherFileName { get; set; }
        //審核狀態
        public string Review {  get; set; }

    }
}
