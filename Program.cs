﻿using System;
using System.Linq;

namespace Linq
{
    class Program
    {
        static void Main(string[] args)
        {
            var qry = new DemoLinq<MyTable>();

            int x = 18;

            var filtered = qry
                .Where(i => i.Age > x && i.Age < 40)
                .Where(i => i.FirstName == "Peter")
                .Where(i => i.Age < 10)
                .Where(i => i.FirstName == "Pwereeter");

            var lst = filtered.ToList();

            foreach(var i in lst)
            {
                Console.WriteLine(i);
            }

            Console.ReadKey();
        }
    }
}
