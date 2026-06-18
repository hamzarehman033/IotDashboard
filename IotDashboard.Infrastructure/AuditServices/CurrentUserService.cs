using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Infrastructure.AuditServices
{
    internal class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _context;
        public CurrentUserService(IHttpContextAccessor context)
        {
            _context = context;
        }
        public long GetLoggedInUserId()
        {
            var claim = _context.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.ToLower().Equals("id"));
            string id = claim == null ? "0" : claim.Value;
            return Convert.ToInt64(id);
        }
        public long GetCustomerId()
        {
            var customerId = _context.HttpContext?.Request?.Headers["X-Customer-Id"].FirstOrDefault();
            string id = customerId == null ? "0" : customerId;
            return Convert.ToInt64(id);
        }
    }
}
