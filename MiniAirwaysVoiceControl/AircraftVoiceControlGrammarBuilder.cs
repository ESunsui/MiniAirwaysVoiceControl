using System.Globalization;
using System.Runtime.CompilerServices;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using static MiniAirwaysVoiceControl.GrammaVoiceRecog;
using static MiniAirwaysVoiceControl.MiniAirwaysVoiceControlInterface;

namespace MiniAirwaysVoiceControl
{
    internal class AircraftVoiceControlGrammarBuilder
    {
        Choices NumberChoices;
        Choices RunwayDirectionChoice;
        Choices AlphabetChoice;

        List<string> AircraftGetStatusRuleBase = new();
        List<string> AircraftTakeoffRuleBase = new();
        List<string> AircraftLandingRuleBase = new();
        List<string> AircraftFlyHeadingRuleBase = new();
        List<string> AircraftVectorToDestinationWaypointRuleBase = new();
        List<string> AircraftVectorToNormalWaypointRuleBase = new();

        List<string> AirlineCallsigns = new();
        List<string> NamedWaypoints = new();

        const string AircraftElement = "{AIRCRAFT}";
        const string NamedWaypointElement = "{WAYPOINT}";
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
                "alpha", "bravo", "charlie", "delta", "echo", "foxtrot", "golf", "hotel", "india",
                "juliet", "kilo", "lima", "mike", "november", "oscar", "papa", "quebec", "romeo",
                "sierra", "tango", "uniform", "victor", "whiskey", "x-ray", "yankee", "zulu"
            });
        }

        public void SetRules(GrammarStruct grammarStruct)
        {
            AircraftGetStatusRuleBase = new();
            AircraftTakeoffRuleBase = new();
            AircraftLandingRuleBase = new();
            AircraftFlyHeadingRuleBase = new();
            AircraftVectorToDestinationWaypointRuleBase = new();
            AircraftVectorToNormalWaypointRuleBase = new();

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
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, NamedWaypointElement) == 1)
                    AircraftVectorToNormalWaypointRuleBase.Add(rule);
            }
        }

        public Grammar[] CreateGrammar(string[] airlines, string[] waypoints)
        {
            AirlineCallsigns = new List<string>(airlines);
            NamedWaypoints = new List<string>(waypoints);

            List<Grammar> grammars = new List<Grammar>();
            Choices AirlineChoice = new Choices(airlines);
            Choices NamedWaypointChoice = new Choices(waypoints);
            

            GrammarBuilder AircraftElement = new GrammarBuilder();
            AircraftElement.Append(new GrammarBuilder(AirlineChoice));
            AircraftElement.Append(new GrammarBuilder(NumberChoices, 3, 3));

            GrammarBuilder NamedWaypointElement = new GrammarBuilder();
            NamedWaypointElement.Append(new GrammarBuilder(NamedWaypointChoice));

            GrammarBuilder NormalWaypointElement = new GrammarBuilder();
            NormalWaypointElement.Append(new GrammarBuilder(AlphabetChoice, 3, 3));

            GrammarBuilder HeadingElement = new GrammarBuilder();
            HeadingElement.Append(NumberChoices);
            HeadingElement.Append(NumberChoices);
            HeadingElement.Append(NumberChoices);

            GrammarBuilder RunwayElement = new GrammarBuilder();
            RunwayElement.Append(new GrammarBuilder(NumberChoices, 1, 2));
            RunwayElement.Append(new GrammarBuilder(RunwayDirectionChoice, 0, 1));

            // No Command, Show flying path
            for(int i = 0; i < AircraftGetStatusRuleBase.Count; i++)
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
            // Sanitize patterns in P for regular expression
            List<string> sanitizedPatterns = new List<string>();
            foreach (var pattern in this.AirlineCallsigns)
            {
                sanitizedPatterns.Add(Regex.Escape(pattern));
            }

            // Create a pattern to match any one of the patterns in P once
            string unionPattern = string.Join("|", sanitizedPatterns);
            string numberWords = "(one|two|three|four|five|six|seven|eight|nine|zero)";

            // Create the full regex pattern according to the rules specified
            string fullPattern = $@"^(?=.*\b({unionPattern})\b)(?:(?!(\1)).)*?(\bAirline\b)?(?!(\1)).*?(\bFlight\b)?(?!(\1)).*?({numberWords}(?:\s+{numberWords}){{1,3}})(?!(\1)).*?$";

            // Build the regex object with case insensitive option
            Regex regex = new Regex(fullPattern, RegexOptions.IgnoreCase);

            // Perform the match
            List<string> matches = regex.Matches(s).Cast<Match>().Select(m => m.Value).ToList();
            if (matches.Count > 0)
            {
                if (matches.Count > 1)
                    matches.Sort((a, b) => a.Length == b.Length ? s.IndexOf(b).CompareTo(s.IndexOf(a)) : b.Length.CompareTo(a.Length));
                return matches.First();
            }
            return string.Empty;
        }

        string MatchRunwayPattern(string s)
        {
            string numberWords = "(one|two|three|four|five|six|seven|eight|nine|zero)";
            string directionWords = "(left|right|center)";
            string fullPattern = $@"^(?:Runway\s?)?{numberWords}\s{numberWords}(?:\s{directionWords})?$";
            Regex regex = new Regex(fullPattern, RegexOptions.IgnoreCase);
            List<string> matches = regex.Matches(s).Cast<Match>().Select(m => m.Value).ToList();
            if (matches.Count > 0)
            {
                if (matches.Count > 1)
                    matches.Sort((a, b) => a.Length == b.Length ? s.IndexOf(b).CompareTo(s.IndexOf(a)) : b.Length.CompareTo(a.Length));
                return matches.First();
            }
            return string.Empty;
        }

        string MatchHeadingPattern(string s)
        {
            string numberWords = "(one|two|three|four|five|six|seven|eight|nine|zero)";
            string fullPattern = $@"^{numberWords}\s{numberWords}\s{numberWords}$";
            Regex regex = new Regex(fullPattern, RegexOptions.IgnoreCase);
            List<string> matches = regex.Matches(s).Cast<Match>().Select(m => m.Value).ToList();
            if (matches.Count > 0)
            {
                if (matches.Count > 1)
                    matches.Sort((a, b) => s.IndexOf(b).CompareTo(s.IndexOf(a)));
                return matches.First();
            }
            return string.Empty;
        }

        string MatchNamedWaypointPattern(string s)
        {
            foreach (var waypoint in this.NamedWaypoints)
            {
                if (s.Contains(waypoint))
                {
                    return waypoint;
                }
            }
            return string.Empty;
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
            string waypoint = MatchNamedWaypointPattern(s);

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
        
    }
}
