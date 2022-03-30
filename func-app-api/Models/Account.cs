using System;
using System.Collections.Generic;
using System.Text;

namespace func_app_api.Models
{
    public class Account
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public bool IsGreater { get; set; }
    }
}
