using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
<<<<<<< HEAD
using TimePilot.Entities.Project;

namespace TimePilot.DataAccess.Repository
{
    public class Story : RepositoryBase
=======
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class StoryRepository : RepositoryBase
>>>>>>> database
    {
        public List<Story> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from story";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Story> projects = dbContext.Query<Story>(sql, new { maxRows = _maxResults }).ToList();
            return projects;
        }

        public List<Story> GetAllByForeignId(Story t)
        {
            throw new NotImplementedException();
        }


        /* Returns a project object with the same key as 'key'
         * Returns a project object with key = null if a project with 'key' was not found */
<<<<<<< HEAD
        public Story GetById(Story proj)
        {
            // This will be the return value if no ID was found
            Story dummyProj = new Story { ProjectKey = null };

            string sql = @"SELECT * from project where ProjectKey = @k";
            List<Story> projects = dbContext.Query<Story>(sql, new { k = proj.ProjectKey }).ToList();
            if (projects.Count <= 0)
            {
                return dummyProj;
            }
            else
            {
                return projects[0];
=======
        public Story GetById(Story story)
        {
            // This will be the return value if no ID was found
            Story dummyStory = new Story { StoryID = -1 };

            string sql = @"SELECT * from story where StoryID = @id";
            List<Story> stories = dbContext.Query<Story>(sql, new { id = story.StoryID }).ToList();
            if (stories.Count <= 0)
            {
                return dummyStory;
            }
            else
            {
                return stories[0];
>>>>>>> database
            }
        }

        public bool Update(Story t)
        {
            throw new NotImplementedException();
        }

        /* Deletes a project from the project table
         * Returns true if successful delete
         * Returns false if not successful delete (likely project wasn't found)*/
<<<<<<< HEAD
        public bool Delete(Story proj)
        {
            string sql = @"DELETE from project where ProjectKey = @k";
            List<Story> projects = dbContext.Query<Story>(sql, new { k = proj.ProjectKey }).ToList();
=======
        public bool Delete(Story story)
        {
            string sql = @"DELETE from project where ProjectKey = @k";
            List<Story> projects = dbContext.Query<Story>(sql, new { k = story.ProjectKey }).ToList();
>>>>>>> database
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
<<<<<<< HEAD
        public bool Add(Story t)
        {
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO project (Projectkey, Summary, ModifiedDate) VALUES (@k, @s, @date)";

            //Do a query sending sql string and assigning "@p" variable in sql string to the t object passed in
            dbContext.Query(sql, new { k = t.ProjectKey, s = t.Summary, date = DateTime.Now });
=======
        public bool Add(Story story)
        {
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO story (StoryID, StoryKey, Summary, StoryPoints, Projectkey) 
                           VALUES (@id, @k, @s, @p, @pk)";

            //Do a query sending sql string and assigning "@p" variable in sql string to the t object passed in
            dbContext.Query(sql, new { id = story.StoryID, k = story.StoryKey,
                                       s = story.Summary, p = story.StoryPoints,
                                       pk = story.ProjectKey}).ToList();
>>>>>>> database

            //Project didn't exist, now it does
            return true;
        }
    }
}