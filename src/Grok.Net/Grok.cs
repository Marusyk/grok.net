using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PCRE;

namespace GrokNet
{
    public class Grok
    {
        private readonly string _grokPattern;
        private readonly Dictionary<string, string> _patterns;
        private readonly Dictionary<string, string> _typeMaps;
        private PcreRegex _compiledRegex;
        private IReadOnlyList<string> _patternGroupNames;
        private const PcreOptions _defaultRegexOptions = PcreOptions.Compiled | PcreOptions.ExplicitCapture;
        private readonly PcreOptions _regexOptions;

        private static readonly PcreRegex _grokRegex = new PcreRegex("%{(\\w+)(?::(\\w+)(?::\\w+)?)?}", PcreOptions.Compiled);
        private static readonly PcreRegex _grokRegexWithType = new PcreRegex("%{(\\w+):(\\w+):(\\w+)?}", PcreOptions.Compiled);

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
        public Grok(string grokPattern, PcreOptions regexOptions)
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
        public Grok(string grokPattern, Stream customPatterns, PcreOptions regexOptions)
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
        ///     custom patterns if necessary, and custom <see cref="PcreOptions"/> .
        /// </summary>
        /// <param name="grokPattern">The Grok pattern to use.</param>
        /// <param name="customPatterns">Custom patterns to add.</param>
        /// <param name="regexOptions">Additional regex options.</param>
        public Grok(string grokPattern, IDictionary<string, string> customPatterns, PcreOptions regexOptions)
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
                ValidateGrokPattern();
                ParsePattern();
            }

            var grokItems = new List<GrokItem>();

            foreach (PcreMatch match in _compiledRegex.Matches(text))
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
            _typeMaps.Clear();
            string pattern = ExpandPattern(_grokPattern, new List<string>());

            _compiledRegex = new PcreRegex(pattern, _regexOptions);
            _patternGroupNames = _compiledRegex.PatternInfo.GroupNames.ToList();
        }

        private string ExpandPattern(string pattern, List<string> expansionPath)
        {
            ProcessTypeMappings(pattern);

            return _grokRegex.Replace(pattern, match => ExpandMatch(match, expansionPath));
        }

        private string ExpandMatch(PcreMatch match, List<string> expansionPath)
        {
            string patternName = match.Groups[1].Value;

            if (!_patterns.TryGetValue(patternName, out string rawPattern))
            {
                throw CreateMissingPatternException(patternName, expansionPath);
            }

            expansionPath.Add(patternName);
            string expanded;
            try
            {
                expanded = ExpandPattern(rawPattern, expansionPath);
            }
            finally
            {
                expansionPath.RemoveAt(expansionPath.Count - 1);
            }

            PcreGroup nameGroup = match.Groups[2];
            return nameGroup.Success ? $"(?<{nameGroup.Value}>{expanded})" : $"({expanded})";
        }

        private static FormatException CreateMissingPatternException(string patternName, List<string> expansionPath)
        {
            if (expansionPath.Count == 0)
            {
                return new FormatException($"Invalid Grok pattern: Pattern '{patternName}' not found.");
            }

            string path = string.Join(" -> ", expansionPath) + " -> " + patternName;
            return new FormatException($"Invalid Grok pattern: Pattern '{patternName}' not found (expansion path: {path}).");
        }

        private void ProcessTypeMappings(string pattern)
        {
            IEnumerable<PcreMatch> matches = _grokRegexWithType.Matches(pattern);
            foreach (PcreMatch match in matches)
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
                        return int.Parse(data, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    case "float":
                        return double.Parse(data, NumberStyles.Float, CultureInfo.InvariantCulture);
                    case "datetime":
                        return DateTime.Parse(data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
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
                _ = PcreRegex.Match("", pattern);
            }
            catch (Exception e)
            {
                throw new FormatException($"Invalid regular expression {pattern}", e);
            }
        }

        private void ValidateGrokPattern()
        {
            var grokPatternRegex = new PcreRegex("%\\{([^:}]+)(?::[^}]+)?(?::[^}]+)?\\}");
            IEnumerable<PcreMatch> matches = grokPatternRegex.Matches(_grokPattern);

            foreach (PcreMatch match in matches)
            {
                var patternName = match.Groups[1].Value;
                if (!_patterns.ContainsKey(patternName))
                {
                    throw new FormatException($"Invalid Grok pattern: Pattern '{patternName}' not found.");
                }
            }
        }
    }
}
