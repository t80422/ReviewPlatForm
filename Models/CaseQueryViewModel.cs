using PagedList;

namespace WebApplication1.Models
{
    public class CaseQueryViewModel
    {
        public class CaseQueryData
        {
            public subsidy Subsidy { get; set; }
            public industry Industry { get; set; }
            /// <summary>
            /// 可申請人數
            /// </summary>
            public int EligibleApplicants { get; set; }
            /// <summary>
            /// 本次申請人數
            /// </summary>
            public int CurrentApplicants { get; set; }
            /// <summary>
            /// 申請通過人數
            /// </summary>
            public int ApprovedApplicants { get; set; }
        }

        public class Index
        {
            public IPagedList<CaseQueryData> caseQueryList { get; set; }
        }

        public class CaseStatistics
        {
            public IPagedList<CaseStatisticsData> CaseStatisticsList { get; set; }
        }

        public class CaseStatisticsData
        {
            public int Id { get; set; }
            public string IndustryName { get; set; }
            /// <summary>
            /// 可申請人數
            /// </summary>
            public double EligibleApplicants { get; set; }
            /// <summary>
            /// 本次申請人數
            /// </summary>
            public int CurrentApplicants { get; set; }
            public int SixMonth { get; set; }
            public int SevenMonth { get; set; }
            public int EightMonth { get; set; }
            public int NineMonth { get; set; }
            public int TenMonth { get; set; }
            public int ElevenMonth { get; set; }
            public int TwelveMonth { get; set; }
            /// <summary>
            /// 申請金額
            /// </summary>
            public int ApplicationAmount { get; set; }
            /// <summary>
            /// 撥款金額
            /// </summary>
            public int GrantAmount { get; set; }
            public string Memo { get; set; }
        }

        public class RequestPayment
        {
            public IPagedList<RequestPaymentData> RequestPaymentList { get; set; }
        }

        public class RequestPaymentData
        {
            public int ID { get; set; }
            /// <summary>
            /// 撥款狀況
            /// </summary>
            public string PayStatus { get; set; }
            public string SubsidyNo { get; set; }
            public string IndustryName { get; set; }
            /// <summary>
            /// 可申請人數
            /// </summary>
            public double EligibleApplicants { get; set; }
            public int OneMonth { get; set; }
            public int TwoMonth { get; set; }
            public int ThreeMonth { get; set; }
            public int FourMonth { get; set; }
            public int FiveMonth { get; set; }
            public int SixMonth { get; set; }
            public int SevenMonth { get; set; }
            public int EightMonth { get; set; }
            public int NineMonth { get; set; }
            public int TenMonth { get; set; }
            public int ElevenMonth { get; set; }
            public int TwelveMonth { get; set; }
            /// <summary>
            /// 本次申請人數
            /// </summary>
            public int CurrentApplicants { get; set; }
            /// <summary>
            /// 申請金額
            /// </summary>
            public int ApplicationAmount { get; set; }
            /// <summary>
            /// 撥款金額
            /// </summary>
            public int GrantAmount { get; set; }
            public string BankAcctName { get; set; }
            public string BankName { get; set; }
            public string BankCode { get; set; }
            public string BankAcct { get; set; }
        }
    }
}