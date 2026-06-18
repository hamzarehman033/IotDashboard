using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Dtos
{
    public class UserVM
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public long? CustomerId { get; set; }
        public List<long> Modules { get; set; } = new List<long>();
        public List<string> Roles { get; set; } = new List<string>();
    }
}
