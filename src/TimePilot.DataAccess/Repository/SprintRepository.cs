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
            List<Sprint> sprints = dbContext.Query<Sprint>(sql, new { maxRows = _maxResults }).ToList();
            return sprints;
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

        private void Update(Sprint sprint)
        {
            string sql = @"UPDATE sprint SET Name = @n WHERE SprintID = @id";
            dbContext.Query<Sprint>(sql, new { n = sprint.Name, id = sprint.SprintID });
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
        public int Add(Sprint sprint)
        {
            Sprint check = GetById(sprint);
            if (check.SprintID == -1)
            {
                //TSQL string to insert the project passed to this function into the project table
                string sql = @"INSERT INTO Sprint (Name, ProjectKey) VALUES (@n, @pk);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                //Do a query sending sql string and assigning variables in sql string to the sprint object passed in
                var addedSprintID = dbContext.Query<int>(sql, new { n = sprint.Name, pk = sprint.ProjectKey }).Single();

                //Sprint didn't exist, now it does
                return addedSprintID;
            }
            else
            {
                Update(sprint);
                return sprint.SprintID;
            }
        }
    }
}