﻿using System.Globalization;
using System.Runtime.CompilerServices;
using System.Speech.Recognition;
using System.Text;
using System.Text.RegularExpressions;
using static MiniAirwaysVoiceControl.GrammaVoiceRecog;
using static MiniAirwaysVoiceControl.MiniAirwaysVoiceControlInterface;

namespace MiniAirwaysVoiceControl
{
    public class AircraftGrammarBuilder
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        Choices NumberChoices;
        Choices RunwayDirectionChoice;
        Choices AlphabetChoice;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        List<string> AircraftGetStatusRuleBase = [];
        List<string> AircraftTakeoffRuleBase = [];
        List<string> AircraftLandingRuleBase = [];
        List<string> AircraftFlyHeadingRuleBase = [];
        List<string> AircraftVectorToDestinationWaypointRuleBase = [];
        List<string> AircraftVectorToNormalWaypointRuleBase = [];

        List<string> AirlineCodes = [];
        List<string> AirlineCallsigns = [];
        List<string> NamedWaypoints = [];
        List<string> Runways = [];
        List<string> RunwaySpells = [];

        const string AircraftElement = "{AIRCRAFT}";
        const string NamedWaypointElement = "{WAYPOINT_D}";
        const string NormalWaypointElement = "{WAYPOINT_N}";
        const string RunwayElement = "{RUNWAY}";
        const string HeadingElement = "{HEADING}";

        public void Init()
        {
            NumberChoices = new Choices(new string[] {
                "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero"
            });
            RunwayDirectionChoice = new Choices(new string[] {
                "left", "right", "center"
            });
            AlphabetChoice = new Choices(new string[] {
                "al-fuh", "how-tel", "li-ma", "tang-gow"
            });
        }

        public void SetRules(GrammarStruct grammarStruct)
        {
            AircraftGetStatusRuleBase = [];
            AircraftTakeoffRuleBase = [];
            AircraftLandingRuleBase = [];
            AircraftFlyHeadingRuleBase = [];
            AircraftVectorToDestinationWaypointRuleBase = [];
            AircraftVectorToNormalWaypointRuleBase = [];

            foreach (string rule in grammarStruct.AircraftStatRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1)
                    AircraftGetStatusRuleBase.Add(rule);
            }

