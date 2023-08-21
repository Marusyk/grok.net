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
        private readonly RegexOptions _regexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture;

        private static readonly Regex _grokRegex = new Regex("%{(\\w+):(\\w+)(?::\\w+)?}", RegexOptions.Compiled);
        private static readonly Regex _grokRegexWithType = new Regex("%{(\\w+):(\\w+):(\\w+)?}", RegexOptions.Compiled);
        private static readonly Regex _grokWithoutName = new Regex("%{(\\w+)}", RegexOptions.Compiled);

        public Grok(string grokPattern)
        {
            _grokPattern = grokPattern;
            _patterns = new Dictionary<string, string>();
            _typeMaps = new Dictionary<string, string>();

            LoadPatterns();
        }

        public Grok(string grokPattern, Stream customPatterns) : this(grokPattern)
        {
            LoadCustomPatterns(customPatterns);
        }

        public Grok(string grokPattern, IDictionary<string, string> customPatterns) : this(grokPattern)
        {
            AddPatterns(customPatterns);
        }

        public Grok(string grokPattern, IDictionary<string, string> customPatterns, RegexOptions regexOptions): this(grokPattern, customPatterns)
        {
            _regexOptions |= regexOptions;
        }

        private void AddPatterns(IDictionary<string, string> customPatterns)
        {
            if (customPatterns == null)
            {
                return;
            }

            foreach (var pattern in customPatterns)
            {
                AddPatternIfNotExists(pattern.Key, pattern.Value);
            }
        }

        private void AddPatternIfNotExists(string key, string value)
        {
            if (!_patterns.ContainsKey(key))
            {
                EnsurePatternIsValid(value);
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

                MatchCollection matches =
                    _grokRegexWithType.Matches(string.IsNullOrEmpty(pattern) ? _grokPattern : pattern);
                foreach (Match match in matches)
                {
                    _typeMaps.Add(match.Groups[2].Value, match.Groups[3].Value);
                }

                string str = _grokWithoutName.Replace(
                    _grokRegex.Replace(string.IsNullOrEmpty(pattern) ? _grokPattern : pattern, ReplaceWithName),
                    ReplaceWithoutName);
                if (str.Equals(pattern, StringComparison.CurrentCultureIgnoreCase))
                {
                    flag = true;
                }

                pattern = str;
            } while (!flag);

            _compiledRegex = new Regex(pattern, _regexOptions);
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
                    using (var sr = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName),
                               Encoding.UTF8))
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
                throw new FormatException("Pattern line was not in a correct form (two strings split by space)");
            }

            if (strArray[0].StartsWith("#"))
            {
                return;
            }

            AddPatternIfNotExists(strArray[0], strArray[1]);
        }

        private void EnsurePatternIsValid(string pattern)
        {
            try
            {
                _ = Regex.Match("", pattern);
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid regular expression {pattern}", e);
            }
        }

        private string ReplaceWithName(Match match)
        {
            Group group1 = match.Groups[2];
            Group group2 = match.Groups[1];

            return _patterns.TryGetValue(group2.Value, out var str) ? $"(?<{group1}>{str})" : $"(?<{group1}>)";
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