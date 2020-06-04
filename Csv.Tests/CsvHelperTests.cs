using AutoFixture;
using Csv.Lib;
using Csv.TestsDomain.Classes;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csv.Tests
{
    public class CsvHelperTests
    {
        [TestCase]
        public void WriteReadCSV()
        {
            // Arrange
            var users = PrepareUsers(10);

            var csvHelper = new CsvHelper();
            csvHelper.Config.DateTimeFormat = "yyyyMMdd HH:mm:ss";
            csvHelper.Config.Delimiter = '\t';
            csvHelper.Config.HasHeaderRecord = true;


            // Act

            // write CSV
            string csvWriteResult = csvHelper.WriteRecords(users);

            // read CSV
            CsvReadResult<User> csvReaderResult = csvHelper.GetRecords<User>(csvWriteResult);


            // Assert
            csvReaderResult.Errors.Should().HaveCount(0);
            csvReaderResult.Records.Should().HaveCount(users.Count());

            csvReaderResult.Records.Should().BeEquivalentTo(users);
        }

        private IEnumerable<User> PrepareUsers(int count)
        {
            var fixture = new Fixture();
            Random random = new Random();
            var users = new List<User>(); //fixture.CreateMany<User>();

            for (int i = 0; i < count; i++)
            {
                var u = fixture.Create<User>();
                u.RowNumber = i + 2;
                u.Id = 100 + u.RowNumber;
                u.NotMapped = 0;
                u.BirthDate = random.NextDouble() > 0.6 ? null : fixture.Create<DateTime?>();
                if (u.BirthDate.HasValue)
                {
                    // remove milliseconds
                    u.BirthDate = u.BirthDate.Value.AddTicks(-(u.BirthDate.Value.Ticks % TimeSpan.TicksPerSecond));
                }

                users.Add(u);
            }

            return users;
        }
    }
}
