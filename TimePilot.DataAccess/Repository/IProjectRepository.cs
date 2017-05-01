using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimePilot.Entities.Project;

namespace TimePilot.DataAccess.Repository
{
    public interface IProjectRepository : IRepository<Project>
    {
        IList<Project> SearchProjects(string search);
        IList<Project> GetByName(string name);
    }
}
