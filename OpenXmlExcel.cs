using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApplication1
{
    public class OpenXmlExcel : IDisposable
    {
        private SpreadsheetDocument _exl;
        private WorkbookPart _wbPart;
        private SharedStringTable _ssTable;
        private Worksheet _ws;
        private Dictionary<string, Cell> _cellDictionary;
        private List<List<string>> _data = new List<List<string>>();

        public Worksheet Worksheet
        {
            get { return _ws; }
        }

        /// <summary>
        /// 讀取Excel
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="isEditable"></param>
        public OpenXmlExcel(Stream stream, bool isEditable)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            try
            {
                _exl = SpreadsheetDocument.Open(stream, isEditable);
                _wbPart = _exl.WorkbookPart;
                _ssTable = _wbPart.SharedStringTablePart.SharedStringTable;
                var wsPart = (WorksheetPart)_wbPart.GetPartById(_wbPart.Workbook.Descendants<Sheet>().First().Id);
                _ws = wsPart.Worksheet;
                //將儲存格塞到字典加快搜尋速度
                _cellDictionary = _ws.Descendants<Cell>().GroupBy(x => x.CellReference.Value).ToDictionary(g => g.Key, g => g.First());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public OpenXmlExcel() { }

        public void SaveToExcel(MemoryStream memoryStream)
        {
            // 使用 MemoryStream 創建 SpreadsheetDocument
            using (_exl = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
            {
                // 添加 WorkbookPart 和 Workbook
                var workbookPart = _exl.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // 添加 WorksheetPart 和 Worksheet
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // 添加 Sheets 和 Sheet
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = _exl.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sheet1"
                };
                sheets.Append(sheet);

                // 将数据写入工作表
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                for (int i = 0; i < _data.Count; i++)
                {
                    var row = new Row { RowIndex = (uint)(i + 1) };
                    sheetData.Append(row);

                    for (int j = 0; j < _data[i].Count; j++)
                    {
                        var cell = new Cell { CellReference = GetCellAddress(i + 1, j + 1) };
                        cell.CellValue = new CellValue(_data[i][j]);
                        cell.DataType = CellValues.String;
                        row.Append(cell);
                    }
                }

                // 保存更改
                worksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
            }
        }

        public void Dispose()
        {
            if (_exl != null)
                _exl.Dispose();
        }

        public string GetCellValue(int rowIndex, int columnIndex)
        {
            var addressName = GetCellAddress(rowIndex, columnIndex);

            if (!_cellDictionary.TryGetValue(addressName, out Cell cell))
            {
                return null;
            }

            if (string.IsNullOrEmpty(cell.InnerText))
            {
                return null;
            }

            string value = null;

            if (cell.InnerText.Length > 0)
            {
                value = cell.InnerText;

                if (cell.DataType != null)
                {
                    switch (cell.DataType.Value)
                    {
                        case CellValues.SharedString:
                            value = _ssTable.ElementAt(int.Parse(value)).InnerText;
                            //過濾注音符號
                            value = Regex.Replace(value, @"[\u3100-\u312F\u31A0-\u31BF]", string.Empty);
                            break;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// 寫入儲存格
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="text"></param>
        public void WriteToCell(int rowIndex, int columnIndex, string text)
        {
            while (_data.Count <= rowIndex)
            {
                _data.Add(new List<string>());
            }

            var row = _data[rowIndex];
            while (row.Count <= columnIndex)
            {
                row.Add(null);
            }

            row[columnIndex] = text;
        }

        private string GetCellAddress(int rowIndex, int columnIndex)
        {
            // 將列索引轉換為字母
            var columnName = ConvertColumnIndexToColumnName(columnIndex);
            // 組合為完整地址
            return $"{columnName}{rowIndex}";
        }

        private string ConvertColumnIndexToColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}