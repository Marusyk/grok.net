using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ConsoleTables;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace GrokNet.PowerShell
{
    internal static class OutputHelper
    {
        public static string GetJsonOutput(List<Dictionary<string, object>> records, bool indent)
        {
            if (records.Count == 0)
            {
                return "[]";
            }

            return JsonConvert.SerializeObject(records, indent ? Formatting.Indented : Formatting.None);
        }

        public static string GetCsvOutput(List<Dictionary<string, object>> records, string delimiter)
        {
            if (records.Count == 0)
            {
                return string.Empty;
            }

            Dictionary<string, object>[] notNullRecords = records.Where(r => r != null).ToArray();
            if (!notNullRecords.Any())
            {
                return "No matching elements found";
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine, Delimiter = delimiter
            };

            using (var writer = new StringWriter())
            {
                using (var csv = new CsvWriter(writer, config))
                {
                    // write headers
                    Dictionary<string, object>.KeyCollection headers = notNullRecords.First().Keys;
                    foreach (var header in headers)
                    {
                        csv.WriteField(header);
                    }

                    csv.NextRecord();

                    // write records
                    foreach (Dictionary<string, object> item in records)
                    {
                        if (item != null)
                        {
                            foreach (var value in item.Values)
                            {
                                csv.WriteField(value?.ToString());
                            }
                        }

                        csv.NextRecord();
                    }

                    csv.Flush();
                }

                return writer.ToString();
            }
        }


        public static string GetFormattedOutput(List<Dictionary<string, object>> records)
        {
            Dictionary<string, object>[] notNullRecords = records.Where(r => r != null).ToArray();
            if (!notNullRecords.Any())
            {
                return "No matching elements found";
            }

            var table = new ConsoleTable();
            // write headers
            Dictionary<string, object>.KeyCollection headers = notNullRecords.First().Keys;
            table.AddColumn(headers);

            var emptyRow = Enumerable.Repeat("", table.Columns.Count).ToArray();

            foreach (Dictionary<string, object> item in records)
            {
                if (item is null)
                {
                    table.AddRow(emptyRow);
                }
                else
                {
                    table.AddRow(item.Values.ToArray());
                }
            }

            return table.ToString();
        }
    }
}