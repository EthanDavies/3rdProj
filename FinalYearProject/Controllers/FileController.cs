using System;
using System.Collections.Generic;
using System.IO;
using FinalYearProject.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalYearProject.Controllers
{
    public class FileController : Controller
    {
        
        [Authorize(Roles = "Admin, Lecturer")]
        public ActionResult Index()
        {
            return View(new UploadModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Lecturer")]
        public ActionResult Index([Bind(Include = "Course,Title")] UploadModel testing)
        {


            if (ModelState.IsValid)
            {
                foreach (string upload in Request.Files)
                {

                    if (Request.Files[upload].FileName != "")
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/uploads/";
                        string filename = Path.GetFileName(Request.Files[upload].FileName);
                        
                        string newfilename = Path.GetFileName((testing.Title) + "." + "pdf");
                        Request.Files[upload].SaveAs(Path.Combine(path, filename));
                    }
                }
                return RedirectToAction("Index");
            }
            //You can use model.Course and model.Title values now
            
            return View(testing);
        }


        [Authorize(Roles = "Admin, Lecturer, Student")]
        public ActionResult Downloads()
        {
            var dir = new System.IO.DirectoryInfo(Server.MapPath("~/App_Data/uploads/"));
            System.IO.FileInfo[] fileNames = dir.GetFiles("*.*");
            List<string> items = new List<string>();
            foreach (var file in fileNames)
            {
                items.Add(file.Name);
            }
            return View(items);
        }

        public FileResult Download(string ImageName)
        {
            var FileVirtualPath = "~/App_Data/uploads/" + ImageName;
            return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));
        }
    }
}