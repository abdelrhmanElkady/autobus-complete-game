using autobus_complete.Models;
using Microsoft.AspNetCore.SignalR;

namespace autobus_complete.Hubs
{
    public class GameHub:Hub
    {
        private static List<User> _users = new List<User>();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            await removeUser();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddUser(string name)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null)
            {
                User newUser = new User(Context.ConnectionId, name);

                _users.Add(newUser);
            }
            else
            {
                int index = _users.FindIndex(x => x.ConnectionId == user.ConnectionId);
                if (index != -1)
                {
                    _users[index].Name = name;
                }
            }


            await GetCurrentUser();
            await GetAllUsers();

        }

        private async Task removeUser()
        {
            User user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (user != null)
            {
                _users.Remove(user);
            }
            await GetAllUsers();
        }

        public async Task GetCurrentUser()
        {
            User currentUser = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            await Clients.Client(Context.ConnectionId).SendAsync("GetCurrentUser", currentUser);
        }

        public async Task GetAllUsers()
        {
            await Clients.All.SendAsync("GetAllUsers", _users);
        }

        public async Task StartGame()
        {
            await Clients.All.SendAsync("StartGame");
        }

        public async Task AutobusComplete()
        {
            await Clients.All.SendAsync("AutobusComplete");
        }

        public async Task SetResult(Answer answer)
        {

            User currentUser = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            int index = _users.FindIndex(x => x.ConnectionId == currentUser.ConnectionId);
            if (index != -1)
            {
                _users[index].Answer = answer;
            }
        }

        public async Task SetScore(int score)
        {
            User currentUser = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            int index = _users.FindIndex(x => x.ConnectionId == currentUser.ConnectionId);
            if (index != -1)
            {
                _users[index].Score += score;
            }
        }



        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);

        }
    }
}
