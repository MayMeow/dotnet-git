using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string OwnerID { get; set; }

        [ForeignKey("OwnerID")]
        public ApplicationUser Owner { get; set; }
    }
}
