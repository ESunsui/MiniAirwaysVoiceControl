using System.Speech.Recognition;

namespace SpeechRecognitionApp
{
    public class AircraftVoiceControlGrammarBuilder
    {
        Choices NumberChoices;
        Choices RunwayDirectionChoice;
        Choices DirectionChoice;
        Choices AlphabetChoice;
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

        public Grammar[] CreateGrammar(string[] airlines, string[] waypoints)
        {
            List<Grammar> grammars = new List<Grammar>();
            Choices AirlineChoice = new Choices(airlines);
            Choices WaypointChoice = new Choices(waypoints);
            

            GrammarBuilder AircraftElement = new GrammarBuilder();
            AircraftElement.Append(new GrammarBuilder(AirlineChoice));
            AircraftElement.Append(new GrammarBuilder("Airline", 0, 1));
            AircraftElement.Append(new GrammarBuilder("Flight", 0, 1));
            AircraftElement.Append(new GrammarBuilder(NumberChoices, 2, 4));

            GrammarBuilder NamedWaypointElement = new GrammarBuilder();
            NamedWaypointElement.Append(new GrammarBuilder("Waypoint", 0, 1));
            NamedWaypointElement.Append(new GrammarBuilder(WaypointChoice));

            GrammarBuilder RunwayElement = new GrammarBuilder();
            RunwayElement.Append(new GrammarBuilder("Runway", 0, 1));
            RunwayElement.Append(new GrammarBuilder(NumberChoices, 1, 2));
            RunwayElement.Append(new GrammarBuilder(RunwayDirectionChoice, 0, 1));

            // No Command, Show flying path
            Choices AircraftGetStatusCommandChoice = new Choices(new string[] { "Ident", "What's your status" });
            GrammarBuilder AircraftGetStatus = new GrammarBuilder();
            AircraftGetStatus.Append(AircraftElement);
            AircraftGetStatus.Append(new GrammarBuilder(AircraftGetStatusCommandChoice));
            AircraftGetStatus.Culture = new System.Globalization.CultureInfo("en-US");
            Grammar AircraftGetStatusGrammar = new Grammar(AircraftGetStatus);
            AircraftGetStatusGrammar.Name = "AircraftGetStatus";
            grammars.Add(AircraftGetStatusGrammar);

            // Aircraft Takeoff
            Choices AircraftTakeOffCommandChoice = new Choices(new string[] { "Cleard to takeoff" });
            GrammarBuilder AircraftTakeOff = new GrammarBuilder();
            AircraftTakeOff.Append(AircraftElement);
            AircraftTakeOff.Append(new GrammarBuilder(AircraftTakeOffCommandChoice));
            AircraftTakeOff.Append(RunwayElement);
            AircraftTakeOff.Culture = new System.Globalization.CultureInfo("en-US");
            Grammar AircraftTakeOffGrammar = new Grammar(AircraftTakeOff);
            AircraftTakeOffGrammar.Name = "AircraftTakeOff";
            grammars.Add(AircraftTakeOffGrammar);

            // Aircraft Land
            Choices AircraftLandCommandChoice = new Choices(new string[] { "Cleard to land", "Cleard to approach" });
            GrammarBuilder AircraftLand = new GrammarBuilder();
            AircraftLand.Append(AircraftElement);
            AircraftLand.Append(new GrammarBuilder(AircraftLandCommandChoice));
            AircraftLand.Append(RunwayElement);
            AircraftLand.Culture = new System.Globalization.CultureInfo("en-US");
            Grammar AircraftLandGrammar = new Grammar(AircraftLand);
            AircraftLandGrammar.Name = "AircraftLand";
            grammars.Add(AircraftLandGrammar);

            // Aircraft Vector to Named Waypoint
            Choices AircraftVectorToNamedWaypointCommandChoice = new Choices(new string[] { "Vector to", "Heading to", "Direct to" });
            GrammarBuilder AircraftVectorToNamedWaypoint = new GrammarBuilder();
            AircraftVectorToNamedWaypoint.Append(AircraftElement);
            AircraftVectorToNamedWaypoint.Append(new GrammarBuilder(AircraftVectorToNamedWaypointCommandChoice));
            AircraftVectorToNamedWaypoint.Append(NamedWaypointElement);
            AircraftVectorToNamedWaypoint.Culture = new System.Globalization.CultureInfo("en-US");
            Grammar AircraftVectorToNamedWaypointGrammar = new Grammar(AircraftVectorToNamedWaypoint);
            AircraftVectorToNamedWaypointGrammar.Name = "AircraftVectorToNamedWaypoint";
            grammars.Add(AircraftVectorToNamedWaypointGrammar);

            return grammars.ToArray();
        }
    }
}
