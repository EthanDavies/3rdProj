using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinalYearProject.Models
{
    public class UploadModel
    {
        [Required(ErrorMessage = "Course is required")]
        public string Course { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Uploader { get; set; }
    }
}