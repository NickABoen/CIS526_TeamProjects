﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Demo.Models
{
    public class PointType
    {
        [Key]
        public int ID { get; set; }

        [Display(Name="Point Type")]
        public string Name { get; set; }
    }
}