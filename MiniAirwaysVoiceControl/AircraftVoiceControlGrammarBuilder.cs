using System.Globalization;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using static MiniAirwaysVoiceControl.MiniAirwaysVoiceControlInterface;

namespace MiniAirwaysVoiceControl
{
    internal class AircraftVoiceControlGrammarBuilder
    {
        Choices NumberChoices;
        Choices RunwayDirectionChoice;
        Choices DirectionChoice;
        Choices AlphabetChoice;

        List<string> AircraftGetStatusRuleBase = new();
        List<string> AircraftTakeoffRuleBase = new();
        List<string> AircraftLandingRuleBase = new();
        List<string> AircraftFlyHeadingRuleBase = new();
        List<string> AircraftVectorToWaypointRuleBase = new();

        const string AircraftElement = "{Aircraft}";
        const string NamedWaypointElement = "{Waypoint}";
        const string RunwayElement = "{Runway}";
        const string HeadingElement = "{Heading}";


        public void Init()
        {
            NumberChoices = new Choices(new string[] { 
                "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero" 
            });
            RunwayDirectionChoice = new Choices(new string[] { 
                "Left", "Right", "Center" 
            });
            DirectionChoice = new Choices(new string[] { 
                "North", "South", "East", "West" , "Northwest", "Northeast", "Southwest", "Southeast"
            });
            AlphabetChoice = new Choices(new string[] { 
                "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India",
                "Juliet", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo",
                "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "X-ray", "Yankee", "Zulu" 
            });
        }

        public void SetRules(GrammarStruct grammarStruct)
        {
            AircraftGetStatusRuleBase = new();
            AircraftTakeoffRuleBase = new();
            AircraftLandingRuleBase = new();
            AircraftFlyHeadingRuleBase = new();
            AircraftVectorToWaypointRuleBase = new();

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

            foreach (string rule in grammarStruct.AircraftVectorToWaypointRules)
            {
                if (CSRParser.CountOccurrences(rule, AircraftElement) == 1 && CSRParser.CountOccurrences(rule, NamedWaypointElement) == 1)
                    AircraftVectorToWaypointRuleBase.Add(rule);
            }
        }

        public Grammar[] CreateGrammar(string[] airlines, string[] waypoints)
        {
            List<Grammar> grammars = new List<Grammar>();
            Choices AirlineChoice = new Choices(airlines);
            Choices NamedWaypointChoice = new Choices(waypoints);
            

            GrammarBuilder AircraftElement = new GrammarBuilder();
            AircraftElement.Append(new GrammarBuilder(AirlineChoice));
            AircraftElement.Append(new GrammarBuilder("Airline", 0, 1));
            AircraftElement.Append(new GrammarBuilder("Flight", 0, 1));
            AircraftElement.Append(new GrammarBuilder(NumberChoices, 2, 4));

            GrammarBuilder NamedWaypointElement = new GrammarBuilder();
            NamedWaypointElement.Append(new GrammarBuilder("Waypoint", 0, 1));
            NamedWaypointElement.Append(new GrammarBuilder(NamedWaypointChoice));

            GrammarBuilder NormalWaypointElement = new GrammarBuilder();
            NormalWaypointElement.Append(new GrammarBuilder(AlphabetChoice, 1, 3));

            GrammarBuilder WaypointElement = new GrammarBuilder(new Choices(NamedWaypointElement, NormalWaypointElement));

            GrammarBuilder HeadingElement = new GrammarBuilder();
            HeadingElement.Append(NumberChoices);
            HeadingElement.Append(NumberChoices);
            HeadingElement.Append(NumberChoices);

            GrammarBuilder RunwayElement = new GrammarBuilder();
            RunwayElement.Append(new GrammarBuilder("Runway", 0, 1));
            RunwayElement.Append(new GrammarBuilder(NumberChoices, 1, 2));
            RunwayElement.Append(new GrammarBuilder(RunwayDirectionChoice, 0, 1));

            // No Command, Show flying path
            for(int i = 0; i < AircraftGetStatusRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftGetStatusRuleBase[i], AircraftElement, RunwayElement, HeadingElement, WaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AGS" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Takeoff
            for (int i = 0; i < AircraftTakeoffRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftTakeoffRuleBase[i], AircraftElement, RunwayElement, HeadingElement, WaypointElement, new CultureInfo("en-US"));
                grammar.Name = "ATO" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Land
            for (int i = 0; i < AircraftLandingRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftLandingRuleBase[i], AircraftElement, RunwayElement, HeadingElement, WaypointElement, new CultureInfo("en-US"));
                grammar.Name = "ALD" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft Vector to Waypoint
            for (int i = 0; i < AircraftVectorToWaypointRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftVectorToWaypointRuleBase[i], AircraftElement, RunwayElement, HeadingElement, WaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AVW" + i.ToString();
                grammars.Add(grammar);
            }

            // Aircraft FlyHeading
            for (int i = 0; i < AircraftFlyHeadingRuleBase.Count; i++)
            {
                Grammar grammar = CSRParser.ParseCSR(AircraftFlyHeadingRuleBase[i], AircraftElement, RunwayElement, HeadingElement, WaypointElement, new CultureInfo("en-US"));
                grammar.Name = "AFH" + i.ToString();
                grammars.Add(grammar);
            }

            return grammars.ToArray();
        }
    }

    public class CSRParser
    {
        public static Grammar ParseCSR(string csr, GrammarBuilder aircraftElement, GrammarBuilder runwayElement, GrammarBuilder headingElement, GrammarBuilder WaypointElement, CultureInfo cultureInfo)
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
                        case "Aircraft":
                            gb.Append(aircraftElement);
                            break;
                        case "Runway":
                            gb.Append(runwayElement);
                            break;
                        case "Heading":
                            gb.Append(headingElement);
                            break;
                        case "Waypoint":
                            gb.Append(WaypointElement);
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
