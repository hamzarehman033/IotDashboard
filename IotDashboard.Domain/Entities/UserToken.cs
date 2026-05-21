using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Domain.Entities
{
    public class UserToken : IdentityUserToken<long>
    {
        public int TokenTries { get; set; }
        public DateTime ExpiredDateTimeUTC { get; set; }
    }
}
