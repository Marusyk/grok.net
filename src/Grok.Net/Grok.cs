using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GrokNet
{
    public class Grok
    {
        private readonly string _grokPattern;
        private readonly Dictionary<string, string> _patterns;
        private readonly Dictionary<string, string> _typeMaps;
        private Regex _compiledRegex;
        private List<string> _groupNames;

        private static readonly Regex _grokRegex = new Regex("%{(\\w+):(\\w+)(?::\\w+)?}", RegexOptions.Compiled);
        private static readonly Regex _grokRegexWithType = new Regex("%{(\\w+):(\\w+):(\\w+)?}", RegexOptions.Compiled);
        private static readonly Regex _grokWithoutName = new Regex("%{(\\w+)}", RegexOptions.Compiled);

        public IList<(string, string)> _patternViolations;
        public IList<(string, string)> PatternViolations => _patternViolations;
        
        public Grok(string grokPattern)
        {
            _grokPattern = grokPattern;
            _patterns = new Dictionary<string, string>();
            _typeMaps = new Dictionary<string, string>();
            _patternViolations = new List<(string, string)>();
            
            // loads custom patterns from assembly reference in default constructor...
            // is this a good idea?
            LoadPatterns();
        }

        public Grok(string grokPattern, Stream customPatterns)
            :this(grokPattern)
        {
            LoadCustomPatterns(customPatterns);
        }
        
        public Grok(string grokPattern, IEnumerable<(string, string)> customPatterns)
            :this(grokPattern)
        {
            AddPatterns(customPatterns);
        }

        private void AddPatterns(IEnumerable<(string, string)> customPatterns)
        {
            foreach (var (key, value) in customPatterns)
            {
                if (ValidatePattern(value))
                {
                    AddPatternIfNotExists(key, value);                    
                }
            }
        }
        
        private void AddPatternIfNotExists(string key, string value)
        {
            // check before adding to avoid an exception in case the same pattern is present in the custom patterns file.
            if (!_patterns.ContainsKey(key))
            {
                _patterns.Add(key, value);
            }
        }

        public GrokResult Parse(string text)
        {
            if (_compiledRegex == null)
            {
                ParseGrokString();
            }

            var grokItems = new List<GrokItem>();

            foreach (Match match in _compiledRegex.Matches(text))
            {
                foreach (string groupName in _groupNames)
                {
                    if (groupName != "0")
                    {
                        grokItems.Add(_typeMaps.ContainsKey(groupName)
                            ? new GrokItem(groupName, MapType(_typeMaps[groupName], match.Groups[groupName].Value))
                            : new GrokItem(groupName, match.Groups[groupName].Value));
                    }
                }
            }
            return new GrokResult(grokItems);
        }

        private void ParseGrokString()
        {
            string pattern = string.Empty;
            bool flag;

            do
            {
                flag = false;

                MatchCollection matches = _grokRegexWithType.Matches(string.IsNullOrEmpty(pattern) ? _grokPattern : pattern);
                foreach (Match match in matches)
                {
                    _typeMaps.Add(match.Groups[2].Value, match.Groups[3].Value);
                }

                string str = _grokWithoutName.Replace(_grokRegex.Replace(string.IsNullOrEmpty(pattern) ? _grokPattern : pattern, ReplaceWithName), ReplaceWithoutName);
                if (str.Equals(pattern, StringComparison.CurrentCultureIgnoreCase))
                {
                    flag = true;
                }

                pattern = str;

            } while (!flag);

            _compiledRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            _groupNames = _compiledRegex.GetGroupNames().ToList();
        }

        private static object MapType(string type, string data)
        {
            string lowerInvariant = type.ToLowerInvariant();
            try
            {
                switch (lowerInvariant)
                {
                    case "int":
                        return Convert.ToInt32(data);
                    case "float":
                        return Convert.ToDouble(data);
                    case "datetime":
                        return DateTime.Parse(data);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return data;
        }

        private void LoadPatterns()
        {
            Assembly assembly = typeof(Grok).GetTypeInfo().Assembly;
            foreach (var manifestResourceName in assembly.GetManifestResourceNames())
            {
                if (manifestResourceName.EndsWith("grok-patterns"))
                {
                    using (var sr = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName), Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            ProcessPatternLine(sr.ReadLine());
                        }
                    }
                }
            }
        }

        private void LoadCustomPatterns(Stream customPatterns)
        {
            using (var sr = new StreamReader(customPatterns, Encoding.UTF8, true))
            {
                while (!sr.EndOfStream)
                {
                    ProcessPatternLine(sr.ReadLine());
                }
            }
        }

        private void ProcessPatternLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            string[] strArray = line.Split(new[] { ' ' }, 2);
            if (strArray.Length != 2)
            {
                throw new FormatException("Custom pattern was not in a correct form");
            }

            if (strArray[0].Equals("#", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!ValidatePattern(strArray[1]))
            {
                return;
            }
            AddPatternIfNotExists(strArray[0], strArray[1]);
        }

        private bool ValidatePattern(string pattern)
        {
            try
            {
                _ = Regex.Match("", pattern);
            }
            catch(Exception e)
            {
                _patternViolations.Add((pattern, e.Message));
                return false;
            }
            return true;
        }

        private string ReplaceWithName(Match match)
        {
            Group group1 = match.Groups[2];
            Group group2 = match.Groups[1];

            if (_patterns.TryGetValue(group2.Value, out var str))
            {
                return $"(?<{group1}>{str})";
            }

            return $"(?<{group1}>)";
        }

        private string ReplaceWithoutName(Match match)
        {
            Group group = match.Groups[1];

            if (_patterns.TryGetValue(group.Value, out _))
            {
                string str = _patterns[group.Value];
                return $"({str})";
            }

            return "()";
        }
    }
}
