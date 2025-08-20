using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HealthFit.Models.ViewModels
{
    public class ProductVM
    {
       public Product Product { get; set; }
        public IEnumerable<SelectListItem> selectListItems { get; set; } 

    }
}
