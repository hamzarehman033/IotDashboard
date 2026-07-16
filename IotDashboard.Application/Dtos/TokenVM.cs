using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Dtos
{
    public class TokenVM
    {
        public TokenVM()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
            Name = string.Empty;
            Roles = new List<string>();
            Modules = new List<long>();
            AssignedCustomerIds = new List<long>();
        }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Name { get; set; }
        public long? CustomerId { get; set; }
        public List<string> Roles { get; set; }
        public List<long> Modules { get; set; }
        public List<long> AssignedCustomerIds { get; set; }
    }
}
