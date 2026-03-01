using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace API.Entities
{
    public class Group(string name)
    {
        [Key]
        public required string Name { get; set; } = name;

        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}