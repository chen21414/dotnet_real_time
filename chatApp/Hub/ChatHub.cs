using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService;
using Microsoft.AspNetCore.SignalR;


namespace ChatService.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat Bot";
            _connections = connections;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");

                SendConnectedUsers(userConnection.Room);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                //above returns true or false
                //if there's a value with that connection id as the key, it's going to get userConnection as variable

                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }

        //the link we connect to frontend
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

            _connections[Context.ConnectionId] = userConnection;

            //instead of Clients.all, we want specific group we want
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,
            $"{userConnection.User} has joined {userConnection.Room}"
            );//method: ReceiveMessage

            await SendConnectedUsers(userConnection.Room);
        }

        public Task SendConnectedUsers(string room)
        {
            var users = _connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);

            return Clients.Group(room).SendAsync("UserInRoom", users);
        }
    }
}


// What is IDictionary C#?
// The IDictionary<TKey,TValue> interface is the base interface for generic collections of key/value pairs. Each element is a key/value pair stored in a KeyValuePair<TKey,TValue> object. 
// Each pair must have a unique key. Implementations can vary in whether they allow key to be null .

//Context class
// NET Framework finds a compatible or creates a new instance of the Context class for the object. Once an object is placed in a context, it stays in it for life. 
// Classes that can be bound to a context are called context-bound classes.


// What does TryGetValue do in c#?
// TryGetValue Method: This method combines the functionality of the ContainsKey method and the Item property. If the key is not found, 
// then the value parameter gets the appropriate default value for the value type TValue; for example, 0 (zero) for integer types, false for Boolean types, and null for reference types.


//out parameter modifier (C# Reference)
// The out keyword causes arguments to be passed by reference. It makes the formal parameter an alias for the argument, which must be a variable. In other words, 
// any operation on the parameter is made on the argument. It is like the ref keyword, except that ref requires that the variable be initialized before it is passed. It is also like the in keyword, 
// except that in does not allow the called method to modify the argument value. To use an out parameter, both the method definition and the calling method must explicitly use the out keyword. For example:
// int initializeInMethod;
// OutArgExample(out initializeInMethod);
// Console.WriteLine(initializeInMethod);     // value is now 44

// void OutArgExample(out int number)
// {
//     number = 44;
// }