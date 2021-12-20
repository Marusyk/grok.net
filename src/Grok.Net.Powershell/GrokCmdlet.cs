using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace GrokNet.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Grok")]
    [Alias("grok")]
    public class GrokCmdlet : Cmdlet
    {
        [ValidateLength(1, int.MaxValue)]
        [Parameter(ParameterSetName = "default", Mandatory = true, ValueFromPipeline = true)]
        [Alias("i")]
        public string Input { get; set; }
        
        [ValidateLength(1, int.MaxValue)]
        [Parameter(ParameterSetName = "file", Mandatory = true, ValueFromPipeline = true)]
        [Alias("p")]
        public string Path { get; set; }

        [ValidateLength(1, int.MaxValue)]
        [Parameter(ParameterSetName = "default", Mandatory = true)]
        [Parameter(ParameterSetName = "file", Mandatory = true)]
        [Alias("f")]
        public string Filter { get; set; }
        
        [Parameter(ParameterSetName = "default")]
        [Parameter(ParameterSetName = "file")]
        [Alias("o")]
        public string OutputFormat { get; set; }

        [Parameter(ParameterSetName = "default")]
        [Parameter(ParameterSetName = "file")]
        [Alias("e")]
        public SwitchParameter IgnoreEmptyLines { get; set; }

        [Parameter(ParameterSetName = "default")]
        [Parameter(ParameterSetName = "file")]
        [Alias("u")]
        public SwitchParameter IgnoreUnmatched { get; set; }

        [Parameter(ParameterSetName = "default")]
        [Parameter(ParameterSetName = "file")]
        [Alias("d")]
        public string CsvDelimiter { get; set; } = ",";

        [Parameter(ParameterSetName = "default")]
        [Parameter(ParameterSetName = "file")]
        [Alias("j")]
        public SwitchParameter IndentJson { get; set; }

        [Parameter(ParameterSetName = "version", Mandatory = true)]
        [Alias("v")]
        public SwitchParameter Version { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // get version
            if (Version.IsPresent)
            {
                var version = typeof(Grok).Assembly.GetName().Version.ToString();
                WriteObject($"Grok.Net {version}");
                return;
            }

            // process input line by line
            var result = new List<Dictionary<string, object>>();

            if (Path != null)
            {
                ProcessFile(result);
            }
            else
            {
                ProcessString(result);
            }

            // print output
            if ("json".Equals(OutputFormat, StringComparison.InvariantCultureIgnoreCase))
            {
                var json = OutputHelper.GetJsonOutput(result, IndentJson.IsPresent);
                WriteObject(json);
            }
            else if ("csv".Equals(OutputFormat, StringComparison.InvariantCultureIgnoreCase))
            {
                var csv = OutputHelper.GetCsvOutput(result, CsvDelimiter);
                WriteObject(csv);
            }
            else
            {
                var output = OutputHelper.GetFormattedOutput(result);
                WriteObject(output);
            }
        }

        private void ProcessString(List<Dictionary<string, object>> result)
        {
            var grok = new Grok(Filter);

            var lines = Input.Split(new[] {Environment.NewLine},
                IgnoreEmptyLines.IsPresent ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

            foreach (string line in lines)
            {
                ProcessLine(grok, line, result);
            }
        }

        private void ProcessFile(List<Dictionary<string, object>> result)
        {
            var grok = new Grok(Filter);

            using (var fileStream = File.OpenRead(Path))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    ProcessLine(grok, line, result);
                }
            }
        }

        private void ProcessLine(Grok grok, string line, List<Dictionary<string, object>> result)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (!IgnoreEmptyLines.IsPresent)
                {
                    result.Add(null);
                }
                return;
            }

            var grokResult = grok.Parse(line);

            if (grokResult.Count == 0)
            {
                if (!IgnoreUnmatched.IsPresent)
                {
                    result.Add(null);
                }

                return;
            }

            var dictionary = new Dictionary<string, object>();

            foreach (GrokItem item in grokResult)
            {
                dictionary[item.Key] = item.Value;
            }

            result.Add(dictionary);
        }
    }
}