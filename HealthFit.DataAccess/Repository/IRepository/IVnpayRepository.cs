using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthFit.Models;
using HealthFit.Models.Models;

namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface IVnpayRepository : IRepository<Payment>
    {
        void Update(Payment obj);
    }
}
