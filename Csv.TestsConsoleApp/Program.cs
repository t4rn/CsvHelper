using AutoFixture;
using Csv.Lib;
using Csv.TestsDomain.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Csv.TestsConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var fixture = new Fixture();
            int elementsCount = 10000;

            Console.WriteLine($"{DateTime.Now} - Start for {elementsCount} elements");

            List<User> users = fixture.CreateMany<User>(elementsCount).ToList();
            Console.WriteLine($"{DateTime.Now} - Users prepared");

            TestPerformanceOfAtrributeReading(users);


            Console.WriteLine("\nEnd - press any key to exit...");
            Console.Read();
        }

        private static void TestPerformanceOfAtrributeReading(List<User> users)
        {
            var stopwatch = Stopwatch.StartNew();
            var indGetIndexesByAll = GetIndexesByAllAttributes<User>();
            Console.WriteLine($"{stopwatch.Elapsed} for GetIndexesByAll");

            stopwatch.Restart();
            var indGetIndexesByOne = GetIndexesBySpecificAttribute<User>();
            Console.WriteLine($"{stopwatch.Elapsed} for GetIndexesByOne");

            stopwatch.Restart();
            var x = GetIndexesBySpecificAttribute<User>();
            Console.WriteLine($"{stopwatch.Elapsed} for GetIndexesByOne");

            stopwatch.Restart();
            var x2 = GetIndexesByAllAttributes<User>();
            Console.WriteLine($"{stopwatch.Elapsed} for GetIndexesByAll");
        }

        private static Dictionary<string, int> GetIndexesByAllAttributes<T>()
        {
            var dict = new Dictionary<string, int>();

            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(false);
                foreach (object attr in attrs)
                {
                    if (attr is CsvFieldAttribute csvAttr)
                    {
                        string propName = prop.Name;
                        int indx = csvAttr.Index;
                        string colName = csvAttr.ColumnName;
                        Type type = prop.PropertyType;

                        dict.Add(propName, indx);
                    }
                }
            }

            return dict;
        }

        private static Dictionary<string, int> GetIndexesBySpecificAttribute<T>()
        {
            var dict = new Dictionary<string, int>();

            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                CsvFieldAttribute csvAttr = prop.GetCustomAttribute<CsvFieldAttribute>(false);
                if (csvAttr != null)
                {
                    string propName = prop.Name;
                    int indx = csvAttr.Index;
                    string colName = csvAttr.ColumnName;
                    Type type = prop.PropertyType;

                    dict.Add(propName, indx);
                }
            }

            return dict;
        }
    }
}
