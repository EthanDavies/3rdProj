using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FinalYearProject.Hubs
{
    /// <summary>
    /// This is the SignalR hub that is required in order for the chat room to work
    /// </summary>
    public class ChatHub : Hub
    {
        static List<string> ConnectedUsers = new List<string>();

        /// <summary>
        /// This hub method is responsible for sending a message to the chat room, 
        /// and all its connected users
        /// </summary>
        /// <param name="name">this is the name of the sender</param>
        /// <param name="message">this is the senders message</param>
        public void Send(string name, string message)
        {
            Clients.All.addNewMessageToPage( name, message);

        }

        /// <summary>
        /// this method is used to signal that a new user has joined the chat room
        /// </summary>
        /// <param name="name">string of the user that has joined</param>
       public void Joined(string name)
        {
            Clients.All.addNewMessageToPage(name, "has joined the chat room");
        }
    }
}