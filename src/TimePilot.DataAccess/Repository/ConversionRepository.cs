using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class ConversionRepository : RepositoryBase
    {
        public List<Conversion> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from ConversionRate";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Conversion> conversions = dbContext.Query<Conversion>(sql, new { maxRows = _maxResults }).ToList();
            return conversions;
        }

        public List<Conversion> GetAllByForeignId(string ProjectKey)
        {
            string sql = @"SELECT * from ConversionRate where ProjectKey = @pk";

            //maxRows = this._maxResults, hardcoded to 1000 for now due to maxResults being null
            List<Conversion> conversions = dbContext.Query<Conversion>(sql, new { pk = ProjectKey }).ToList();
            return conversions;
        }

        /* Returns a Conversion object with the same key as 'key'
         * Returns a Conversion object with key = null if a Conversion with 'key' was not found */
        public Conversion GetById(Conversion conversion)
        {
            // This will be the return value if no ID was found
            Conversion dummyConversion = new Conversion { ConversionRateID = -1 };

            string sql = @"SELECT * from ConversionRate where ConversionRateID = @id";
            List<Conversion> Conversions = dbContext.Query<Conversion>(sql, new { id = conversion.ConversionRateID }).ToList();
            if (Conversions.Count <= 0)
            {
                return dummyConversion;
            }
            else
            {
                return Conversions[0];
            }
        }

        public bool Update(Conversion conversion)
        {
            string sql = @"UPDATE ConversionRate SET   StoryPoints = @sp, ConversionRate = @cr, 
                                                       ProjectKey = @pk
                                                 WHERE ConversionRateID = @id";
            dbContext.Query<Member>(sql, new
            {
                id = conversion.ConversionRateID,
                sp = conversion.StoryPoints,
                cr = conversion.ConversionRate,
                pk = conversion.ProjectKey
            });

            return true;
        }

        /* Deletes a Conversion from the Conversion table
         * Returns true if successful delete
         * Returns false if not successful delete (likely Conversion wasn't found)*/
        public bool Delete(Conversion conversion)
        {
            string sql = @"DELETE from ConversionRate where ConversionRateID = @id";
            List<Conversion> Conversions = dbContext.Query<Conversion>(sql, new { id = conversion.ConversionRateID }).ToList();
            if (Conversions.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /* Adds a Conversion to the Conversion table
         * Always returns true */
        public int Add(Conversion conversion)
        {
            Conversion check = GetById(conversion);
            if (check.ConversionRateID == -1)
            {
                //TSQL string to insert the Conversion passed to this function into the Conversion table
                string sql = @"INSERT INTO ConversionRate (StoryPoints, ConversionRate, ProjectKey) 
                                                           VALUES (@sp, @r, @k)
                                                           SELECT CAST(SCOPE_IDENTITY() as int)";

                //Do a query sending sql string and assigning "@p" variable in sql string to the t object passed in
                var addedConversionID = dbContext.Query<int>(sql, new { sp = conversion.StoryPoints,
                                                                        r = conversion.ConversionRate,
                                                                        k = conversion.ProjectKey}).Single();

                //Conversion didn't exist, now it does
                return addedConversionID;
            }
            else
            {
                Update(conversion);
                return conversion.ConversionRateID;
            }
        }

        public List<Conversion> SearchConversions(string search)
        {
            throw new NotImplementedException();
        }

        public List<Conversion> GetByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}