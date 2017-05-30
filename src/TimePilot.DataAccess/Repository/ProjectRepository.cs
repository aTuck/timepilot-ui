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

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Project> projects = dbContext.Query<Project>(sql, new {maxRows = 200}).ToList();
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
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO project (Projectkey, ModifiedDate) VALUES (@k, @date)";

            //Do a query sending sql string and assigning "@p" variable in sql string to the t object passed in
            dbContext.Query(sql, new { k = t.Key, date = DateTime.Now });

            //Project didn't exist, now it does
            return true;
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