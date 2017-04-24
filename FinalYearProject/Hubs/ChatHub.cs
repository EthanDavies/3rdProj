using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FinalYearProject.Hubs
{
    public class ChatHub : Hub
    {
        static List<string> ConnectedUsers = new List<string>();

        public void Send(string name, string message)
        {
            Clients.All.addNewMessageToPage(name, message);

        }

       public void Joined(string name)
        {
            Clients.All.addNewMessageToPage(name, "has joined the chat room");
        }
    }
}