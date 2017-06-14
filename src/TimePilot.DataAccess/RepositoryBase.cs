#region References

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

#endregion

namespace TimePilot.DataAccess
{
    public abstract class RepositoryBase : IDisposable
    {
        protected readonly IDbConnection _dbContext;
        protected readonly int _maxResults;

        protected RepositoryBase()
        {
            _dbContext = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            _maxResults = int.Parse(ConfigurationManager.AppSettings["MaxResults"]);
        }

        protected IDbConnection dbContext
        {
            get
            {
                if (_dbContext.State == ConnectionState.Closed ||
                    _dbContext.State == ConnectionState.Broken)
                    _dbContext.Open();
                return _dbContext;
            }
        }
        protected void OpenConnection()
        {
            _dbContext.Open();
        }


        public void Dispose()
        {
            //close the connection if it is not closed
            if (_dbContext.State != ConnectionState.Closed)
                _dbContext.Close();
        }
    }
}