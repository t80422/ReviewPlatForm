using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Industry;

namespace WebApplication1.Controllers
{
    public class IndustryController : Controller
    {
        private AjaxsController ajax = new AjaxsController();
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        // GET: Industry
        public ActionResult Index(int? page = 1)
        {
            var query = db.industry.OrderByDescending(x => x.id_id);
            var result = query.ToPagedList((int)page, 10);

            return View(result);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Industry data)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/assets/upload/Industry");

                //var passbook = ajax.UploadFile(data.PassBook, path);
                //var lincense = ajax.UploadFile(data.Lincense, path);
                //var register = ajax.UploadFile(data.Register, path);
                //var taxRegistration = ajax.UploadFile(data.TaxRegistration, path);

                //var insertData = new industry()
                //{
                //    id_name = data.Name,
                //    id_address = data.Address,
                //    id_tel = data.Phone,
                //    id_fax = data.Fax,
                //    id_company = data.Company,
                //    id_tax_id = data.TaxID,
                //    id_owner = data.Owner,
                //    id_tel_owner = data.OwnerPhone,
                //    id_last_time = DateTime.Now,
                //    id_license = lincense,
                //    id_license_name = data.LicenseName,
                //    id_register = register,
                //    id_register_name = data.RegisterName,
                //    id_email = data.Email,
                //    id_bank_code = data.BankCode,
                //    id_bank_acct = data.BankAcct,
                //    id_bank_acct_name = data.BankAcctName,
                //    id_passbook = passbook,
                //    id_passbook_name = data.PassBookName,
                //    id_city = data.City,
                //    id_tax_registration = taxRegistration,
                //    id_tax_registration_name = data.TaxRegistrationName
                //};

                //db.industry.Add(insertData);
                db.SaveChanges();

                Session["msg"] = "新增成功";

