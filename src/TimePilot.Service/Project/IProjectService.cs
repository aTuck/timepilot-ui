using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimePilot.Entities;

namespace TimePilot.Service.Project
{
    public interface IProjectService
    {
        IList<Entities.Project> GetAll();
        Entities.Project GetById(Entities.Project project);
        bool Update(Entities.Project project);
        bool Add(Entities.Project project);
        bool Delete(Entities.Project project);
    }
}
