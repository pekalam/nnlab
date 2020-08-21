using System;
using System.Collections.Generic;
using System.Text;
using Data.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Data.Tests.Domain.FileServices
{
    public class CsvValidationServiceTests
    {
        [Theory]
        [InlineData(@"Files\invalidcsv_field.csv")]
        [InlineData(@"Files\invalidcsv_emptyfields.csv")]
        [InlineData(@"Files\invalidcsv_noheader.csv")]
        [InlineData(@"does_not_exist")]
        public void Validate_when_invalid_csv_returns_false_and_error_msg(string file)
        {
            //arrange
            var service = new CsvValidationService();

            //act
            var (result, error, r, c) = service.Validate(file);


            //assert
            result.Should().BeFalse();
            error.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData(@"Files\plik.csv", 99, 2)]
        public void Validate_when_valid_returns_true_and_null_error(string file, int rows, int cols)
        {
            //arrange
            var service = new CsvValidationService();

            //act
            var (result, error, r, c) = service.Validate(file);


            //assert
            result.Should().BeTrue();
            error.Should().BeNull();

            r.Should().Be(rows);
            c.Should().Be(cols);
        }


        [Fact]
        public void ReadHeaders_when_valid_returns_header_values()
        {
            //arrange
            var service = new CsvValidationService();

            //act
            var headers = service.ReadHeaders(@"Files\plik.csv");

            headers.Should().BeEquivalentTo("x", "y");
        }


        [Fact]
        public void ReadHeaders_when_invalid_returns_null()
        {
            //arrange
            var service = new CsvValidationService();

            //act
            var headers = service.ReadHeaders(@"Files\invalidscsv_noheader.csv");

            headers.Should().BeNull();
        }
    }
}