using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models.Git
{
    public class TreeObject
    {
        public string Name { get; set; }
        public string Sha { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
