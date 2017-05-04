using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities.Project;

namespace TimePilot.DataAccess.Repository
{
    public class ProjectRepository : RepositoryBase, IProjectRepository
    {
        public IList<Project> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from project";

            List<Project> projects = dbContext.Query<Project>(sql, new {maxRows = this._maxResults}).ToList();
            return projects;
        }

        public IList<Project> GetAllByForeignId(Project t)
        {
            throw new NotImplementedException();
        }

        public Project GetById(Project t)
        {
            throw new NotImplementedException();
        }

        public bool Update(Project t)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Project t)
        {
            throw new NotImplementedException();
        }

        public bool Add(Project t)
        {
            throw new NotImplementedException();
        }

        public IList<Project> SearchProjects(string search)
        {
            throw new NotImplementedException();
        }

        public IList<Project> GetByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}