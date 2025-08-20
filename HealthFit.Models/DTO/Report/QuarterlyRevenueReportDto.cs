using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthFit.Models.DTO.Report
{
    public class QuarterlyRevenueReportDto
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }
}
