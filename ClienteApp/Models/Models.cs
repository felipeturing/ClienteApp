using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteApp.Models
{
    public class Data
    {
        public Worker? worker { get; set; }
        public Grupo? grupo { get; set; }
    }

    public class Grupo
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public App[]? apps { get; set; }
    }

    public class Worker
    {
        public string? name { get; set; }
        public string? status { get; set; }
        public string? statusApp { get; set; }
    }

    public class App
    {
        public string? name { get; set; }
        public string? version { get; set; }
        public string? path { get; set; }
    }
}
