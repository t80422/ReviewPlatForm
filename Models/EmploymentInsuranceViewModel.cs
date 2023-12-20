using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class EmploymentInsuranceViewModel
    {
        public List<employment_insurance> Employments;
        public industry Industry { get; set; }
        public string IDCard { get; set; }
    }
}