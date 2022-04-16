using autobus_complete.Models;
using Microsoft.AspNetCore.SignalR;

namespace autobus_complete.Hubs
{
    public class GameHub:Hub
    {
        private static List<User> _users = new List<User>();
        private static List<Group> _groups = new List<Group>();


        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("GoToLobby");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

           
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            
            _groups.ForEach(group =>
            {
                if (group.Users.Contains(user))
                {
                    group.Users.Remove(user);  
                }
                
            });

            foreach (var group in _groups.ToList())
            {
                if(group.Users.Count == 0)
                {
                    _groups.Remove(group);
                }
                
            }

            await removeUser();
            await GetAllGroups();
            await GetRoomUsers();
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

        public string GetRoomName()
        {
            string roomName = "";
            foreach (var group in _groups)
            {
                foreach (var user in group.Users)
                {
                    if (user.ConnectionId == Context.ConnectionId)
                    {
                        roomName = group.Name;
                    }
                }
            }
            return roomName;
        }

        public async Task StartGame()
        {
            string roomName = GetRoomName();
            await Clients.Group(roomName).SendAsync("StartGame");
            //await Clients.All.SendAsync("StartGame");
        }

        public async Task AutobusComplete()
        {
            string roomName = GetRoomName();
            await Clients.Group(roomName).SendAsync("AutobusComplete");
            //await Clients.All.SendAsync("AutobusComplete");
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

            string roomName = GetRoomName();
            await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);


        }

        public async Task AddGroup(string GroupName,string OwnerName)
        {

            _groups.Add(new Group(GroupName));
            Group currentGroup = _groups.FirstOrDefault(x => x.Name == GroupName);

            // adding user to user list
            await AddUser(OwnerName);
            User user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            // checking if the user already exists in another room and if true remove him
            _groups.ForEach(group =>
            {
                foreach (var user in group.Users.ToList())
                {
                    if (user.ConnectionId == Context.ConnectionId)
                    {
                        group.Users.Remove(user);
                        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
                    }

                }              
            });

            // adding the user to current room
            currentGroup.Users.Add(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);

            // if there is any room empty remove it from the list
            foreach (var room in _groups.ToList())
            {
                if (room.Users.Count == 0)
                {
                    _groups.Remove(room);
                }

            }

            
            await GetAllGroups();
            await GetCurrentUser();
            await GetRoomUsers();
        }

        public async Task GetAllGroups()
        {
            await Clients.All.SendAsync("GetAllGroups", _groups);
        }

        public async Task JoinRoom(string roomName,string userName)
        {
            
            Group currentGroup = _groups.FirstOrDefault(x => x.Name == roomName);
            User userInUsers = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (userInUsers == null)
            {
                await AddUser(userName);
                await GetCurrentUser();
            }
            else
            {
                int index = _users.FindIndex(x => x.ConnectionId == userInUsers.ConnectionId);
                if (index != -1)
                {
                    _users[index].Name = userName;
                }
                await GetCurrentUser();
            }

            // checking if the user already exists in another room and if true remove him
            _groups.ForEach(group =>
            {
                foreach (var user in group.Users.ToList())
                {
                    if (user.ConnectionId == Context.ConnectionId)
                    {
                        group.Users.Remove(user); 
                        Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
                    }

                }
            });

            User userInCurrentGroup = currentGroup.Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if(userInCurrentGroup == null)
            {
                User user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                currentGroup.Users.Add(user);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            }
            else
            {
                int index = currentGroup.Users.FindIndex(x => x.ConnectionId == userInUsers.ConnectionId);
                if (index != -1)
                {
                    _users[index].Name = userName;
                   
                }
            }

            // if there is any room empty remove it from the list
            foreach (var room in _groups.ToList())
            {
                if (room.Users.Count == 0)
                {
                    _groups.Remove(room);
                }

            }

            await GetCurrentUser();
            await GetAllGroups();
            await GetAllUsers();
            await GetRoomUsers();

        }

        public async Task GetRoomUsers()
        {
            string roomName = GetRoomName();
            Group currentGroup = _groups.FirstOrDefault(x => x.Name == roomName);

            List<User> roomUsers = new List<User>();
            if (currentGroup == null)
            {
               roomUsers = new List<User>();
            }
            else
            {
                roomUsers = currentGroup.Users;

            }
             
            await Clients.Group(roomName).SendAsync("RoomUsers", roomUsers);
        }
    }
}
