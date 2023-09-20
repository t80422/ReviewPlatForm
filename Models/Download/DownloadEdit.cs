using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Download
{
    public class DownloadEdit
    {
        public int id { get; set; }
        public int type { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string filename1 { get; set; }
        public HttpPostedFileBase file1 { get; set; }
        public string filename2 { get; set; }
        public HttpPostedFileBase file2 { get; set; }
        public string filename3 { get; set; }
        public HttpPostedFileBase file3 { get; set; }
        public string filename4 { get; set; }
        public HttpPostedFileBase file4 { get; set; }
        public string filename5 { get; set; }
        public HttpPostedFileBase file5 { get; set; }
        public bool visible { get; set; }

    }
}