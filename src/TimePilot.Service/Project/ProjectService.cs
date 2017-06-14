using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimePilot.DataAccess.Repository;
using TimePilot.Entities;

namespace TimePilot.Service.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public IList<Entities.Project> GetAll()
        {
            throw new NotImplementedException();
        }

        public Entities.Project GetById(Entities.Project project)
        {
            throw new NotImplementedException();
        }

        public bool Update(Entities.Project project)
        {
            throw new NotImplementedException();
        }

        public bool Add(Entities.Project project)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Entities.Project project)
        {
            throw new NotImplementedException();
        }
    }
}