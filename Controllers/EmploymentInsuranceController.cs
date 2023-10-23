using DocumentFormat.OpenXml.Spreadsheet;
using PagedList;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class EmploymentInsuranceController : Controller
    {
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        public ActionResult Import(int subsidyID)
        {
            ViewBag.subsidyID = subsidyID;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(int subsidyID, HttpPostedFileBase file, int? enterMonth)
        {
            var extension = Path.GetExtension(file.FileName);

            if (extension != ".xlsx" && extension != ".xls")
            {
                Session["msg"] = "請選擇excel檔案";
                ViewBag.subsidyID = subsidyID;

                return View();
            }

            using (OpenXmlExcel oxExcel = new OpenXmlExcel(file.InputStream, false))
            {
                employment_insurance data;

                foreach (Row row in oxExcel.Worksheet.Descendants<Row>().Where(r => r.RowIndex >= 2))
                {
                    var rowIndex = row.RowIndex.ToString();

                    if (oxExcel.GetCellValue("A" + rowIndex) == null) break;

                    data = new employment_insurance();
                    data.ei_year =int.Parse( oxExcel.GetCellValue("A"+rowIndex));
                    data.ei_month = int.Parse(oxExcel.GetCellValue("B" + rowIndex));
                    data.ei_no = int.Parse(oxExcel.GetCellValue("C" + rowIndex));
                    data.ei_name = oxExcel.GetCellValue("D" + rowIndex);
                    data.ei_id_card = oxExcel.GetCellValue("E" + rowIndex);

                    var birthday = oxExcel.GetCellValue("F" + rowIndex);

                    if (birthday.Length != 7)
                    {
                        Session["msg"] = $"第{rowIndex}列的生日格式錯誤,參考格式:0801019";
                        ViewBag.subsidyID = subsidyID;
                        return View();
                    }

                    data.ei_birthday = birthday;
                    data.ei_type = oxExcel.GetCellValue("G" + rowIndex);
                    data.ei_last_change_date = oxExcel.GetCellValue("H" + rowIndex);
                    data.ei_memo = oxExcel.GetCellValue("I" + rowIndex);
                    data.ei_enter_month = enterMonth;

                    db.employment_insurance.AddOrUpdate(ei => new { ei.ei_year, ei.ei_month, ei.ei_id_card }, data);
                }

                db.SaveChanges();
            }
            Session["msg"] = "上傳成功";

            return RedirectToAction("Edit_Manager", "Subsidy", new { subsidyID });
        }

        public ActionResult Index(string idCard, int? page = 1)
        {
            var query = db.employment_insurance.Where(x => x.ei_id_card == idCard).OrderBy(x => x.ei_year).ThenBy(x => x.ei_month);

            return View(query.ToPagedList((int)page, 10));
        }
    }
}
