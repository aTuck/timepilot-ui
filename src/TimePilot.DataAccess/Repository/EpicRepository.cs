using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class EpicRepository : RepositoryBase
    {
        public List<Epic> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from Epic";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Epic> epics = dbContext.Query<Epic>(sql, new { maxRows = _maxResults }).ToList();
            return epics;
        }

        public void DeleteAll()
        {
            string sql = @"DELETE FROM Epic";
            dbContext.Query<Epic>(sql);
        }

        public List<Epic> GetAllByForeignId(string ProjectKey)
        {
            string sql = @"SELECT * from Epic where ProjectKey = @pk ORDER BY Summary ASC";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Epic> epics = dbContext.Query<Epic>(sql, new { pk = ProjectKey }).ToList();
            return epics;
        }


        /* Returns a Epic object with the same id as 'id'
         * Returns a Epic object with k = -1 if a Epic with 'id' was not found */
        public Epic GetById(Epic Epic)
        {
            // This will be the return value if no ID was found
            Epic dummyEpic = new Epic { EpicKey = "-1" };

            string sql = @"SELECT * from Epic where EpicKey = @k";
            List<Epic> epics = dbContext.Query<Epic>(sql, new { k = Epic.EpicKey }).ToList();
            if (epics.Count <= 0)
            {
                return dummyEpic;
            }
            else
            {
                return epics[0];
            }
        }

        public bool Update(Epic Epic)
        {
            throw new NotImplementedException();
        }

        /* Deletes a Epic from the Epic table
         * Returns true if successful delete
         * Returns false if not successful delete (likely Epic wasn't found)*/
        public bool Delete(int EpicKey)
        {
            string sql = @"DELETE from Epic where EpicKey = @k";
            List<Epic> epics = dbContext.Query<Epic>(sql, new { k = EpicKey }).ToList();
            if (epics.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /* Adds a Epic to the Epic table
         * Always returns true */
        public bool Add(Epic Epic)
        {
            //TSQL string to insert the project passed to this function into the project table
            string sql = @"INSERT INTO Epic (EpicKey, Summary, Projectkey) 
                           VALUES (@k, @s, @pk)";

            //Do a query sending sql string and assigning variables in sql string to the Epic object passed in
            dbContext.Query(sql, new { k = Epic.EpicKey, s = Epic.Summary, pk = Epic.ProjectKey }).ToList();

            //Epic didn't exist, now it does
            return true;
        }
    }
}