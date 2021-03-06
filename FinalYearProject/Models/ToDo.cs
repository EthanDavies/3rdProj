﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalYearProject.Models
{
    /// <summary>
    /// This model is used to create the data structure that makes up a todo item
    /// </summary>
    public class ToDo
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool IsDone { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}