﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Models.ViewModels
{
    public class UserRoleVM:ApplicationUser
    {
        public string? Role { get; set; }
    }
}
