using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1_lab4_5.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;



    }
}
