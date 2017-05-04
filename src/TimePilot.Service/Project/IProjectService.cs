using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimePilot.Service.Project
{
    public interface IProjectService
    {
        IList<Entities.Project.Project> GetAll();
        Entities.Project.Project GetById(Entities.Project.Project project);
        bool Update(Entities.Project.Project project);
        bool Add(Entities.Project.Project project);
        bool Delete(Entities.Project.Project project);
    }
}
