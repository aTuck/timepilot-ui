using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;

namespace TimePilot.DataAccess
{
    public interface IRepository<T> where T : class
    {
        IList<T> GetAll();
        IList<T> GetAllByForeignId(T t);
        T GetById(T t);
        bool Update(T t);
        bool Delete(T t);
        bool Add(T t);
    }
}