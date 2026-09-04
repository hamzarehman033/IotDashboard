using IotDashboard.Application.Dtos;

namespace IotDashboard.Api.Services
{
    public interface IChatService
    {
        Task<Response<ChatReplyVM>> AskAsync(ChatRequestVM request, CancellationToken cancellationToken = default);
    }
}
