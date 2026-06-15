namespace IotDashboard.Domain.Entities
{
    public static class RoleNames
    {
        public const string SysAdmin = "SysAdmin";
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Technician = "Technician";
        public const string Viewer = "Viewer";

        public static readonly string[] All = new[]
        {
            SysAdmin,
            Admin,
            User,
            Technician,
            Viewer
        };
    }
}
