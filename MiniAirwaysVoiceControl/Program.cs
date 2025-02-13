using System.Diagnostics;
using System.Speech.Recognition;
using SpeechRecognitionApp;
using Windows.Devices.Sensors;

namespace MiniAirwaysVoiceControl
{
    public class Program
    {
        static GrammaVoiceRecog VoiceRecog;
        static AircraftVoiceControlGrammarBuilder GrammarBuilder;
        static NamedPipeClient PipeClient;
        static void Main(string[] args)
        {
            VoiceRecog = new GrammaVoiceRecog();
            GrammarBuilder = new AircraftVoiceControlGrammarBuilder();
            PipeClient = new NamedPipeClient();
            Init();

            // Keep the console window open.  
            while (true)
            {
                Console.ReadLine();
            }
        }

        

        static async Task Init()
        {
            string[] airlines = new string[] { "Flyer", "Itarrow", "Lux Air", "K L M", "Swiss" };
            string[] namedWaypoints = new string[] { "Vaton", "Neska", "Kobbi", "Lindy", "Norry", "Saber" };

            await PipeClient.Connect();

            GrammarBuilder.Init();

            Grammar[] grammars = GrammarBuilder.CreateGrammar(airlines, namedWaypoints);

            VoiceRecog.Init("en-US");
            VoiceRecog.SetGrammar(grammars);

            bool InputSet = VoiceRecog.SetDefaultInput();

            //VoiceRecog.SimulateAsync("United Two one seven");

            if (!InputSet)
            {
                Console.WriteLine("Failed to set default input");
                return;
            }
            else
            {
                VoiceRecog.OnSpeechHypothesized += (string Text) =>
                {
                    PipeClient.Send(Text);
                };

                VoiceRecog.Start();
            }
        }
    }
}