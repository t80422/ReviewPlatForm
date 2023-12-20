using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Razor.Parser;
using System.Web.WebPages;
using WebApplication1.Controllers;

namespace WebApplication1
{
    public static class Utility
    {
        private static AjaxsController ajax=new AjaxsController();

        public static string UploadAndDeleteFile(HttpPostedFileBase file, string fileName, string path)
        {
            if (file == null) return fileName;

            var newfileName = ajax.UploadFile(file,path)??"";

            if (!string.IsNullOrEmpty(newfileName))
            {
                ajax.DeleteFile($"{path}/{fileName}");
                return newfileName;
            }
            return fileName;
        }

        /// <summary>
        /// 民國日期轉換西元日期
        /// </summary>
        /// <param name="rocDate">格式:yyyMMdd</param>
        /// <param name="acDate"></param>
        /// <returns></returns>
        public static bool ROCDateToACDate(string rocDate, out DateTime acDate)
        {
            rocDate = rocDate.Replace("年", "/").Replace("月","/").Replace("日","/");
            var dateParts=rocDate.Split('/');

            //分解日期
            if (!int.TryParse(dateParts[0], out int rocYear) ||
                !int.TryParse(dateParts[1], out int month) ||
                !int.TryParse(dateParts[2], out int day))
            {
                acDate = default;
                return false;
            }

            //民國年轉換西元年
            int acYear = rocYear + 1911;

            try
            {
                acDate = new DateTime(acYear, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                acDate = default;
                return false;
            }
        }

        /// <summary>
        /// 取得可申請人數
        /// </summary>
        /// <param name="roomCount"></param>
        /// <returns></returns>
        public static double GetEligibleApplicantCount(int roomCount)
        {
            double maxMember = Math.Ceiling((double)roomCount / 8);
            if (maxMember == 0)
                maxMember = 1;
            return maxMember;
        }
    }
}