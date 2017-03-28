using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FinalYearProject
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message)
        {
            //Call the broadcast method to update the clients.
            Clients.All.broadcastMessage(name, message);
        }
    }
}