using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using FinalYearProject.Models;

namespace FinalYearProject.Controllers
{
    /// <summary>
    /// Controller that contains the methods used by the home views
    /// </summary>
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// method is responsible for returning the index home view
        /// </summary>
        /// <returns>returns the index home view</returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

        /// <summary>
        /// This controller method is responsible for getting the current user and returning it,
        /// to the view in the form of a list.
        /// </summary>
        /// <returns>returns the list of containing the user to the view</returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        public ActionResult Chat()
        {
            List<string> items = new List<string>();
            try
            {
                string currentUserId = User.Identity.GetUserId();
                ApplicationUser currentUser = db.Users.FirstOrDefault(
                    x => x.Id == currentUserId);


                items.Add(currentUser.UserName);
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return View(items);
        }
  
    }
}