using FinalYearProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalYearProject.App_Start
{
    public class Connections
    {
        public static void PutUsersOffline()
        {
            using (var db = new ChatContext())
            {
                foreach (var connection in db.Connections)
                    connection.IsOnline = false;

                db.SaveChanges();
            }
        }
    }
}