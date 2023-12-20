using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
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

            ViewBag.subsidyID = subsidyID;

            if (extension != ".xlsx" && extension != ".xls")
            {
                Session["msg"] = "請選擇excel檔案";
                return View(subsidyID);
            }

            using (OpenXmlExcel oxExcel = new OpenXmlExcel(file.InputStream, false))
            {
                var industryID = db.subsidy.Find(subsidyID).s_id_id;
                var subsidyNo = db.subsidy.Find(subsidyID).s_no;
                List<employment_insurance> list = new List<employment_insurance>();

                try
                {
                    foreach (Row row in oxExcel.Worksheet.Descendants<Row>().Where(r => r.RowIndex >= 6))
                    {
                        var rowIndex = int.Parse(row.RowIndex);
                        var columnIndex = 5;

                        if (string.IsNullOrEmpty(oxExcel.GetCellValue(rowIndex, 2))) break;

                        if (!Utility.ROCDateToACDate(oxExcel.GetCellValue(rowIndex, 4), out DateTime changeDate))
                        {
                            Session["msg"] = $"日期格式錯誤 第{rowIndex}列 4行";
                            return View(subsidyID);
                        }

                        while (columnIndex <= 40)
                        {
                            var data = new employment_insurance();

                            data.ei_name = oxExcel.GetCellValue(rowIndex, 2);
                            data.ei_id_card = oxExcel.GetCellValue(rowIndex, 3);
                            data.ei_type = oxExcel.GetCellValue(rowIndex, 41);
                            data.ei_enter_month = enterMonth;
                            data.ei_id_id = (int)industryID;
                            data.ei_type = oxExcel.GetCellValue(rowIndex, 41);
                            data.ei_id_id = (int)industryID;
                            data.ei_subsidy_no = subsidyNo;
                            data.ei_last_change_date = changeDate.ToString("yyyy/MM/dd");

                            var sDate = oxExcel.GetCellValue(rowIndex, columnIndex);

                            if (string.IsNullOrEmpty(sDate))
                            {
                                columnIndex+=3;
                                continue;
                            }

                            sDate = sDate.Replace("年", "/").Replace("月", "");

                            var dateParts = sDate.Split('/');

                            data.ei_year = int.Parse(dateParts[0]) + 1911;
                            data.ei_month = int.Parse(dateParts[1]);

                            #region 檢查是否重複

                            bool isDuplicate = db.employment_insurance.Any(ei =>
                                                                            ei.ei_id_card == data.ei_id_card &&
                                                                            ei.ei_id_id == data.ei_id_id &&
                                                                            ei.ei_year == data.ei_year &&
                                                                            ei.ei_month == data.ei_month);
                            if (isDuplicate)
                            {
                                Session["msg"] = $"重複的年月 第{rowIndex}列 {columnIndex}行";
                                return View(subsidyID);
                            }

                            #endregion

                            columnIndex++;

                            data.ei_no = int.Parse(oxExcel.GetCellValue(rowIndex, columnIndex));
                            columnIndex++;

                            data.ei_salary = int.Parse(oxExcel.GetCellValue(rowIndex, columnIndex));
                            columnIndex++;

                            list.Add(data);
                        }
                    }

                    db.employment_insurance.AddRange(list);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    Session["msg"] = "重複年月,請檢查檔案";
                    return View(subsidyID);
                }
            }

            Session["msg"] = "上傳成功";

            return RedirectToAction("Edit_Manager", "Subsidy", new { subsidyID, isView = false, title = 1 });
        }

        public ActionResult Index(string idCard, int industryID)
        {
            var model = new EmploymentInsuranceViewModel();
            var query = db.employment_insurance.Where(x => x.ei_id_card == idCard && x.ei_id_id == industryID)
                                               .OrderBy(x => x.ei_year)
                                               .ThenBy(x => x.ei_month);

            model.Employments = query.ToList();
            model.Industry = db.industry.Find(industryID);
            model.IDCard = idCard;

            return View(model);
        }

        public ActionResult Delete(int year, int month, string idCard, int industryID)
        {
            var data = db.employment_insurance.Find(year, month, idCard, industryID);

            db.employment_insurance.Remove(data);
            db.SaveChanges();

            return RedirectToAction("Index", new { idCard, industryID });
        }
    }
}