            foreach (string rule in grammarStruct.AircraftTakeoffRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, RunwayElement) == 1)
                    AircraftTakeoffRuleBase.Add(rule);
            }

            foreach (string rule in grammarStruct.AircraftLandingRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, RunwayElement) == 1)
                    AircraftLandingRuleBase.Add(rule);
            }

            foreach (string rule in grammarStruct.AircraftFlyHeadingRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, HeadingElement) == 1)
                    AircraftFlyHeadingRuleBase.Add(rule);
            }

            foreach (string rule in grammarStruct.AircraftVectorToDestinationWaypointRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, NamedWaypointElement) == 1)
                    AircraftVectorToDestinationWaypointRuleBase.Add(rule);
            }

            foreach (string rule in grammarStruct.AircraftVectorToNormalWaypointRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, NormalWaypointElement) == 1)
                    AircraftVectorToNormalWaypointRuleBase.Add(rule);
            }
        }

        public Grammar[] CreateGrammar(GrammarSource gs)
        {
            AirlineCallsigns = [.. gs.Airlines];
            AirlineCodes = [.. gs.AirlineCodes];
            NamedWaypoints = [.. gs.NamedWaypoints];

            List<Grammar> grammars = [];
            Choices AirlineChoice = new Choices(gs.Airlines);
            Choices NamedWaypointChoice = new Choices(gs.NamedWaypoints);


            GrammarBuilder AircraftElement = new GrammarBuilder();
            AircraftElement.Append(new GrammarBuilder(AirlineChoice));
            AircraftElement.Append(new GrammarBuilder(NumberChoices));
            AircraftElement.Append(new GrammarBuilder(NumberChoices));
            AircraftElement.Append(new GrammarBuilder(NumberChoices));

            GrammarBuilder NamedWaypointElement = new GrammarBuilder();
            NamedWaypointElement.Append(new GrammarBuilder(NamedWaypointChoice));

            GrammarBuilder NormalWaypointElement = new GrammarBuilder();
            NormalWaypointElement.Append(new GrammarBuilder(AlphabetChoice));
            NormalWaypointElement.Append(new GrammarBuilder(NumberChoices));
            NormalWaypointElement.Append(new GrammarBuilder(NumberChoices));

            GrammarBuilder HeadingElement = new GrammarBuilder();
            HeadingElement.Append(NumberChoices);
            HeadingElement.Append(NumberChoices);
            HeadingElement.Append(NumberChoices);

            GrammarBuilder RunwayElement = new GrammarBuilder();
            
            if (gs.RunwayNames.Length == 0)
            {
                RunwayElement.Append("Runway");
                RunwayElement.Append(new GrammarBuilder(NumberChoices, 1, 2));
                RunwayElement.Append(new GrammarBuilder(RunwayDirectionChoice, 0, 1));
            }
            else
            {
                Runways = gs.RunwayNames.ToList();
                string[] rwys = ParseUtil.RunwayNameToSpell(gs.RunwayNames);
                RunwaySpells = rwys.ToList();
                RunwayElement.Append(new GrammarBuilder(new Choices(rwys)));
            }


            // No Command, Show flying path
            for (int i = 0; i < AircraftGetStatusRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftGetStatusRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AGS" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Takeoff
            for (int i = 0; i < AircraftTakeoffRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftTakeoffRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo("en-US"));
                grammar.Name = "ATO" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Land
            for (int i = 0; i < AircraftLandingRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftLandingRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo("en-US"));
                grammar.Name = "ALD" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Vector to Destination Waypoint
            for (int i = 0; i < AircraftVectorToDestinationWaypointRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftVectorToDestinationWaypointRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AVD" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Vector to Normal Waypoint
            for (int i = 0; i < AircraftVectorToNormalWaypointRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftVectorToNormalWaypointRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AVW" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft FlyHeading
            for (int i = 0; i < AircraftFlyHeadingRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftFlyHeadingRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AFH" + i.ToString();
                grammars.Add(grammar);
            }

            return grammars.ToArray();
        }

        string MatchAircraftPattern(string s)
        {
            return ParseUtil.TransformSubstrings(AirlineCallsigns, AirlineCodes, [], [], s, 3, 4).FirstOrDefault(string.Empty);
        }

        string MatchRunwayPattern(string s)
        {
            for (int i=0; i<Runways.Count; i++)
            {
                if (s.ToLower().Contains(RunwaySpells[i].ToLower()))
                {
                    return Runways[i];
                }
            }
            return string.Empty;
        }

        string MatchHeadingPattern(string s)
        {
            return ParseUtil.TransformSubstrings([], [], [], [], s, 3, 3).LastOrDefault(string.Empty);
        }

        string MatchNamedWaypointPattern(string s)
        {
            foreach (var waypoint in this.NamedWaypoints)
            {
                if (s.ToLower().Contains(waypoint.ToLower()))
                {
                    return waypoint;
                }
            }
            return string.Empty;
        }

        string MatchNormalWaypointPattern(string s)
        {
            return ParseUtil.TransformSubstrings(["al-fuh", "how-tel", "li-ma", "tang-gow"], ["A", "H", "L", "T"], [], [], s, 2, 2).FirstOrDefault(string.Empty);
        }

        public SRResult ExtractGrammar(ResultType rt, string grammarName, string s)
        {
            GrammarType gt = GrammarType.Invalid;
            if (rt == ResultType.Rejected || string.IsNullOrEmpty(s))
            {
                return new SRResult()
                {
                    Type = rt,
                    Grammar = gt,
                    Message = "",
                    Aircraft = "",
                    Waypoint = "",
                    Heading = "",
                    Runway = ""
                };
            }

            switch (grammarName.Substring(0, 3))
            {
                case "AGS":
                    gt = GrammarType.AircraftStat;
                    break;
                case "ATO":
                    gt = GrammarType.AircraftTakeoff;
                    break;
                case "ALD":
                    gt = GrammarType.AircraftLanding;
                    break;
                case "AFH":
                    gt = GrammarType.AircraftFlyHeading;
                    break;
                case "AVD":
                    gt = GrammarType.AircraftVectorToDestinationWaypoint;
                    break;
                case "AVW":
                    gt = GrammarType.AircraftVectorToNormalWaypoint;
                    break;
                default:
                    break;
            }

            string aircraft = MatchAircraftPattern(s);
            string runway = MatchRunwayPattern(s);
            string heading = MatchHeadingPattern(s);
            string waypoint = "";
            if (gt == GrammarType.AircraftVectorToDestinationWaypoint)
                waypoint = MatchNamedWaypointPattern(s);
            else if (gt == GrammarType.AircraftVectorToNormalWaypoint)
                waypoint = MatchNormalWaypointPattern(s);

            return new SRResult()
            {
                Type = rt,
                Grammar = gt,
                Message = s,
                Aircraft = aircraft,
                Waypoint = waypoint,
                Heading = heading,
                Runway = runway
            };
        }


    }

    public class CSRParser
    {
        public static Grammar ParseCSR(string csr, GrammarBuilder aircraftElement, GrammarBuilder runwayElement, GrammarBuilder headingElement, GrammarBuilder destinationWaypointElement, GrammarBuilder normalWaypointElement, CultureInfo cultureInfo)
        {
            GrammarBuilder gb = new GrammarBuilder();
            int lastIndex = 0;

            // Regex to match tokens: {element}, <choice1|choice2>, [optional]
            Regex pattern = new Regex(@"\{[^\}]+\}|\<[^\>]+\>|$$[^$$]+\]");

            MatchCollection matches = pattern.Matches(csr);

            foreach (Match match in matches)
            {
                // Append any plain text before the current match
                if (match.Index > lastIndex)
                {
                    string text = csr.Substring(lastIndex, match.Index - lastIndex).Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        gb.Append(text);
                    }
                }

                string token = match.Value;
                if (token.StartsWith("{") && token.EndsWith("}"))
                {
                    // Handle predefined elements, assuming speach engine will capture its value via semantic keys
                    string element = token.Trim('{', '}');
                    switch (element)
                    {
                        case "AIRCRAFT":
                            gb.Append(aircraftElement);
                            break;
                        case "RUNWAY":
                            gb.Append(runwayElement);
                            break;
                        case "HEADING":
                            gb.Append(headingElement);
                            break;
                        case "WAYPOINT_D":
                            gb.Append(destinationWaypointElement);
                            break;
                        case "WAYPOINT_N":
                            gb.Append(normalWaypointElement);
                            break;
                        default:
                            gb.Append(token); break;
                    }
                }
                else if (token.StartsWith("<") && token.EndsWith(">"))
                {
                    // Handle choices
                    string[] choices = token.Trim('<', '>').Split('|');
                    gb.Append(new Choices(choices));
                }
                else if (token.StartsWith("[") && token.EndsWith("]"))
                {
                    // Handle optional sections, with the optional content appearing zero or one time
                    string optionalContent = token.Trim('[', ']');
                    gb.Append(new GrammarBuilder(optionalContent, 0, 1));
                }

                // Update last index
                lastIndex = match.Index + match.Length;
            }

            // Append any remaining text after the last match
            if (lastIndex < csr.Length)
            {
                string text = csr.Substring(lastIndex).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    gb.Append(text);
                }
            }

            gb.Culture = cultureInfo;
            return new Grammar(gb);
        }

        public static int CountOccurrences(string stringA, string stringB)
        {
            if (string.IsNullOrEmpty(stringB))
            {
                return 0;
            }

            string pattern = Regex.Escape(stringB);
            return Regex.Matches(stringA, pattern).Count;
        }
    }

    public class ParseUtil
    {
        private static readonly Dictionary<string, string> wordToNumber = new Dictionary<string, string>
    {
        { "zero", "0" }, { "one", "1" }, { "two", "2" },
        { "three", "3" }, { "four", "4" }, { "five", "5" },
        { "six", "6" }, { "seven", "7" }, { "eight", "8" },
        { "nine", "9" }
    };

        private static readonly Dictionary<string, string> NumberToWord = new Dictionary<string, string>
    {
        { "0", "zero" }, { "1", "one" },  { "2", "two" },
        { "3", "three" }, { "4", "four" }, { "5", "five" },
        { "6", "six" }, { "7", "seven" }, { "8", "eight" },
        { "9", "nine" }
    };

        private static readonly Dictionary<string, string> LCRToWord = new Dictionary<string, string>
    {
        { "L", "left" }, { "C", "center" },  { "R", "right" }
    };


        public static List<string> TransformSubstrings(List<string> A, List<string> B, List<string> C, List<string> D, string S, int min, int max)
        {
            List<string> results = new List<string>();

            // 所有数字单词的模式
            string numberWordPattern = string.Join("|", wordToNumber.Keys.Select(Regex.Escape));
            // 匹配连续的数字单词，出现次数在 min 和 max 之间
            string numberPattern = $@"((?:(?:{numberWordPattern})\s?){{{min},{max}}})";

            if (A.Count > 0 && B.Count > 0 && A.Count == B.Count)
            {
                // 构建 A 列表中单词的正则模式
                string wordPattern = string.Join("|", A.Select(Regex.Escape));
                // 综合模式：匹配 A 列表中的单词，后接数字单词序列，可能后接一个后缀词
                string pattern = $@"\b({wordPattern})\b\s?{numberPattern}";
                if (C.Count > 0 && D.Count > 0 && C.Count == D.Count)
                {
                    pattern += @"\s?(\b(?:\w+)\b)";
                }

                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

                foreach (Match match in regex.Matches(S))
                {
                    string substring = match.Value;

                    // 替换 A 中的单词为 B 中对应的单词
                    for (int i = 0; i < A.Count; i++)
                    {
                        string aWord = A[i];
                        string bWord = B[i];
                        substring = Regex.Replace(substring, @"\b" + Regex.Escape(aWord) + @"\b", bWord, RegexOptions.IgnoreCase);
                    }

                    // 替换数字单词为对应的数字
                    substring = Regex.Replace(substring, numberPattern, m =>
                    {
                        var numberWords = m.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        return string.Join("", numberWords.Select(w => wordToNumber[w.ToLower()]));
                    });

                    // 处理后缀词
                    if (C.Count > 0 && D.Count > 0 && C.Count == D.Count)
                    {
                        string followingWord = match.Groups[3].Value;
                        if (C.Contains(followingWord, StringComparer.OrdinalIgnoreCase))
                        {
                            int index = C.FindIndex(c => string.Equals(c, followingWord, StringComparison.OrdinalIgnoreCase));
                            if (index != -1)
                            {
                                // 替换为 D 中对应的单词
                                substring = Regex.Replace(substring, @"\b" + Regex.Escape(followingWord) + @"\b", D[index], RegexOptions.IgnoreCase);
                            }
                        }
                        else
                        {
                            // 如果后缀词不在 C 中，则移除它
                            substring = Regex.Replace(substring, @"\b" + Regex.Escape(followingWord) + @"\b", "", RegexOptions.IgnoreCase).Trim();
                        }
                    }

                    results.Add(substring);
                }
            }
            else
            {
                // 当 A 和 B 为空时，只匹配数字序列，可能后接一个后缀词
                string pattern = numberPattern;
                if (C.Count > 0 && D.Count > 0 && C.Count == D.Count)
                {
                    pattern += @"\s?(\b(?:\w+)\b)";
                }
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

                foreach (Match match in regex.Matches(S))
                {
                    // 将数字单词转换为数字，并移除空格
                    string numbersWithoutSpaces = Regex.Replace(match.Groups[1].Value, $@"(?:{numberWordPattern})", m =>
                    {
                        return wordToNumber[m.Value.ToLower()];
                    }).Replace(" ", "");

                    string result = numbersWithoutSpaces;

                    if (C.Count > 0 && D.Count > 0 && C.Count == D.Count)
                    {
                        string followingWord = match.Groups[2].Value;
                        if (C.Contains(followingWord, StringComparer.OrdinalIgnoreCase))
                        {
                            int index = C.FindIndex(c => string.Equals(c, followingWord, StringComparison.OrdinalIgnoreCase));
                            if (index != -1)
                            {
                                // 在结果中添加替换后的后缀词
                                result += " " + D[index];
                            }
                            // 如果后缀词不在 C 中，则不添加任何内容（即移除它）
                        }
                    }

                    results.Add(result.Trim());
                }
            }
            return results;
        }

        public static string[] RunwayNameToSpell(string[] names)
        {
            List<string> ret = new();
            foreach (string name in names)
            {
                StringBuilder sb = new();
                sb.Append("Runway ");
                foreach (char ch in name)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    if (NumberToWord.TryGetValue(ch.ToString(), out string sp))
                    {
                        sb.Append(sp);
                    }
                    else if (LCRToWord.TryGetValue(ch.ToString(), out string sd))
                    {
                        sb.Append(sd);
                    }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    sb.Append(" ");
                }
                ret.Add(sb.ToString().Trim());
            }
            return ret.ToArray();
        }
    }
}


