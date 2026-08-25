using System.Text.Json;
using IotDashboard.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Hubs
{
    [AllowAnonymous]
    public class CameraStreamHub : Hub
    {
        private const string MembershipsKey = "camera-memberships";

        private readonly IDeviceRepository _deviceRepository;

        public CameraStreamHub(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task StartPublisher(long deviceId, byte cameraIndex)
        {
            await EnsureDeviceAsync(deviceId);
            await Groups.AddToGroupAsync(Context.ConnectionId, PubGroup(deviceId, cameraIndex));
            SetMembership(deviceId, cameraIndex, isPublisher: true);
            await Clients.Group(ViewGroup(deviceId, cameraIndex)).SendAsync(
                "PublisherJoined",
                new { deviceId, cameraIndex });
        }

        public Task StartStream(long deviceId, byte cameraIndex) => StartViewer(deviceId, cameraIndex);

        public async Task StartViewer(long deviceId, byte cameraIndex)
        {
            await EnsureDeviceAsync(deviceId);
            await Groups.AddToGroupAsync(Context.ConnectionId, ViewGroup(deviceId, cameraIndex));
            SetMembership(deviceId, cameraIndex, isPublisher: false);
            await RequestOffer(deviceId, cameraIndex);
        }

        public async Task RequestOffer(long deviceId, byte cameraIndex)
        {
            await EnsureDeviceAsync(deviceId);
            if (!IsViewer(deviceId, cameraIndex))
            {
                throw new HubException("Join as viewer first");
            }

            await Clients.Group(PubGroup(deviceId, cameraIndex)).SendAsync(
                "StartRequested",
                new { deviceId, cameraIndex, viewerConnectionId = Context.ConnectionId });
        }

        public async Task Signal(long deviceId, byte cameraIndex, string viewerConnectionId, JsonElement payload)
        {
            await EnsureDeviceAsync(deviceId);
            var membership = GetMembership(deviceId, cameraIndex)
                ?? throw new HubException("Join as publisher or viewer first");

            var message = new { deviceId, cameraIndex, viewerConnectionId, payload };
            if (membership.IsPublisher)
            {
                await Clients.Client(viewerConnectionId).SendAsync("CameraSignal", message);
                return;
            }

            if (viewerConnectionId != Context.ConnectionId)
            {
                throw new HubException("Viewer must signal with own connection id");
            }

            await Clients.Group(PubGroup(deviceId, cameraIndex)).SendAsync("CameraSignal", message);
        }

        public async Task StopStream(long deviceId)
        {
            await NotifyLeftAsync(TakeMemberships(deviceId));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await NotifyLeftAsync(TakeMemberships(null));
            await base.OnDisconnectedAsync(exception);
        }

        private async Task NotifyLeftAsync(List<Membership> removed)
        {
            foreach (var member in removed)
            {
                if (member.IsPublisher)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, PubGroup(member.DeviceId, member.CameraIndex));
                    await Clients.Group(ViewGroup(member.DeviceId, member.CameraIndex)).SendAsync(
                        "PublisherLeft",
                        new { deviceId = member.DeviceId, cameraIndex = member.CameraIndex });
                }
                else
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, ViewGroup(member.DeviceId, member.CameraIndex));
                    await Clients.Group(PubGroup(member.DeviceId, member.CameraIndex)).SendAsync(
                        "StopRequested",
                        new
                        {
                            deviceId = member.DeviceId,
                            cameraIndex = member.CameraIndex,
                            viewerConnectionId = Context.ConnectionId
                        });
                }
            }
        }

        private void SetMembership(long deviceId, byte cameraIndex, bool isPublisher)
        {
            var memberships = Memberships();
            memberships.RemoveAll(x => x.DeviceId == deviceId && x.CameraIndex == cameraIndex);
            memberships.Add(new Membership
            {
                DeviceId = deviceId,
                CameraIndex = cameraIndex,
                IsPublisher = isPublisher
            });
        }

        private bool IsViewer(long deviceId, byte cameraIndex)
        {
            var membership = GetMembership(deviceId, cameraIndex);
            return membership is { IsPublisher: false };
        }

        private Membership? GetMembership(long deviceId, byte cameraIndex)
        {
            return Memberships().FirstOrDefault(x => x.DeviceId == deviceId && x.CameraIndex == cameraIndex);
        }

        private List<Membership> TakeMemberships(long? deviceId)
        {
            var memberships = Memberships();
            var removed = deviceId is null
                ? memberships.ToList()
                : memberships.Where(x => x.DeviceId == deviceId).ToList();
            foreach (var member in removed)
            {
                memberships.Remove(member);
            }

            return removed;
        }

        private List<Membership> Memberships()
        {
            if (Context.Items[MembershipsKey] is List<Membership> memberships)
            {
                return memberships;
            }

            memberships = new List<Membership>();
            Context.Items[MembershipsKey] = memberships;
            return memberships;
        }

        private static string PubGroup(long deviceId, byte cameraIndex) => $"camera-{deviceId}-{cameraIndex}-pub";

        private static string ViewGroup(long deviceId, byte cameraIndex) => $"camera-{deviceId}-{cameraIndex}-view";

        private async Task EnsureDeviceAsync(long deviceId)
        {
            var exists = await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(x => x.Id == deviceId && x.IsActive);

            if (!exists)
            {
                throw new HubException("Device not found");
            }
        }

        private sealed class Membership
        {
            public long DeviceId { get; init; }
            public byte CameraIndex { get; init; }
            public bool IsPublisher { get; init; }
        }
    }
}
