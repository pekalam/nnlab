using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;

namespace Data.Domain.Services
{
    public interface ICsvValidationService
    {
        //TODO return type
        (bool result, string? error, int rows, int cols) Validate(string path);
        string[]? ReadHeaders(string path);
    }

    internal class CsvValidationService : ICsvValidationService
    {
        public (bool result, string? error, int rows, int cols) Validate(string path)
        {
            if (!File.Exists(path)) return (false, "File not found", 0,0);

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var rdr = new StreamReader(fs);
            using var csv = new CsvReader(rdr, CultureInfo.CurrentCulture);


            csv.Read();
            csv.ReadHeader();


            foreach (var str in csv.Context.HeaderRecord)
            {
                if (double.TryParse(str, out _))
                {
                    return (false, "Numeric value in csv header", 0, 0);
                }
            }


            int lines = 1;
            while (csv.Read())
            {
                for (int i = 0; i < csv.Context.HeaderRecord.Length; i++)
                {
                    if (!double.TryParse(csv[i], out _))
                    {
                        return (false, $"Invalid field value at line: {lines} column: {i + 1}", 0, 0);
                    }
                }
                lines++;
            }


            return (true, null, lines-1, csv.Context.HeaderRecord.Length);
        }

        public string[]? ReadHeaders(string path)
        {
            if (!File.Exists(path)) return null;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var rdr = new StreamReader(fs);
            using var csv = new CsvReader(rdr, CultureInfo.CurrentCulture);

            csv.Read();
            csv.ReadHeader();

            foreach (var str in csv.Context.HeaderRecord)
            {
                if (double.TryParse(str, out _))
                {
                    return null;
                }
            }

            return csv.Context.HeaderRecord;
        }
    }
}
