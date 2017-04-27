using System;
using System.Collections.Generic;
using System.IO;
using FinalYearProject.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalYearProject.Controllers
{
    /// <summary>
    /// This controller class is responsible for handing the methods that,
    /// deal with the uploading and downloading of files on the system
    /// </summary>
    public class FileController : Controller
    {


        /// <summary>
        /// This index method is responsible for creating a path for the file,
        /// to be uploaded to. File is sent through a request, filename is set,
        /// then index view is returned
        /// </summary>
        /// <returns>Returns the index view</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        public ActionResult Index()
        {
            foreach (string upload in Request.Files)
            {
                if (Request.Files[upload].FileName != "")
                {
                    try
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/uploads/";
                        string filename = Path.GetFileName(Request.Files[upload].FileName);
                        Request.Files[upload].SaveAs(Path.Combine(path, filename));
                    } catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                }
            }
            return View();
        }

        /// <summary>
        /// This controller method is responsible for handing the list of downloads that are,
        /// available to the user. A list of items in the uploads path is created, then returned,
        /// to the view
        /// </summary>
        /// <returns>returns the list of downloads to the view</returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        public ActionResult Downloads()
        {
            List<string> items = new List<string>();
            try
            {
                var dir = new System.IO.DirectoryInfo(Server.MapPath("~/App_Data/uploads/"));
                System.IO.FileInfo[] fileNames = dir.GetFiles("*.*");

                foreach (var file in fileNames)
                {
                    items.Add(file.Name);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return View(items);
        }
        /// <summary>
        /// This method is responsible for downloading the file once requested,
        /// starts the download of a specific file when called
        /// </summary>
        /// <param name="ImageName">The filename of the required file to be downloaded</param>
        /// <returns>returns the file by force downloading it</returns>
        public FileResult Download(string ImageName)
        {
           
           var FileVirtualPath = "~/App_Data/uploads/" + ImageName;
           return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));
            
            
        }
    }
}