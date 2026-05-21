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
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}