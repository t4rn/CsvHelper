using AutoFixture;
using Csv.Lib;
using Csv.TestsDomain.Classes;
using Csv.TestsDomain.Classes.Errors;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Csv.Tests
{
    public class CsvHelperErrorTests
    {
        [TestCase]
        public void WriteRecords_NullParameter()
        {
            // Arrange
            var csvHelper = new CsvHelper();
            IEnumerable<User> nullUsers = null;

            // Act
            Action act = () => csvHelper.WriteRecords(nullUsers);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .And.Message.Should().StartWith("Value cannot be null.");
        }

        [TestCase]
        public void MissingCsvFieldAttribute()
        {
            // Arrange
            IEnumerable<NoAttributes> misconfiguredObjects = new Fixture().CreateMany<NoAttributes>();
            var csvHelper = new CsvHelper();

            // Act
            Action act = () => csvHelper.WriteRecords(misconfiguredObjects);

            // Assert
            act.Should().Throw<Exception>()
                .And.Message.Should().StartWith("Missing 'CsvFieldAttribute' configuration in");
        }

        [TestCase]
        public void DuplicatedIndexes()
        {
            // Arrange
            IEnumerable<DuplicatedIndex> misconfiguredObjects = new Fixture().CreateMany<DuplicatedIndex>();
            var csvHelper = new CsvHelper();

            // Act
            Action act = () => csvHelper.WriteRecords(misconfiguredObjects);

            // Assert
            act.Should().Throw<Exception>()
                .And.Message.Should().StartWith("Duplicated index/es");
        }
    }
}
