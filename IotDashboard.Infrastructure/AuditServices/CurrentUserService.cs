using IotDashboard.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            if (!long.TryParse(customerId, out var activeCustomerId) || activeCustomerId <= 0)
            {
                return 0;
            }

            var user = _context.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return activeCustomerId;
            }

            if (user.IsInRole(RoleNames.SysAdmin))
            {
                return activeCustomerId;
            }

            var primaryCustomerId = GetLongClaim(user, "customerId");
            if (!user.IsInRole(RoleNames.Manager))
            {
                return primaryCustomerId == activeCustomerId ? activeCustomerId : 0;
            }

            var assignedCustomerIds = user.Claims
                .Where(x => x.Type == "assignedCustomerId")
                .Select(x => long.TryParse(x.Value, out var id) ? id : 0)
                .Where(x => x > 0)
                .ToHashSet();

            return assignedCustomerIds.Contains(activeCustomerId) ? activeCustomerId : 0;
        }

        private static long GetLongClaim(ClaimsPrincipal user, string claimType)
        {
            var claim = user.Claims.FirstOrDefault(x => x.Type == claimType);
            return long.TryParse(claim?.Value, out var id) ? id : 0;
        }
    }
}
