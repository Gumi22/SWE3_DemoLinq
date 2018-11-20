using System;
using System.Linq;
using Npgsql;

namespace Linq
{
    class Program
    {
        static void Main(string[] args)
        {
            OrMapper orm = new OrMapper(new PostGreSqlDatabase());

            //ToDo: Get some values from OR Mapper with query.

            //ToDo: Change these Values.

            //ToDo: Save dif to database.



            //Testing: 

            var qry = orm.GetQuery<Photographer>();
            var qry2 = orm.GetQuery<ChangeTrackerEntry>();

            var filtered = qry
                .Where(i => i.Id <= 3);

            var lst = filtered.ToList();

            foreach(var i in lst)
            {
                Console.WriteLine($"{i.Id}, {i.Name}, {i.SurName}, {i.BirthDate}, {i.Notes}");
            }

            Console.ReadKey();
        }
    }
}
