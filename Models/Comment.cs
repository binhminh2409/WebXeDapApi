﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebXeDapAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
    }
}
