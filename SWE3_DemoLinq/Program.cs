using System;
using System.Collections.Generic;
using System.Linq;
using Linq.Tables;
using Npgsql;

namespace Linq
{
    class Program
    {
        static void Main(string[] args)
        {
            /*OrMapper orm = new OrMapper();

            //Get some values
            var lst = GetListAndPrintPhotographers(orm);
            Console.WriteLine("------------------------------------------------------------------");
            var lstc = GetListAndPrintCameras(orm);
            Console.WriteLine("------------------------------------------------------------------");
            var lstp = GetListAndPrintPictures(orm);
            Console.WriteLine("------------------------------------------------------------------");


            //Change some Values:
            lst[0].Name += " X";
            lst[1].BirthDate = lst[1].BirthDate.AddDays(1);
            lst[2].Notes += " X";
            
            orm.SubmitChanges();

            GetListAndPrintPhotographers(orm);
            Console.WriteLine("------------------------------------------------------------------");

            //insert a new Value:
            orm.Insert(new Photographer()
            {
                BirthDate = new DateTime(1968, 10, 30),
                Id = 0,
                Name = "Lol",
                Notes = "notes",
                SurName = "OMG"
            });

            orm.SubmitChanges();

            var lst2 = GetListAndPrintPhotographers(orm);
            Console.WriteLine("------------------------------------------------------------------");

            //Delete a Value:
            using (var lst3 = lst2.Where(photographer => photographer.Name.Equals("Lol")).GetEnumerator()) { 
                if (lst3.MoveNext())
                {
                    orm.Delete(lst3.Current);
                }
            }

            orm.SubmitChanges();

            GetListAndPrintPhotographers(orm);
            Console.WriteLine("------------------------------------------------------------------");*/

            Console.ReadKey();
        }

        private static List<Photographer> GetListAndPrintPhotographers(OrMapper orm)
        {
            var qry = orm.GetQuery<Photographer>();
            var filtered = qry
                .Where(i => true);

            var lst = filtered.ToList();

            foreach (var i in lst)
            {
                Console.WriteLine($"{i.Id}, {i.Name}, {i.SurName}, {i.BirthDate}, {i.Notes}");
            }

            return lst;
        }

        private static List<Camera> GetListAndPrintCameras(OrMapper orm)
        {
            var qry = orm.GetQuery<Camera>();
            var filtered = qry
                .Where(i => true);

            var lst = filtered.ToList();

            foreach (var i in lst)
            {
                Console.WriteLine($"{i.Id}, {i.IsoLimitAcceptable}, {i.IsoLimitGood}, {i.Model}, {i.Notes}, {i.Producer}, {i.PurchaseDate}");
            }

            return lst;
        }

        private static List<Picture> GetListAndPrintPictures(OrMapper orm)
        {
            var qry = orm.GetQuery<Picture>();
            var filtered = qry
                .Where(i => true);

            var lst = filtered.ToList();

            foreach (var i in lst)
            {
                Console.WriteLine($"{i.Id}, {i.Filename}, {i.PhotographerId}, {i.CameraId}, {i.ExifAperture}, {i.ExifExposureProg}, {i.ExifExposureTime}, {i.ExifFlash}, {i.ExifIso}, {i.IptcCaption}, {i.IptcCopyright}, {i.IptcHeadline}, {i.IptcKeywords}");
            }

            return lst;
        }

    }
}
