using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WebApplication1.Controllers
{
    public class AjaxsController : Controller
    {
        // GET: Ajaxs
        public string UploadFile (HttpPostedFileBase file , string path)
        {

            if (file != null && file.ContentLength > 0 && !String.IsNullOrEmpty(path))
            {
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                // 副檔名
                string extension = Path.GetExtension(file.FileName);
                // 儲存的檔案名稱
                string fileName = $"{Guid.NewGuid()}{extension}";
                // 儲存路徑
                string savePath = Path.Combine(path,fileName);
                file.SaveAs(savePath);

                return fileName;
            }

            return null;
        }

        public bool DeleteFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return true;
            }
            return false;
        }

        public Dictionary<string, List<List<string>>> ReadFile(HttpPostedFileBase file)
        {
            Dictionary<string, List<List<string>>> result = new Dictionary<string, List<List<string>>>();


            if (file != null && file.ContentLength > 0)
            {
                Regex regex = new Regex("[A-Z]");

                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(file.InputStream, false))
                {
                    foreach (Sheet sheet in spreadsheet.WorkbookPart.Workbook.Sheets)
                    {
                        Worksheet worksheet = (spreadsheet.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                        
                        IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                        var data2 = new List<List<string>>();

                        int MaxCount = 0;

                        foreach (Row row in rows)
                        {
                            
                            
                            // 以第一列標題作為最大欄位數
                            if(row.RowIndex.Value == 1)
                            {                                
                                MaxCount = row.Descendants<Cell>().Count();
                            }
                            List<string> data = new List<string>();
                            int i = 0;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                //string Col = regex.Matches(cell.CellReference.ToString()).ToString();
                                string Col = "";
                                foreach(var match in regex.Matches(cell.CellReference.ToString())) Col += match;
                                i++;
                                int intCol = (int)GetAllBytes(Col);
                                while (i < intCol)
                                {
                                    data.Add("");
                                    i++;
                                }
                                data.Add(GetCellValue(spreadsheet, cell) ?? "");
                            }
                            if (i < MaxCount)
                            {
                                for (var j = i; j < MaxCount; j++)
                                {
                                    data.Add("");
                                }
                            }
                            data2.Add(data);
                        }

                        result.Add(sheet.Name, data2);
                    }

                    return result;
                }
            }
            return null;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue?.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        public double GetAllBytes(string letter)
        {
            double resultByte = 0;
            if(!String.IsNullOrEmpty(letter))
            {
                byte[] BytesArray = Encoding.ASCII.GetBytes(letter);
                Array.Reverse(BytesArray);
                for(var i = 0; i < BytesArray.Length; i++)
                {
                    resultByte += (BytesArray[i] - 65 + 1) * Math.Pow(26, i);
                }

            }
            return resultByte;
        }

        // 轉換雜湊
        public string ConvertToSHA256(string text)
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}