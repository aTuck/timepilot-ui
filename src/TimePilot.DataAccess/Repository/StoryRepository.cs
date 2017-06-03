using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class StoryRepository : RepositoryBase
    {
        public List<Story> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from story";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Story> stories = dbContext.Query<Story>(sql, new { maxRows = _maxResults }).ToList();
            return stories;
        }

        public List<Story> GetAllByForeignId(string ProjectKey)
        {
            string sql = @"SELECT * from story where ProjectKey = @pk";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Story> stories = dbContext.Query<Story>(sql, new { pk = ProjectKey}).ToList();
            return stories;
        }


        /* Returns a story object with the same id as 'id'
         * Returns a story object with id = -1 if a story with 'id' was not found */
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
            }
        }

        public bool Update(Story t)
        {
            throw new NotImplementedException();
        }

        /* Deletes a story from the story table
         * Returns true if successful delete
         * Returns false if not successful delete (likely story wasn't found)*/
        public bool Delete(Story story)
        {
            string sql = @"DELETE from story where StoryID = @id";
            List<Story> stories = dbContext.Query<Story>(sql, new { id = story.StoryID }).ToList();
            if (stories.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /* Adds a story to the story table
         * Always returns true */
        public bool Add(Story story)
        {
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO story (StoryID, StoryKey, Summary, StoryPoints, Projectkey) 
                           VALUES (@id, @k, @s, @p, @pk)";

            //Do a query sending sql string and assigning variables in sql string to the story object passed in
            dbContext.Query(sql, new { id = story.StoryID, k = story.StoryKey,
                                       s = story.Summary, p = story.StoryPoints,
                                       pk = story.ProjectKey}).ToList();

            //Story didn't exist, now it does
            return true;
        }
    }
}