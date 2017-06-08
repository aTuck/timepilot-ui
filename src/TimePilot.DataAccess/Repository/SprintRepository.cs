using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class SprintRepository : RepositoryBase
    {
        public List<Sprint> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from sprint";

            //_maxResults is set in web.config
            List<Sprint> stories = dbContext.Query<Sprint>(sql, new { maxRows = _maxResults }).ToList();
            return stories;
        }

        public void DeleteAll()
        {
            string sql = @"DELETE FROM sprint";
            dbContext.Query<Sprint>(sql);
        }

        public List<Sprint> GetAllByForeignId(string ProjectKey)
        {
            string sql = @"SELECT * from sprint where ProjectKey = @pk";

            List<Sprint> sprints = dbContext.Query<Sprint>(sql, new { pk = ProjectKey }).ToList();
            return sprints;
        }

        /* Returns a sprint object with the same id as 'id'
         * Returns a sprint object with id = -1 if a sprint with 'id' was not found */
        public Sprint GetById(Sprint sprint)
        {
            // This will be the return value if no ID was found
            Sprint dummySprint = new Sprint { SprintID = -1 };

            string sql = @"SELECT * from sprint where SprintID = @id";
            List<Sprint> sprints = dbContext.Query<Sprint>(sql, new { id = sprint.SprintID }).ToList();
            if (sprints.Count <= 0)
            {
                return dummySprint;
            }
            else
            {
                return sprints[0];
            }
        }

        public bool Update(Sprint sprint)
        {
            throw new NotImplementedException();
        }

        /* Deletes a Sprint from the Sprint table
         * Returns true if successful delete
         * Returns false if not successful delete (likely Sprint wasn't found)*/
        public bool Delete(int SprintID)
        {
            string sql = @"DELETE from sprint where SprintID = @id";
            List<Sprint> sprints = dbContext.Query<Sprint>(sql, new { id = SprintID }).ToList();
            if (sprints.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /* Adds a Sprint to the Sprint table
         * Always returns true */
        public bool Add(Sprint sprint)
        {
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO Sprint (SprintID, ProjectKey) 
                           VALUES (@id, @pk)";

            //Do a query sending sql string and assigning variables in sql string to the sprint object passed in
            dbContext.Query(sql, new {id = sprint.SprintID, pk = sprint.ProjectKey}).ToList();

            //Sprint didn't exist, now it does
            return true;
        }
    }
}