using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using System.Web;
using TimePilot.Entities;

namespace TimePilot.DataAccess.Repository
{
    public class MemberRepository : RepositoryBase
    {
        public List<Member> GetAll()
        {
            string sql = @"SELECT TOP (@maxRows) * from member";

            //_maxResults is set in web.config
            List<Member> members = dbContext.Query<Member>(sql, new { maxRows = _maxResults }).ToList();
            return members;
        }

        public void DeleteAll()
        {
            string sql = @"DELETE FROM member";
            dbContext.Query<Member>(sql);
        }

        public List<Member> GetAllByForeignId(int SprintID)
        {
            string sql = @"SELECT * from member where SprintID = @s";

            List<Member> members = dbContext.Query<Member>(sql, new { s = SprintID }).ToList();
            return members;
        }

        /* Returns a member object with the same id as 'id'
         * Returns a member object with id = -1 if a member with 'id' was not found */
        public Member GetById(Member member)
        {
            // This will be the return value if no ID was found
            Member dummyMember = new Member { MemberID = -1 };

            string sql = @"SELECT * from member where MemberID = @id";
            List<Member> members = dbContext.Query<Member>(sql, new { id = member.MemberID }).ToList();
            if (members.Count <= 0)
            {
                return dummyMember;
            }
            else
            {
                return members[0];
            }
        }

        private void Update(Member member)
        {
            string sql = @"UPDATE Member SET   Name = @n, SprintDays = @sd, Percentwork = @pw, 
                                               StandupDuration = @std, Misc = @m, TimeOff = @t,
                                               SprintID = @sid
                                         WHERE MemberID = @id";
            dbContext.Query<Member>(sql, new { id = member.MemberID, n = member.Name, sd = member.SprintDays,
                                               pw = member.PercentWork, std = member.StandupDuration,
                                               m = member.Misc, t = member.TimeOff, sid = member.SprintID });
        }

        /* Deletes a Member from the Member table
         * Returns true if successful delete
         * Returns false if not successful delete (likely Member wasn't found)*/
        public bool Delete(int MemberID)
        {
            string sql = @"DELETE from member where MemberID = @id";
            List<Member> members = dbContext.Query<Member>(sql, new { id = MemberID }).ToList();
            if (members.Count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /* Adds a Member to the Member table
         * Always returns true */
        public int Add(Member member)
        {
            Member check = GetById(member);
            if (check.MemberID == -1)
            {
                //TSQL string to insert the project passed to this function into the project table
                string sql = @"INSERT INTO Member (Name, SprintDays, Percentwork, StandupDuration,
                                                   Misc, TimeOff, SprintID) 
                                                   VALUES (@n, @sd, @pw, @std, @m, @t, @sid)
                                                   SELECT CAST(SCOPE_IDENTITY() as int)";

                //Do a query sending sql string and assigning variables in sql string to the member object passed in
                var addedMemberID = dbContext.Query<int>(sql, new { n = member.Name, sd = member.SprintDays,
                                                                    pw = member.PercentWork, std = member.StandupDuration,
                                                                    m = member.Misc, t = member.TimeOff, sid = member.SprintID}).Single();

                //Member didn't exist, now it does
                return addedMemberID;
            }
            else
            {
                Update(member);
                return member.MemberID;
            }
        }
    }
}