using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Parser;
using WebApplication1.Models;
using WebApplication1.Models.Industry;

namespace WebApplication1.Controllers
{
    [IsLogin]
    public class IndustryController : Controller
    {
        private AjaxsController ajax = new AjaxsController();
        private ReviewPlatformEntities db = new ReviewPlatformEntities();

        /// <summary>
        /// 列表頁
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <param name="review">審核狀態</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Index(string key, int? page = 1)
        {
            var query = db.industry.AsEnumerable();

            #region 搜尋

            //if (!string.IsNullOrWhiteSpace(key))            
            //    query = query.Where(x => x.id_name.Contains(key));
            //    
            if (!string.IsNullOrWhiteSpace(key))
            {
                query = query.Where(x => (x.id_name != null && x.id_name.Contains(key)) ||
                                         (x.id_tax_id != null && x.id_tax_id.Contains(key)) ||
                                         (x.id_company != null && x.id_company.Contains(key)));
            }

            #endregion

            var data = new IndustryViewModel.Index();
            data.Industries = query.OrderByDescending(x => x.id_id).ToPagedList((int)page, 10);

            return View(data);
        }

        public ActionResult Create()
        {
            //var result = new Industry();

            //ViewBag.it_id = new SelectList(GetIndustryTypes(), "Value", "Text", string.Empty);

            //return View(result);

            var model = new IndustryViewModel.IndustryModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IndustryViewModel.IndustryModel data)
        {
            if (!ModelState.IsValid)
                return View(data);

            var checkTaxID = db.industry.Where(x => x.id_tax_id == data.Industry.id_tax_id).FirstOrDefault();

            if (checkTaxID != null)
            {
                Session["msg"] = data.Industry.id_tax_id + " 此統一編號已申請";
                return View(data);
            }
            
            var insertData = new industry();

            HandleFileUploads(data, insertData, Server.MapPath("~/assets/upload/Industry"));

            insertData.id_name = data.Industry.id_name;
            insertData.id_tax_id = data.Industry.id_tax_id;
            insertData.id_room = data.Industry.id_room;
            insertData.id_company = data.Industry.id_company;
            insertData.id_it_id = data.Industry.id_it_id;
            insertData.id_email = data.Industry.id_email;
            insertData.id_postal_code = data.Industry.id_postal_code;
            insertData.id_city = data.Industry.id_city;
            insertData.id_address = data.Industry.id_address;
            insertData.id_tel = data.Industry.id_tel;
            insertData.id_fax = data.Industry.id_fax;
            insertData.id_owner = data.Industry.id_owner;
            insertData.id_tel_owner = data.Industry.id_tel_owner;
            insertData.id_owner_area_code = data.Industry.id_owner_area_code;
            insertData.id_extension = data.Industry.id_extension;
            insertData.id_owner_phone = data.Industry.id_owner_phone;
            insertData.id_bank_name = data.Industry.id_bank_name;
            insertData.id_bank_acct_name = data.Industry.id_bank_acct_name;
            insertData.id_owner_email = data.Industry.id_owner_email;
            insertData.id_bank_code = data.Industry.id_bank_code;
            insertData.id_bank_acct = data.Industry.id_bank_acct;
            insertData.id_business_status = data.Industry.id_business_status;
            insertData.id_operator = data.Industry.id_operator;

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

            var result = new IndustryViewModel.IndustryModel();
            result.Industry = data;

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IndustryViewModel.IndustryModel data)
        {
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var updateData = db.industry.Find(data.Industry.id_id);

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

            updateData.id_passbook = HandleFile(data.PassBookFile, updateData.id_passbook);
            updateData.id_passbook_name = data.PassBookFile?.FileName ?? updateData.id_passbook_name;

            updateData.id_license = HandleFile(data.LicenseFile, updateData.id_license);
            updateData.id_license_name = data.LicenseFile?.FileName ?? updateData.id_license_name;

            updateData.id_register = HandleFile(data.RegisterFile, updateData.id_register);
            updateData.id_register_name = data.RegisterFile?.FileName ?? updateData.id_register_name;

            #endregion

            #region 資料上傳

            updateData.id_bank_acct_name = data.Industry.id_bank_acct_name;
            updateData.id_bank_code = data.Industry.id_bank_code;
            updateData.id_owner = data.Industry.id_owner;
            updateData.id_tel_owner = data.Industry.id_tel_owner;
            updateData.id_extension = data.Industry.id_extension;
            updateData.id_owner_phone = data.Industry.id_owner_phone;
            updateData.id_tax_id = data.Industry.id_tax_id;
            updateData.id_bank_acct = data.Industry.id_bank_acct;
            updateData.id_owner_area_code = data.Industry.id_owner_area_code;
            updateData.id_bank_name = data.Industry.id_bank_name;
            updateData.id_owner_email = data.Industry.id_owner_email;
            updateData.id_business_status = data.Industry.id_business_status;
            updateData.id_operator = data.Industry.id_operator;

            db.SaveChanges();
            Session["msg"] = "修改成功";

            #endregion

            return (int)Session["perm"] < 3 ? RedirectToAction("Index") : RedirectToAction("Edit", new { id = data.Industry.id_id });
        }

