using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalYearProject.Models
{
    /// <summary>
    /// This model class is used to organize data for the chat room
    /// </summary>
    public class ChatModel
    {

        /// <summary>
        /// Users that have connected to the chat
        /// </summary>
        public List<ChatUser> Users;

        /// <summary>
        /// Messages by the users
        /// </summary>
        public List<ChatMessage> ChatHistory;

        /// <summary>
        /// Creates the data required for the chat room
        /// </summary>
        public ChatModel()
        {
            Users = new List<ChatUser>();
            ChatHistory = new List<ChatMessage>();

            ChatHistory.Add(new ChatMessage()
            {
                Message = "The chat server started at " + DateTime.Now
            });
        }

        /// <summary>
        /// The data used to make up a chat user
        /// </summary>
        public class ChatUser
        {
            public string NickName;
            public DateTime LoggedOnTime;
            public DateTime LastPing;
        }

        public class ChatMessage
        {
            /// <summary>
            /// If null, the message is from the server
            /// </summary>
            public ChatUser ByUser;

            public DateTime When = DateTime.Now;

            public string Message = "";

        }
    }
}