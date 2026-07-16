using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Dtos
{
    public class UpdateUserVM
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public List<long>? Modules { get; set; }
        public List<long>? AssignedCustomerIds { get; set; }
    }
}
