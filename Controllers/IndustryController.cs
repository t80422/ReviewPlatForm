using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.Industry;

namespace WebApplication1.Controllers
{
    [IsLogin]
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
            var result = new Industry();

            ViewBag.it_id = new SelectList(GetIndustryTypes(), "Value", "Text", string.Empty);

            return View(result);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Industry data)
        {
            if (ModelState.IsValid)
            {
                #region 必填

                #endregion

                var checkTaxID = db.industry.Where(x => x.id_tax_id == data.id_tax_id).FirstOrDefault();

                if (checkTaxID != null)
                {
                    Session["msg"] = data.id_tax_id + " 此統一編號已申請";
                    ViewBag.it_id = new SelectList(GetIndustryTypes(), "Value", "Text", string.Empty);

                    return View(data);
                }

                string path = Server.MapPath("~/assets/upload/Industry");

                var passbook = ajax.UploadFile(data.id_passbook, path);
                var lincense = ajax.UploadFile(data.id_license, path);
                var register = ajax.UploadFile(data.id_register, path);

                var insertData = new industry();

                if (insertData != null)
                {
                    insertData.id_passbook = passbook ?? insertData.id_passbook;
                    insertData.id_passbook_name = data.id_passbook != null ? data.id_passbook.FileName : insertData.id_passbook_name;
                    insertData.id_license = lincense ?? insertData.id_license;
                    insertData.id_license_name = data.id_license != null ? data.id_license.FileName : insertData.id_license_name;
                    insertData.id_register = register ?? insertData.id_register;
                    insertData.id_register_name = data.id_register != null ? data.id_register.FileName : insertData.id_register_name;

                    insertData.id_name = data.id_name;
                    insertData.id_tax_id = data.id_tax_id;
                    insertData.id_room = data.id_room;
                    insertData.id_company = data.id_company;
                    insertData.id_it_id = data.id_it_id;
                    insertData.id_email = data.id_email;
                    insertData.id_postal_code = data.id_postal_code;
                    insertData.id_city = data.id_city;
                    insertData.id_address = data.id_address;
                    insertData.id_tel = data.id_tel;
                    insertData.id_fax = data.id_fax;
                    insertData.id_owner = data.id_owner;
                    insertData.id_tel_owner = data.id_tel_owner;
                    insertData.id_owner_area_code = data.id_owner_area_code;
                    insertData.id_extension = data.id_extension;
                    insertData.id_owner_phone = data.id_owner_phone;
                    insertData.id_bank_name = data.BankName;
                    insertData.id_bank_acct_name = data.id_bank_acct_name;
                    insertData.id_owner_email = data.OwnerEmail;
                    insertData.id_bank_code = data.id_bank_code;
                    insertData.id_bank_acct = data.id_bank_acct;

                    db.industry.Add(insertData);
                    db.SaveChanges();

                    user_accounts User = new user_accounts()
                    {
                        ua_acct = insertData.id_tax_id,
                        ua_psw = ajax.ConvertToSHA256(insertData.id_tax_id),
                        ua_perm = 3,
                        ua_user_id = insertData.id_id,
                    };
                    db.user_accounts.Add(User);
                    db.SaveChanges();

                    Session["msg"] = "新增成功";

                    return RedirectToAction("Index");
                }
            }
            ViewBag.it_id = new SelectList(GetIndustryTypes(), "Value", "Text", string.Empty);

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

            var result = new Industry();
            result.id_id = id.Value;
            result.id_address = data.id_address;
            result.id_tel = data.id_tel;
            result.id_fax = data.id_fax;
            result.id_company = data.id_company;
            result.id_tax_id = data.id_tax_id;
            result.id_owner = data.id_owner;
            result.id_tel_owner = data.id_tel_owner;
            result.id_extension = data.id_extension;
            result.id_owner_phone = data.id_owner_phone;
            result.id_room = data.id_room;
            result.id_license_name = data.id_license_name;
            result.id_register_name = data.id_register_name;
            result.id_email = data.id_email;
            result.id_bank_code = data.id_bank_code;
            result.id_bank_acct = data.id_bank_acct;
            result.id_bank_acct_name = data.id_bank_acct_name;
            result.id_passbook_name = data.id_passbook_name;
            result.id_review = data.id_review;
            result.id_city = data.id_city;
            result.id_it_id = data.id_it_id;
            result.id_area_code = data.id_area_code;
            result.id_postal_code = data.id_postal_code;
            result.id_name = data.id_name;
            result.id_owner_area_code = data.id_owner_area_code;
            result.BankName = data.id_bank_name;
            result.OwnerEmail = data.id_owner_email;

            return View(result);
        }

        //modify by v0.8
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Industry data)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        #region 必填

        //        //modify by v0.8
        //        //var checkIndustry = db.industry.Find(Session["UserID"]);
        //        //==============
        //        var checkIndustry = db.industry.Find(data.id_id);
        //        //modify by v0.8

        //        if (string.IsNullOrEmpty(checkIndustry.id_license) && data.id_license == null)
        //        {
        //            Session["msg"] = "請上傳 營業執照或登記證";

        //            data.id_passbook_name = checkIndustry.id_passbook_name;

        //            return View(data);
        //        }

        //        if (string.IsNullOrEmpty(checkIndustry.id_passbook) && data.id_passbook == null)
        //        {
        //            Session["msg"] = "請上傳 銀行存摺";

        //            data.id_license_name = checkIndustry.id_license_name;

        //            return View(data);
        //        }

        //        #endregion

        //        string path = Server.MapPath("~/assets/upload/Industry");

        //        var passbook = ajax.UploadFile(data.id_passbook, path);
        //        var lincense = ajax.UploadFile(data.id_license, path);
        //        var register = ajax.UploadFile(data.id_register, path);

        //        //modify by v0.8
        //        //var updateData = db.industry.Find((int)Session["UserID"]);
        //        //==============
        //        var updateData = db.industry.Find(data.id_id);
        //        //modify by v0.8

        //        if (updateData != null)
        //        {
        //            if (!string.IsNullOrEmpty(passbook)) ajax.DeleteFile($"{path}/{updateData.id_passbook}");
        //            if (!string.IsNullOrEmpty(lincense)) ajax.DeleteFile($"{path}/{updateData.id_license}");
        //            if (!string.IsNullOrEmpty(register)) ajax.DeleteFile($"{path}/{updateData.id_register}");

        //            updateData.id_passbook = passbook ?? updateData.id_passbook;
        //            updateData.id_passbook_name = data.id_passbook != null ? data.id_passbook.FileName : updateData.id_passbook_name;
        //            updateData.id_license = lincense ?? updateData.id_license;
        //            updateData.id_license_name = data.id_license != null ? data.id_license.FileName : updateData.id_license_name;
        //            updateData.id_register = register ?? updateData.id_register;
        //            updateData.id_register_name = data.id_register != null ? data.id_register.FileName : updateData.id_register_name;

        //            updateData.id_bank_acct_name = data.id_bank_acct_name;
        //            updateData.id_bank_code = data.id_bank_code;
        //            updateData.id_owner = data.id_owner;
        //            updateData.id_tel_owner = data.id_tel_owner;
        //            updateData.id_extension = data.id_extension;
        //            updateData.id_owner_phone = data.id_owner_phone;
        //            updateData.id_tax_id = data.id_tax_id;
        //            updateData.id_bank_acct = data.id_bank_acct;
        //            updateData.id_owner_area_code = data.id_owner_area_code;
        //            updateData.id_bank_name = data.BankName;
        //            updateData.id_owner_email = data.OwnerEmail;

        //            db.SaveChanges();
        //            Session["msg"] = "修改成功";

        //            return (int)Session["perm"] < 3 ? RedirectToAction("Index") : RedirectToAction("Edit", new { id = data.id_id });
        //        }            
        //    return View(data);
        //}
        //=================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Industry data)
        {
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var updateData = db.industry.Find(data.id_id);

            if (updateData == null)
            {
                return View(data);
            }

            #region 檔案管理
            
            string path = Server.MapPath("~/assets/upload/Industry");

            Func<HttpPostedFileBase, string, string> HandleFile = (file, existingFilePath) =>
            {
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(existingFilePath))
                    {
                        ajax.DeleteFile($"{path}/{existingFilePath}");
                    }
                    return ajax.UploadFile(file, path);
                }
                return existingFilePath;
            };

            updateData.id_passbook = HandleFile(data.id_passbook, updateData.id_passbook);
            updateData.id_passbook_name=data.id_passbook?.FileName ??updateData.id_passbook_name;

            updateData.id_license = HandleFile(data.id_license, updateData.id_license);
            updateData.id_license_name = data.id_license?.FileName ?? updateData.id_license_name;

            updateData.id_register = HandleFile(data.id_register, updateData.id_register);
            updateData.id_register_name = data.id_register?.FileName ?? updateData.id_register_name;

            #endregion

            #region 必填檔案

            if (string.IsNullOrEmpty(updateData.id_license))
            {
                Session["msg"] = "請上傳 營業執照或登記證";
                return View(data);
            }

            if (string.IsNullOrEmpty(updateData.id_passbook))
            {
                Session["msg"] = "請上傳 銀行存摺";
                return View(data);
            }
            #endregion

            #region 資料上傳

            updateData.id_bank_acct_name = data.id_bank_acct_name;
            updateData.id_bank_code = data.id_bank_code;
            updateData.id_owner = data.id_owner;
            updateData.id_tel_owner = data.id_tel_owner;
            updateData.id_extension = data.id_extension;
            updateData.id_owner_phone = data.id_owner_phone;
            updateData.id_tax_id = data.id_tax_id;
            updateData.id_bank_acct = data.id_bank_acct;
            updateData.id_owner_area_code = data.id_owner_area_code;
            updateData.id_bank_name = data.BankName;
            updateData.id_owner_email = data.OwnerEmail;

            db.SaveChanges();
            Session["msg"] = "修改成功";

            #endregion

            return (int)Session["perm"] < 3 ? RedirectToAction("Index") : RedirectToAction("Edit", new { id = data.id_id });
        }
        //modify by v0.8

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

                                string[] citySplit = { };

                                #region 加入資料

                                if (Row.IndexOf("地址") >= 0)
                                {
                                    string[] cities = { "臺北市", "新北市", "基隆市", "新竹市", "桃園市", "新竹縣", "宜蘭縣", "臺中市", "苗栗縣", "彰化縣", "南投縣", "雲林縣", "高雄市", "臺南市", "嘉義市", "嘉義縣", "屏東縣", "澎湖縣", "花蓮縣", "臺東縣", "金門縣", "連江縣" };
                                    citySplit = row[Row.IndexOf("地址")].Split(cities, StringSplitOptions.RemoveEmptyEntries);
                                }
                                Console.WriteLine(citySplit[0]);
                                Console.WriteLine(citySplit[1]);

                                Data.id_id = Data.id_id > 0 ? Data.id_id : 0;
                                Data.id_it_id = type;
                                Data.id_name = Row.IndexOf("中文名稱") >= 0 ? row[Row.IndexOf("中文名稱")] : Data.id_name;
                                Data.id_postal_code = citySplit[0] != null ? citySplit[0] : "";
                                Data.id_address = citySplit[1] != null ? citySplit[1] : "";
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
                                Data.id_last_time = DateTime.Now;
                                Data.id_email = Row.IndexOf("E-mail") >= 0 ? row[Row.IndexOf("E-mail")] : Data.id_email;
                                Data.id_city = Row.IndexOf("所在縣市") >= 0 ? row[Row.IndexOf("所在縣市")] : Data.id_city;

                                //modify by v0.8 匯入改為新增沒有的資料
                                //db.industry.AddOrUpdate(x => x.id_tax_id, Data);
                                //db.SaveChanges();
                                //=====================================
                                bool inserted = false;
                                var existingData = db.industry.FirstOrDefault(x => x.id_tax_id == Data.id_tax_id);
                                if (existingData == null)
                                {
                                    db.industry.Add(Data);
                                    db.SaveChanges();

                                    inserted = true;
                                }
                                //modify by v0.8 匯入改為新增沒有的資料

                                #endregion 加入資料

                                #region 新增則加入帳號

                                //modify by v0.8 匯入改為新增沒有的資料
                                //if (!Updated)
                                //{
                                //    user_accounts User = new user_accounts()
                                //    {
                                //        ua_acct = TaxId,
                                //        ua_psw = ajax.ConvertToSHA256(TaxId),
                                //        ua_perm = 3,
                                //        ua_user_id = Data.id_id,
                                //    };
                                //    db.user_accounts.Add(User);
                                //    db.SaveChanges();
                                //}
                                //=====================================
                                if (!Updated && inserted)
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
                                //modify by v0.8 匯入改為新增沒有的資料

                                #endregion 新增則加入帳號

                                success++;

                            }
                            catch (Exception)
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

        private List<SelectListItem> GetIndustryTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text="請選擇",Value=""},
                new SelectListItem { Text="觀光旅館",Value="1"},
                new SelectListItem { Text="旅館",Value="2"},
                new SelectListItem { Text="民宿",Value="3"},
            };
        }
    }
}