﻿using System;
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
        private IReadOnlyList<string> _patternGroupNames;
        private const RegexOptions _defaultRegexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture;
        private readonly RegexOptions _regexOptions;

        private static readonly Regex _grokRegex = new Regex("%{(\\w+):(\\w+)(?::\\w+)?}", RegexOptions.Compiled);
        private static readonly Regex _grokRegexWithType = new Regex("%{(\\w+):(\\w+):(\\w+)?}", RegexOptions.Compiled);
        private static readonly Regex _grokWithoutName = new Regex("%{(\\w+)}", RegexOptions.Compiled);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Grok"/> class with the specified Grok pattern.
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        public Grok(string grokPattern)
        {
            _grokPattern = grokPattern ?? throw new ArgumentNullException(nameof(grokPattern));
            _patterns = new Dictionary<string, string>();
            _typeMaps = new Dictionary<string, string>();
            _regexOptions = _defaultRegexOptions;

            LoadPatterns();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Grok"/> class with the specified Grok pattern and regex options.
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        /// <param name="regexOptions">Additional regex options.</param>
        public Grok(string grokPattern, RegexOptions regexOptions)
            : this(grokPattern)
        {
            _regexOptions = _defaultRegexOptions | regexOptions;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Grok"/> class with the specified Grok pattern and custom patterns from a stream.
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        /// <param name="customPatterns">A stream containing custom patterns.</param>
        public Grok(string grokPattern, Stream customPatterns)
            : this(grokPattern)
        {
            LoadCustomPatterns(customPatterns);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Grok"/> class with the specified Grok pattern and custom patterns from a stream,
        ///     as well as additional regex options.
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        /// <param name="customPatterns">A stream containing custom patterns.</param>
        /// <param name="regexOptions">Additional regex options.</param>
        public Grok(string grokPattern, Stream customPatterns, RegexOptions regexOptions)
            : this(grokPattern, regexOptions)
        {
            LoadCustomPatterns(customPatterns);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Grok"/> class with the specified Grok pattern and custom patterns.
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        /// <param name="customPatterns">Custom patterns to add.</param>
        public Grok(string grokPattern, IDictionary<string, string> customPatterns)
            : this(grokPattern)
        {
            LoadCustomPatterns(customPatterns);
        }

        /// <summary>
        ///     Initialized a new instance of the <see cref="Grok"/> class with specified Grok pattern,
        ///     custom patterns if necessary, and custom <see cref="RegexOptions"/> .
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        /// <param name="customPatterns">Custom patterns to add.</param>
        /// <param name="regexOptions">Additional regex options.</param>
        public Grok(string grokPattern, IDictionary<string, string> customPatterns, RegexOptions regexOptions) 
            : this(grokPattern, regexOptions)
        {
            LoadCustomPatterns(customPatterns);
        }

        /// <summary>
        ///     Parses the input text using the defined Grok pattern and returns the parsed result.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>A <see cref="GrokResult"/> containing the parsed items.</returns>
        public GrokResult Parse(string text)
        {
            if (_compiledRegex == null)
            {
                ParsePattern();
            }

            var grokItems = new List<GrokItem>();

            foreach (Match match in _compiledRegex.Matches(text))
            {
                foreach (string groupName in _patternGroupNames)
                {
                    if (groupName != "0")
                    {
                        string groupValue = match.Groups[groupName].Value;

                        grokItems.Add(_typeMaps.TryGetValue(groupName, out string mappedType)
                            ? new GrokItem(groupName, MapType(mappedType, groupValue))
                            : new GrokItem(groupName, groupValue));
                    }
                }
            }

            return new GrokResult(grokItems);
        }

        private void AddPatternIfNotExists(string key, string value)
        {
            if (!_patterns.ContainsKey(key))
            {
                EnsurePatternIsValid(value);
                _patterns.Add(key, value);
            }
        }

        private void ParsePattern()
        {
            string pattern = string.Empty;
            bool done;

            do
            {
                done = false;

                ProcessTypeMappings(ref pattern);

                string newPattern = _grokWithoutName.Replace(_grokRegex.Replace(string.IsNullOrEmpty(pattern) ? _grokPattern : pattern, ReplaceWithName), ReplaceWithoutName);

                if (newPattern.Equals(pattern, StringComparison.CurrentCultureIgnoreCase))
                {
                    done = true;
                }

                pattern = newPattern;
            } while (!done);

            _compiledRegex = new Regex(pattern, _regexOptions);
            _patternGroupNames = _compiledRegex.GetGroupNames().ToList();
        }

        private void ProcessTypeMappings(ref string pattern)
        {
            MatchCollection matches = _grokRegexWithType.Matches(string.IsNullOrEmpty(pattern) ? _grokPattern : pattern);
            foreach (Match match in matches)
            {
                _typeMaps.Add(match.Groups[2].Value, match.Groups[3].Value);
            }
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
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith("grok-patterns"))
                {
                    using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                    {
                        using (var sr = new StreamReader(resourceStream, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                ProcessPatternLine(sr.ReadLine());
                            }
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

        private void LoadCustomPatterns(IDictionary<string, string> customPatterns)
        {
            foreach (var pattern in customPatterns)
            {
                AddPatternIfNotExists(pattern.Key, pattern.Value);
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

        private static void EnsurePatternIsValid(string pattern)
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
