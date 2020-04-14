using AutoFixture;
using CsvHelper.DomainForTests.Classes;
using CsvHelper.Lib.Classes;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper.ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Fixture fixture = new Fixture();
            int elementsCount = 10;
            Console.WriteLine($"{DateTime.Now} - Start for {elementsCount} elements");

            List<User> users = fixture.CreateMany<User>(elementsCount).ToList();
            Console.WriteLine($"{DateTime.Now} - Users prepared");

            //TestPerformanceOfAtrributeReading(users);

            users.First().BirthDate = null;

            var csvHelper = new CsvHelper<User>();
            csvHelper.Config.DateTimeFormat = "yyyyMMdd HH:mm:ss";
            csvHelper.Config.Delimiter = '\t';
            csvHelper.Config.HasHeaderRecord = true;



            string csvWriteResult = csvHelper.WriteRecords(users);


            byte[] byteArray = Encoding.UTF8.GetBytes(csvWriteResult);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);
            CsvReadResult<User> csvReaderResult = csvHelper.GetRecords(stream);


            csvReaderResult.Errors.Should().HaveCount(0);
            csvReaderResult.Records.Should().HaveCount(users.Count);

            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].BirthDate == null)
                {
                    csvReaderResult.Records[i].BirthDate.Should().BeNull();
                }
                else
                {
                    csvReaderResult.Records[i].BirthDate.Should()
                        .Be(users[i].BirthDate.Value.AddTicks(-(users[i].BirthDate.Value.Ticks % TimeSpan.TicksPerSecond)));
                }
                csvReaderResult.Records[i].FirstName.Should().Be(users[i].FirstName);
                csvReaderResult.Records[i].Id.Should().Be(users[i].Id);
                csvReaderResult.Records[i].LastName.Should().Be(users[i].LastName);
                csvReaderResult.Records[i].WorkStatus.Should().Be(users[i].WorkStatus);
                csvReaderResult.Records[i].Title.Should().Be(users[i].Title);
            }


            
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

            Console.ReadLine();
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
