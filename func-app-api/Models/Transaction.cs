using System;
using System.Collections.Generic;
using System.Text;

namespace func_app_api.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int Account { get; set; }
        public string Direction { get; set; }
        public float Amount { get; set; }
    }
}
