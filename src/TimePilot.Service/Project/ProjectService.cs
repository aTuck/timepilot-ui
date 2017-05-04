using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimePilot.DataAccess.Repository;

namespace TimePilot.Service.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public IList<Entities.Project.Project> GetAll()
        {
            throw new NotImplementedException();
        }

        public Entities.Project.Project GetById(Entities.Project.Project project)
        {
            throw new NotImplementedException();
        }

        public bool Update(Entities.Project.Project project)
        {
            throw new NotImplementedException();
        }

        public bool Add(Entities.Project.Project project)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Entities.Project.Project project)
        {
            throw new NotImplementedException();
        }
    }
}