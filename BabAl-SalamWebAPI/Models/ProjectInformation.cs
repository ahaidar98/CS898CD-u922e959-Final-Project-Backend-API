using System;
using System.Collections.Generic;
using System.Linq;

namespace BabAl_SalamWebAPI.Models
{
    public class ProjectInformation
    {
        public ProjectDTO Project { set; get; }
        //public IQueryable<ItemDTO> Items { set; get; }
    }

    public class ProjectDataInformation
    {
        public ProjectInformation ProjectInformation { set; get; }
        public IEnumerable<ProjectDTO> Projects { set; get; }
    }
}
