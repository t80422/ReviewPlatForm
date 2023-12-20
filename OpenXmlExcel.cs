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
        private Dictionary<string,Cell> _cellDictionary;

        public Worksheet Worksheet 
        {
            get { return _ws; } 
        }

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

        public void Dispose()
        {
            _exl.Dispose();
        }

        public string GetCellValue(int rowIndex, int columnIndex)
        {
            var addressName= GetCellAddress(rowIndex, columnIndex);

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