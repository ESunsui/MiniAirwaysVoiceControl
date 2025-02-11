using System.Diagnostics;
using System.Speech.Recognition;
using SpeechRecognitionApp;

namespace MiniAirwaysVoiceControl
{
    public class Program
    {
        static GrammaVoiceRecog VoiceRecog;
        static void Main(string[] args)
        {
            Start();

            // Keep the console window open.  
            while (true)
            {
                Console.ReadLine();
            }
        }

        private static Grammar CreateGrammar()
        {
            Choices AirlineChoice = new Choices(new string[] { "Delta", "America", "JetBlue", "United", "Southwest", "Alaska" });
            GrammarBuilder AirlineElement = new GrammarBuilder(AirlineChoice);
            Choices FlightNumberChoices = new Choices(new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero"});
            GrammarBuilder FlightNumberElement = new GrammarBuilder(FlightNumberChoices, 2, 4);
            GrammarBuilder AircraftElement = new GrammarBuilder();
            AircraftElement.Append(AirlineElement);
            AircraftElement.Append(new GrammarBuilder("Airline", 0, 1));
            AircraftElement.Append(new GrammarBuilder("Flight", 0, 1));
            AircraftElement.Append(FlightNumberElement);
            AircraftElement.Culture = new System.Globalization.CultureInfo("en-US");

            return new Grammar(AircraftElement);

        }

        static void Start()
        {
            VoiceRecog = new GrammaVoiceRecog();
            Grammar AircraftGrammar = CreateGrammar();
            AircraftGrammar.Name = "Aircraft";
            VoiceRecog.Init("en-US", AircraftGrammar);

            bool InputSet = VoiceRecog.SetDefaultInput();

            //VoiceRecog.SimulateAsync("United Two one seven");

            if (!InputSet)
            {
                Console.WriteLine("Failed to set default input");
                return;
            }
            else
            {
                VoiceRecog.Start();
            }
        }
    }
}