        public ActionResult Edit_Manager(int industryID)
        {
            return View(GetIndustryData(industryID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Manager(IndustryViewModel.IndustryModel data)
        {
            if (!ModelState.IsValid)
            {
                return View(GetIndustryData(data.Industry.id_id));
            }

            var updateData = db.industry.Find(data.Industry.id_id);

            if (updateData == null)
            {
                return View(GetIndustryData(data.Industry.id_id));
            }

            string path = Server.MapPath("~/assets/upload/Industry");

            HandleFileUploads(data, updateData, path);

            #region 資料上傳

            updateData.id_name = data.Industry.id_name;
            updateData.id_tax_id = data.Industry.id_tax_id;
            updateData.id_room = data.Industry.id_room;
            updateData.id_company = data.Industry.id_company;
            updateData.id_it_id = data.Industry.id_it_id;
            updateData.id_email = data.Industry.id_email;
            updateData.id_postal_code = data.Industry.id_postal_code;
            updateData.id_city = data.Industry.id_city;
            updateData.id_address = data.Industry.id_address;
            updateData.id_tel = data.Industry.id_tel;
            updateData.id_fax = data.Industry.id_fax;
            updateData.id_owner = data.Industry.id_owner;
            updateData.id_review_passbook = data.Industry.id_review_passbook;
            updateData.id_review_register = data.Industry.id_review_register;
            updateData.id_review_license = data.Industry.id_review_license;
            updateData.id_owner_email = data.Industry.id_owner_email;
            updateData.id_owner_area_code = data.Industry.id_owner_area_code;
            updateData.id_tel_owner = data.Industry.id_tel_owner;
            updateData.id_extension = data.Industry.id_extension;
            updateData.id_owner_phone = data.Industry.id_owner_phone;
            updateData.id_bank_name = data.Industry.id_bank_name;
            updateData.id_bank_acct_name = data.Industry.id_bank_acct_name;
            updateData.id_bank_code = data.Industry.id_bank_code;
            updateData.id_bank_acct = data.Industry.id_bank_acct;
            updateData.id_business_status = data.Industry.id_business_status;
            updateData.id_operator = data.Industry.id_operator;

            #region 審核狀態

            if (data.Industry.id_review_passbook == "退件" || data.Industry.id_review_register == "退件" || data.Industry.id_review_license == "退件")
            {
                updateData.id_review = "退件";
            }
            else if (data.Industry.id_review_passbook == "待補件" || data.Industry.id_review_register == "待補件" || data.Industry.id_review_license == "待補件")
            {
                updateData.id_review = "待補件";
            }
            else if (data.Industry.id_review_passbook == "審核中" || data.Industry.id_review_register == "審核中" || data.Industry.id_review_license == "審核中")
            {
                updateData.id_review = "審核中";
            }
            else if (data.Industry.id_review_passbook == "審核完成" && data.Industry.id_review_register == "審核完成" && data.Industry.id_review_license == "審核完成")
            {
                updateData.id_review = "審核完成";
            }
            else
            {
                updateData.id_review = "待審核";
            }

            #endregion

            #region 更新審核人員

            var perm = (int)Session["perm"];
            var managerID = (int)Session["UserID"];

            switch (perm)
            {
                case 0:
                    updateData.id_mg_id_association = managerID;
                    break;

                case 1:
                    updateData.id_mg_id_snd = managerID;
                    break;

                case 2:
                    updateData.id_mg_id_fst = managerID;
                    break;

                default:
                    break;
            }

            #endregion

            db.SaveChanges();
            Session["msg"] = "存檔成功";

            #endregion

            if ((int)Session["perm"] == 2)
            {
                return RedirectToAction("Index", "Subsidy", new { key = data.Industry.id_name });
            }
            else
            {
                return RedirectToAction("Index");
            }
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

        private IndustryViewModel.IndustryModel GetIndustryData(int industryID)
        {
            var data = new IndustryViewModel.IndustryModel();

            data.Industry = db.industry.Find(industryID);
            data.InitialReviewer = db.manager.Find(data.Industry.id_mg_id_fst);
            data.SecondaryReviewer = db.manager.Find(data.Industry.id_mg_id_snd);
            data.AssociationReviewer = db.manager.Find(data.Industry.id_mg_id_association);

            return data;
        }

        private void HandleFileUploads(IndustryViewModel.IndustryModel data, industry updateData, string path)
        {
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

            updateData.id_passbook = HandleFile(data.PassBookFile, updateData.id_passbook);
            updateData.id_passbook_name = data.PassBookFile?.FileName ?? updateData.id_passbook_name;

            updateData.id_license = HandleFile(data.LicenseFile, updateData.id_license);
            updateData.id_license_name = data.LicenseFile?.FileName ?? updateData.id_license_name;

            updateData.id_register = HandleFile(data.RegisterFile, updateData.id_register);
            updateData.id_register_name = data.RegisterFile?.FileName ?? updateData.id_register_name;
        }
    }
}
