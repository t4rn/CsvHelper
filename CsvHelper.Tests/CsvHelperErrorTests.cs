using CsvHelper.DomainForTests.Classes;
using CsvHelper.Lib.Classes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using CsvHelper.DomainForTests.Classes.Errors;

namespace CsvHelper.Tests
{
    public class CsvHelperErrorTests
    {

        [TestCase]
        public void MissingCsvFieldAttribute()
        {
            // Arrange
            var fixture = new Fixture();

            // Act
            Action act = () => new CsvHelper<NoAttributes>();

            // Assert
            act.Should().Throw<NotImplementedException>()
                .And.Message.Should().StartWith("Missing 'CsvFieldAttribute' configuration in");
        }

        [TestCase]
        public void DuplicatedIndexes()
        {
            // Arrange
            var fixture = new Fixture();

            // Act
            Action act = () => new CsvHelper<DuplicatedIndex>();

            // Assert
            act.Should().Throw<NotImplementedException>()
                .And.Message.Should().StartWith("Duplicated index/es");
        }
    }
}
