using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class ProjectRepository : RepositoryBase, IProjectRepository
    {
        public List<Project> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from project";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Project> projects = dbContext.Query<Project>(sql, new {maxRows = _maxResults}).ToList();
            return projects;
        }

        public List<Project> GetAllByForeignId(Project t)
        {
            throw new NotImplementedException();
        }

        /* Returns a project object with the same key as 'key'
         * Returns a project object with key = null if a project with 'key' was not found */
        public Project GetById(Project proj)
        {
            // This will be the return value if no ID was found
            Project dummyProj = new Project { ProjectKey = null };

            string sql = @"SELECT * from project where ProjectKey = @k";
            List<Project> projects = dbContext.Query<Project>(sql, new { k = proj.ProjectKey }).ToList();
            if (projects.Count <= 0)
            {
                return dummyProj;
            }
            else
            {
                return projects[0];
            }
         }

        public bool Update(Project t)
        {
            throw new NotImplementedException();
        }

        /* Deletes a project from the project table
         * Returns true if successful delete
         * Returns false if not successful delete (likely project wasn't found)*/
        public bool Delete(Project proj)
        {
            string sql = @"DELETE from project where ProjectKey = @k";
            List<Project> projects = dbContext.Query<Project>(sql, new { k = proj.ProjectKey }).ToList();
            if (projects.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /* Adds a project to the project table
         * Always returns true */
        public bool Add(Project t)
        {
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO project (Projectkey, Summary, ModifiedDate) VALUES (@k, @s, @date)";

            //Do a query sending sql string and assigning "@p" variable in sql string to the t object passed in
            dbContext.Query(sql, new { k = t.ProjectKey, s = t.Summary, date = DateTime.Now });

            //Project didn't exist, now it does
            return true;
        }

        public List<Project> SearchProjects(string search)
        {
            throw new NotImplementedException();
        }

        public List<Project> GetByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}