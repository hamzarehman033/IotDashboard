using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Domain.Entities
{
    public class User : IdentityUser<long>
    {
        public User()
        {
            RefreshToken = string.Empty;
        }
        public string RefreshToken { get; set; }
            public long? CustomerId { get; set; }

        public ICollection<UserToken> Tokens { get; } = new List<UserToken>();
            public Customer? Customer { get; set; }
    }
}
