using System.Globalization;
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
        public static string CN_Numbers = "洞腰两三四五六拐八鸠";
        public static string[] AlphabetSpells = { "a-fa", "al-fuh", "how-tel", "li-ma", "tang-gow" };
        public static string[] AlphabetNames = { "A", "A", "H", "L", "T" };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        Choices NumberChoices;
        Choices RunwayDirectionChoice;
        Choices AlphabetChoice;

        Choices NumberChoicesCN;
        Choices RunwayDirectionChoiceCN;
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

        public string InternalStringToHumanString(string s)
        {
            // replace any airline pronounciation with its code
            foreach (string airline in AirlineCallsigns)
            {
                if (s.StartsWith(airline))
                {
                    s = s.Replace(airline, AirlineCodes[AirlineCallsigns.IndexOf(airline)]);
                    break;
                }
            }
            
            // replace any number to arabic number
            // EN and CN both
            string[] EN_Numbers = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            string ArabicNumbers = "0123456789";
            foreach (string number in EN_Numbers)
            {
                s = s.Replace(number, ArabicNumbers[EN_Numbers.ToList().IndexOf(number)].ToString());
            }
            foreach (char number in CN_Numbers.ToList())
            {
                s = s.Replace(number.ToString(), ArabicNumbers[CN_Numbers.IndexOf(number)].ToString());
            }
            
            string[] EN_LCR = new string[] { "left", "center", "right" };
            string[] CN_LCR = new string[] { "左", "中", "右" };
            string LCR = "LCR";
            foreach (string lcr in EN_LCR)
            {
                s = s.Replace(lcr, LCR[EN_LCR.ToList().IndexOf(lcr)].ToString());
            }
            foreach (string lcr in CN_LCR)
            {
                s = s.Replace(lcr, LCR[CN_LCR.ToList().IndexOf(lcr)].ToString());
            }
            
            return s;
        }
        
        public void Init()
        {
            NumberChoices = new Choices(new string[] {
                "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero"
            });
            RunwayDirectionChoice = new Choices(new string[] {
                "left", "right", "center"
            });
            AlphabetChoice = new Choices(AlphabetSpells);

            // convert CN Number string to a list of string character
            string[] CNSplit = new string[CN_Numbers.Length];
            for (int i = 0; i < CN_Numbers.Length; i++)
            {
                CNSplit[i] = CN_Numbers[i].ToString();
            }
            NumberChoicesCN = new Choices(CNSplit);
            RunwayDirectionChoiceCN = new Choices(new string[] {
                "左", "右", "中"
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

        public Grammar[] CreateGrammar(GrammarSource gs, string lang)
        {
            AirlineCallsigns = [.. gs.Airlines];
            AirlineCodes = [.. gs.AirlineCodes];
            NamedWaypoints = [.. gs.NamedWaypoints];

            List<Grammar> grammars = [];
            Choices AirlineChoice = new Choices(gs.Airlines);
            Choices NamedWaypointChoice = new Choices(gs.NamedWaypoints);


            GrammarBuilder AircraftElement = new GrammarBuilder();
            AircraftElement.Append(new GrammarBuilder(AirlineChoice));

            Choices _numberChoices = lang == "zh-CN" ? NumberChoicesCN : NumberChoices;
            AircraftElement.Append(new GrammarBuilder(_numberChoices));
            AircraftElement.Append(new GrammarBuilder(_numberChoices));
            AircraftElement.Append(new GrammarBuilder(_numberChoices));

            GrammarBuilder NamedWaypointElement = new GrammarBuilder();
            NamedWaypointElement.Append(new GrammarBuilder(NamedWaypointChoice));

            GrammarBuilder NormalWaypointElement = new GrammarBuilder();
            NormalWaypointElement.Append(new GrammarBuilder(AlphabetChoice));
            NormalWaypointElement.Append(new GrammarBuilder(_numberChoices));
            NormalWaypointElement.Append(new GrammarBuilder(_numberChoices));

            GrammarBuilder HeadingElement = new GrammarBuilder();
            HeadingElement.Append(_numberChoices);
            HeadingElement.Append(_numberChoices);
            HeadingElement.Append(_numberChoices);

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
                string[] rwys = ParseUtil.RunwayNameToSpell(gs.RunwayNames, lang);
                RunwaySpells = rwys.ToList();
                RunwayElement.Append(new GrammarBuilder(new Choices(rwys)));
            }


            // No Command, Show flying path
            for (int i = 0; i < AircraftGetStatusRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftGetStatusRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo(lang));
                grammar.Name = "AGS" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Takeoff
            for (int i = 0; i < AircraftTakeoffRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftTakeoffRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo(lang));
                grammar.Name = "ATO" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Land
            for (int i = 0; i < AircraftLandingRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftLandingRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo(lang));
                grammar.Name = "ALD" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Vector to Destination Waypoint
            for (int i = 0; i < AircraftVectorToDestinationWaypointRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftVectorToDestinationWaypointRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo(lang));
                grammar.Name = "AVD" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Vector to Normal Waypoint
            for (int i = 0; i < AircraftVectorToNormalWaypointRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftVectorToNormalWaypointRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo(lang));
                grammar.Name = "AVW" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft FlyHeading
            for (int i = 0; i < AircraftFlyHeadingRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftFlyHeadingRuleBase[i], AircraftElement, RunwayElement, HeadingElement, NamedWaypointElement, NormalWaypointElement, new CultureInfo(lang));
                grammar.Name = "AFH" + i.ToString();
                grammars.Add(grammar);
            }

            return grammars.ToArray();
        }

        string MatchAircraftPatternEN(string s)
        {
            return ParseUtil.TransformSubstrings(AirlineCallsigns, AirlineCodes, [], [], s, 3, 4).FirstOrDefault(string.Empty);
        }

        string MatchAircraftPatternCN(string s)
        {
            // 航司总是出现在句子的开头，因此先匹配是哪个航司
            int AirlineIndex = -1;
            foreach (string airline in AirlineCallsigns)
            {
                if (s.StartsWith(airline))
                {
                    AirlineIndex = AirlineCallsigns.IndexOf(airline);
                    break;
                }
            }
            if (AirlineIndex == -1)
            {
                return string.Empty;
            }
            string AirlineCode = AirlineCodes[AirlineIndex];

            // 匹配数字序列
            // // 删掉航司
            string ss = s.Substring(AirlineCallsigns[AirlineIndex].Length).Trim();
            // // 一个一个进行字符进行汉字转阿拉伯数字，直到不是数字
            string number = "";
            bool numberMatched = false;
            string WordToNumber = CN_Numbers;
            for (int i = 0; i < ss.Length; i++)
            {
                if (WordToNumber.Contains(ss[i].ToString()))
                {
                    number += WordToNumber.IndexOf(ss[i].ToString(), StringComparison.Ordinal);
                }
                else
                {
                    numberMatched = true;
                    break;
                }
                // 最多不超过四位数
                if (number.Length == 4)
                {
                    break;
                }
            }
            if (!numberMatched)
            {
                return string.Empty;
            }


            return AirlineCode + ' ' + number;
        }

        string MatchRunwayPatternEN(string s)
        {
            for (int i = 0; i < Runways.Count; i++)
            {
                if (s.ToLower().Contains(RunwaySpells[i].ToLower()))
                {
                    return Runways[i];
                }
            }
            return string.Empty;
        }

        string MatchRunwayPatternCN(string s)
        {
            for (int i = 0; i < Runways.Count; i++)
            {
                string ss = RunwaySpells[i];
                ss = ss.Replace(" ", "");
                if (s.ToLower().Contains(ss))
                {
                    return Runways[i];
                }
            }
            return string.Empty;
        }

        string MatchHeadingPatternEN(string s)
        {
            return ParseUtil.TransformSubstrings([], [], [], [], s, 3, 3).LastOrDefault(string.Empty);
        }

        string MatchHeadingPatternCN(string s)
        {
            // 首先找到“航向”两个字
            int headingIndex = s.IndexOf("航向", StringComparison.Ordinal);
            if (headingIndex == -1)
            {
                return string.Empty;
            }

            string number = "";
            string WordToNumber = CN_Numbers;
            // 找到“航向”后面的数字序列，一共三位
            try
            {
                for (int i = headingIndex + 2; i < s.Length; i++)
                {
                    if (WordToNumber.Contains(s[i].ToString()))
                    {
                        number += WordToNumber.IndexOf(s[i].ToString(), StringComparison.Ordinal);
                    }
                    else
                    {
                        // 后接的都不是数字，哪里出错了
                        return string.Empty;
                    }
                    if (number.Length == 3)
                    {
                        return number;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }

            return string.Empty;
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

        string MatchNormalWaypointPatternEN(string s)
        {
            return ParseUtil.TransformSubstrings(new List<string>(AlphabetSpells), new List<string>(AlphabetNames), [], [], s, 2, 2).FirstOrDefault(string.Empty);
        }

        string MatchNormalWaypointPatternCN(string s)
        {
            // 首先找到“飞航点”
            int waypointIndex = s.IndexOf("飞航点", StringComparison.Ordinal);
            if (waypointIndex == -1)
            {
                return string.Empty;
            }
            string ss = s.Substring(waypointIndex + 3).Trim().ToLower();

            // 接下来要匹配到"al-fuh", "how-tel", "li-ma", "tang-gow"其中一个
            // 匹配不到直接失败
            string waypoint = "";
            string waypointSpell = "";
            bool matched1 = false;
            string[] waypointSpells = AlphabetSpells;
            string[] waypointNames = AlphabetNames;
            foreach (string spell in waypointSpells)
            {
                if (ss.StartsWith(spell))
                {
                    waypoint = waypointNames[Array.IndexOf(waypointSpells, spell)];
                    waypointSpell = spell;
                    matched1 = true;
                    break;
                }
            }
            if (!matched1)
            {
                return string.Empty;
            }

            // 匹配数字序列, 两位
            string sss = ss.Substring(waypointSpell.Length).Trim();
            string number = "";
            string WordToNumber = CN_Numbers;
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    if (WordToNumber.Contains(sss[i].ToString()))
                    {
                        number += WordToNumber.IndexOf(sss[i].ToString(), StringComparison.Ordinal);
                    }
                    else
                    {
                        // 后接的都不是数字，哪里出错了
                        return string.Empty;
                    }

                    if (number.Length == 2)
                    {
                        return waypoint + " " + number;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }

            return string.Empty;
        }

        public SRResult ExtractGrammar(ResultType rt, string grammarName, string s, string lang)
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

            string aircraft, runway, heading, waypoint;

            if (lang == "en-US")
            {
                aircraft = MatchAircraftPatternEN(s);
                runway = MatchRunwayPatternEN(s);
                heading = MatchHeadingPatternEN(s);
                waypoint = "";
                if (gt == GrammarType.AircraftVectorToDestinationWaypoint)
                    waypoint = MatchNamedWaypointPattern(s);
                else if (gt == GrammarType.AircraftVectorToNormalWaypoint)
                    waypoint = MatchNormalWaypointPatternEN(s);
            }
            else if (lang == "zh-CN")
            {
                aircraft = MatchAircraftPatternCN(s);
                runway = MatchRunwayPatternCN(s);
                heading = MatchHeadingPatternCN(s);
                waypoint = "";
                if (gt == GrammarType.AircraftVectorToDestinationWaypoint)
                    waypoint = MatchNamedWaypointPattern(s);
                else if (gt == GrammarType.AircraftVectorToNormalWaypoint)
                    waypoint = MatchNormalWaypointPatternCN(s);
            }
            else
            {
                throw new ArgumentException("Unsupported language");
            }

            Console.WriteLine($"Grammar: {gt}, Aircraft: {aircraft}, Waypoint: {waypoint}, Heading: {heading}, Runway: {runway}");

            return new SRResult()
            {
                Type = rt,
                Grammar = gt,
                Message = InternalStringToHumanString(s),
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
        public static readonly Dictionary<string, string> WordToNumberEN = new Dictionary<string, string>
        {
            { "zero", "0" }, { "one", "1" }, { "two", "2" },
            { "three", "3" }, { "four", "4" }, { "five", "5" },
            { "six", "6" }, { "seven", "7" }, { "eight", "8" },
            { "nine", "9" }
        };

        public static readonly Dictionary<string, string> NumberToWordEN = new Dictionary<string, string>
        {
            { "0", "zero" }, { "1", "one" },  { "2", "two" },
            { "3", "three" }, { "4", "four" }, { "5", "five" },
            { "6", "six" }, { "7", "seven" }, { "8", "eight" },
            { "9", "nine" }
        };

        public static readonly Dictionary<string, string> NumberToWordCN = new Dictionary<string, string>
        {
            { "0", "洞" }, { "1", "腰" },  { "2", "两" },
            { "3", "三" }, { "4", "四" }, { "5", "五" },
            { "6", "六" }, { "7", "拐" }, { "8", "八" },
            { "9", "鸠" }
        };

        public static readonly Dictionary<string, string> WordToNumberCN = new Dictionary<string, string>
            {
                { "腰", "1" }, { "两", "2" }, { "三", "3" },
                { "四", "4" }, { "五", "5" }, { "六", "6" },
                { "拐", "7" }, { "八", "8" }, { "鸠", "9" }
            };

        public static readonly Dictionary<string, string> LCRToWordEN = new Dictionary<string, string>
        {
            { "L", "left" }, { "C", "center" },  { "R", "right" }
        };

        public static readonly Dictionary<string, string> LCRToWordCN = new Dictionary<string, string>
        {
            { "L", "左" }, { "C", "中" }, { "R", "右" }
        };


        public static List<string> TransformSubstrings(List<string> A, List<string> B, List<string> C, List<string> D, string S, int min, int max)
        {
            List<string> results = new List<string>();

            // 所有数字单词的模式
            string numberWordPattern = string.Join("|", WordToNumberEN.Keys.Select(Regex.Escape));
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
                        return string.Join("", numberWords.Select(w => WordToNumberEN[w.ToLower()]));
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
                        return WordToNumberEN[m.Value.ToLower()];
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

        public static string[] RunwayNameToSpell(string[] names, string lang)
        {
            List<string> ret = new();
            foreach (string name in names)
            {
                StringBuilder sb = new();
                sb.Append(lang == "en-US" ? "Runway " : "跑道");
                foreach (char ch in name)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    if (lang == "en-US")
                    {
                        if (NumberToWordEN.TryGetValue(ch.ToString(), out string sp))
                        {
                            sb.Append(sp);
                        }
                        else if (LCRToWordEN.TryGetValue(ch.ToString(), out string sd))
                        {
                            sb.Append(sd);
                        }
                    }
                    else if (lang == "zh-CN")
                    {
                        if (NumberToWordCN.TryGetValue(ch.ToString(), out string sp))
                        {
                            sb.Append(sp);
                        }
                        else if (LCRToWordCN.TryGetValue(ch.ToString(), out string sd))
                        {
                            sb.Append(sd);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported language");
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


