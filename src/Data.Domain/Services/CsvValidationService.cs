﻿using CsvHelper;
using System.Globalization;
using System.IO;

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

            FileStream? fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                return (false, "File does not exist or is being used by another process", 0, 0);
            }
            using var rdr = new StreamReader(fs);
            using var csv = new CsvReader(rdr, CultureInfo.InvariantCulture);


            try
            {
                csv.Read();
                csv.ReadHeader();


                foreach (var str in csv.Context.HeaderRecord)
                {
                    if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        return (false, "Numeric value in csv header", 0, 0);
                    }
                }


                int lines = 1;
                while (csv.Read())
                {
                    for (int i = 0; i < csv.Context.HeaderRecord.Length; i++)
                    {
                        if (!double.TryParse(csv[i], NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        {
                            return (false, $"Invalid field value at line: {lines} column: {i + 1}", 0, 0);
                        }
                    }
                    lines++;
                }
                return (true, null, lines - 1, csv.Context.HeaderRecord.Length);
            }
            catch (System.Exception)
            {
                return (false, "Invalid csv file", 0, 0);
            }
        }

        public string[]? ReadHeaders(string path)
        {
            if (!File.Exists(path)) return null;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var rdr = new StreamReader(fs);
            using var csv = new CsvReader(rdr, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            foreach (var str in csv.Context.HeaderRecord)
            {
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    return null;
                }
            }

            return csv.Context.HeaderRecord;
        }
    }
}
