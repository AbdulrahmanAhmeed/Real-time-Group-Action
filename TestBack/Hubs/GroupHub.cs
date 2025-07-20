using Microsoft.AspNetCore.SignalR;

namespace TestBack.Hubs
{
    public class GroupHub : Hub
    {
        private static readonly List<string> _availableGroups = new()
        {
            "GroupA",
            "GroupB", 
            "GroupC",
            "GroupD"
        };

        public async Task<List<string>> GetGroups()
        {
            return await Task.FromResult(_availableGroups);
        }

        public async Task JoinGroup(string groupName, string userName)
        {
            if (!_availableGroups.Contains(groupName))
            {
                await Clients.Caller.SendAsync("Error", $"Group '{groupName}' does not exist.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Context.Items["UserName"] = userName;
            Context.Items["GroupName"] = groupName;

            await Clients.Group(groupName).SendAsync("UserJoined", $"{userName} has joined the group.");
        }

        public async Task SendAction(string groupName, string userName, string actionName)
        {
            if (!_availableGroups.Contains(groupName))
            {
                await Clients.Caller.SendAsync("Error", $"Group '{groupName}' does not exist.");
                return;
            }

            await Clients.Group(groupName).SendAsync("ActionPerformed", $"{userName} pressed {actionName}.");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("UserName", out var userNameObj) && 
                Context.Items.TryGetValue("GroupName", out var groupNameObj))
            {
                var userName = userNameObj?.ToString();
                var groupName = groupNameObj?.ToString();

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(groupName))
                {
                    await Clients.Group(groupName).SendAsync("UserLeft", $"{userName} has left the group.");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
} 