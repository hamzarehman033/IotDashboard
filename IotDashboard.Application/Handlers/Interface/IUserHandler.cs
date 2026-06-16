using IotDashboard.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IUserHandler
    {
        Task<Response<TokenVM>> GetToken(LoginVM Model);
        Task<Response<TokenVM>> GetTokenByRefresh(string refreshToken);
        Task<Response<string>> Register(RegisterVM model);
        Task<Response<string>> CreateUser(CreateUserVM model);
        Task<Response<string>> ChangePassword(long userId, ChangePasswordVM model);
        Task<Response<string>> GetForgotPasswordToken(string emailorPhone);
        Task<Response<string>> ResetPassword(ResetPasswordVM model);
        Task<Response<List<object>>> GetAllAsync();
        Task<Response<UserVM>> GetById(long userId);
        Task<Response<string>> UpdateUser(long userId, UpdateUserVM model);
        Task<Response<string>> DeleteUser(long userId);
    }
}