                return RedirectToAction("Index", "Login");
            }
            return View(data);
        }

        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var data = db.industry.Find(id);

            if (data == null)
            {
                return HttpNotFound();
            }

            var result = new Industry
            {
                id_id = id.Value,
                id_address = data.id_address,
                id_tel = data.id_tel,
                id_fax = data.id_fax,
                id_company = data.id_company,
                id_tax_id = data.id_tax_id,
                id_owner = data.id_owner,
                id_tel_owner = data.id_tel_owner,
                id_extension = data.id_extension,
                id_owner_phone = data.id_owner_phone,
                id_room = (int)data.id_room,
                id_license_name = data.id_license_name,
                id_register_name = data.id_register_name,
                id_email = data.id_email,
                id_bank_code = data.id_bank_code,
                id_bank_acct = data.id_bank_acct,
                id_bank_acct_name = data.id_bank_acct_name,
                id_passbook_name = data.id_passbook_name,
                id_review = data.id_review,
                id_city = data.id_city,
                id_it_id = data.id_it_id,                
            };           

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Industry data)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/assets/upload/Industry");

                var passbook = ajax.UploadFile(data.id_passbook, path);
                var lincense = ajax.UploadFile(data.id_license, path);
                var register = ajax.UploadFile(data.id_register, path);

                var updateData = db.industry.Find((int)Session["UserID"]);

                if (updateData != null)
                {
                    if (!string.IsNullOrEmpty(passbook)) ajax.DeleteFile($"{path}/{updateData.id_passbook}");
                    if (!string.IsNullOrEmpty(lincense)) ajax.DeleteFile($"{path}/{updateData.id_license}");
                    if (!string.IsNullOrEmpty(register)) ajax.DeleteFile($"{path}/{updateData.id_register}");

                    updateData.id_passbook = passbook ?? updateData.id_passbook;
                    updateData.id_passbook_name = data.id_passbook != null ? data.id_passbook.FileName : updateData.id_passbook_name;
                    updateData.id_license = lincense ?? updateData.id_license;
                    updateData.id_license_name = data.id_license != null ? data.id_license.FileName : updateData.id_license_name;
                    updateData.id_register = register ?? updateData.id_register;
                    updateData.id_register_name = data.id_register != null ? data.id_register.FileName : updateData.id_register_name;
                    
                    updateData.id_bank_acct_name=data.id_bank_acct_name;
                    updateData.id_bank_code=data.id_bank_code;
                    updateData.id_owner = data.id_owner;
                    updateData.id_tel_owner = data.id_tel_owner;
                    updateData.id_extension = data.id_extension;
                    updateData.id_owner_phone = data.id_owner_phone;
                    updateData.id_tax_id = data.id_tax_id;
                    updateData.id_bank_acct = data.id_bank_acct;

                    db.SaveChanges();
                    Session["msg"] = "修改成功";

                    return (int)Session["perm"] < 3 ? RedirectToAction("Index") : RedirectToAction("Edit", new { id = data.id_id });
                }
            }
            return View(data);
        }

        public ActionResult Detail(int? id)
        {
            industry data;

            if (id == null)
            {
                var userID = (int)Session["UserID"];
                data = db.industry.First(x => x.id_id == userID);
            }
            else
            {
                data = db.industry.Find(id);
            }

            if (data == null)
            {
                return HttpNotFound();
            }

            return View(data);
        }

        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName).Trim();


                if (extension != ".xlsx" && extension != ".xls") throw new Exception("請確認副檔名是否正確!");

                var spreadsheets = ajax.ReadFile(file);

                /*
                 以基隆市為例，
                reader["觀光旅館"] = 第一張工作表
                reader["旅館"] = 第二張工作表
                reader["民宿"] = 第三張工作表
                 */

                int type = 0;

                // 失敗筆數
                int fail = 0;
                // 成功筆數
                int success = 0;

                List<industry> InsertData = new List<industry>();
                List<industry> UpdateData = new List<industry>();

                if (spreadsheets != null)
                {
                    foreach (var sheet in spreadsheets)
                    {
                        int rowIndex = 0;

                        // 工作表名稱
                        string sheetName = sheet.Key;

                        // 類別
                        switch (sheetName)
                        {
                            case "觀光旅館":
                                type = 1; break;
                            case "旅館":
                                type = 2; break;
                            case "民宿":
                                type = 3; break;
                            default:
                                type = 0; break;
                        }
                        // 跑內容
                        List<string> Row = new List<string>();

                        foreach (var row in sheet.Value)
                        {
                            // 第一列為標題(跳過)
                            if (rowIndex == 0)
                            {
                                Row = row;
                                rowIndex++;
                                continue;
                            }

                            try
                            {
                                var TaxId = Row.IndexOf("統一編號") >= 0 ? row[Row.IndexOf("統一編號")] : "";

                                // 民宿統編可能為空值
                                if (String.IsNullOrEmpty(TaxId) && type != 3) continue;

                                industry Data = db.industry.Where(x => x.id_tax_id == TaxId).FirstOrDefault() ?? new industry();

                                // 這次是否為更新
                                bool Updated = Data.id_id > 0;

                                #region 加入資料

                                Data.id_id = Data.id_id > 0 ? Data.id_id : 0;
                                Data.id_it_id = type;
                                Data.id_name = Row.IndexOf("中文名稱") >= 0 ? row[Row.IndexOf("中文名稱")] : Data.id_name;
                                Data.id_address = Row.IndexOf("地址") >= 0 ? row[Row.IndexOf("地址")] : Data.id_address;
                                Data.id_tel = Row.IndexOf("電話或手機") >= 0 ? row[Row.IndexOf("電話或手機")] : Data.id_tel;
                                Data.id_fax = Row.IndexOf("傳真") >= 0 ? row[Row.IndexOf("傳真")] : Data.id_fax;
                                Data.id_company = Row.IndexOf("公司/事業名稱") >= 0 ? row[Row.IndexOf("公司/事業名稱")] : Data.id_company;
                                Data.id_tax_id = Row.IndexOf("統一編號") >= 0 ? row[Row.IndexOf("統一編號")] : Data.id_tax_id;
                                Data.id_owner = Row.IndexOf("代表人") >= 0 ? row[Row.IndexOf("代表人")] : Data.id_owner;
                                var aaa = Data.id_room;
                                var bbb = Row.IndexOf("合計總房間數");
                                var ccc = row[Row.IndexOf("合計總房間數")] ?? "0";
                                var ddd = Convert.ToInt32(ccc);
                                Data.id_room = Row.IndexOf("合計總房間數") >= 0 ? Convert.ToInt32(row[Row.IndexOf("合計總房間數")] ?? "0") : Data.id_room;
                                //Convert.ToInt32(Row.IndexOf("合計總房間數") >= 0 ? (row[Row.IndexOf("合計總房間數")] ?? "0") : (Data.id_room ?? 0).ToString());
                                Data.id_last_time = DateTime.Now;
                                Data.id_email = Row.IndexOf("E-mail") >= 0 ? row[Row.IndexOf("E-mail")] : Data.id_email;
                                Data.id_city = Row.IndexOf("E-mail") >= 0 ? row[Row.IndexOf("E-mail")] : Data.id_city;

                                db.industry.AddOrUpdate(x => x.id_tax_id, Data);
                                db.SaveChanges();

                                #endregion 加入資料

                                #region 新增則加入帳號

                                if (!Updated)
                                {
                                    user_accounts User = new user_accounts()
                                    {
                                        ua_acct = TaxId,
                                        ua_psw = ajax.ConvertToSHA256(TaxId),
                                        ua_perm = 3,
                                        ua_user_id = Data.id_id,
                                    };
                                    db.user_accounts.Add(User);
                                    db.SaveChanges();
                                }

                                #endregion 新增則加入帳號

                                success++;

                            }
                            catch (Exception e)
                            {
                                fail++;
                            }
                        }

                    }

                    return Json($"成功 {success} 筆，失敗 {fail} 筆");
                }


                return Json("查無資料");
            }
            else
            {
                throw new Exception("找無檔案");
            }


        }
    }
}