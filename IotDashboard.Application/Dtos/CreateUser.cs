namespace IotDashboard.Application.Dtos
{
    public class CreateUserVM
    {
        public CreateUserVM()
        {
            UserName = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            Role = string.Empty;
            Modules = new List<long>();
            AssignedCustomerIds = new List<long>();
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public long? CustomerId { get; set; }
        public List<long> Modules { get; set; }
        public List<long> AssignedCustomerIds { get; set; }
    }
}