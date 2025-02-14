using System.Diagnostics;
using System.Speech.Recognition;
using SpeechRecognitionApp;
using Windows.Devices.Sensors;
using static MiniAirwaysVoiceControl.MiniAirwaysVoiceControlInterface;

namespace MiniAirwaysVoiceControl
{
    public class Program
    {
        static GrammaVoiceRecog VoiceRecog;
        static AircraftVoiceControlGrammarBuilder GrammarBuilder;
        static NamedPipeClient PipeClient;
        static AircraftVoiceController VoiceControl;

        static void Main(string[] args)
        {
            VoiceRecog = new GrammaVoiceRecog();
            GrammarBuilder = new AircraftVoiceControlGrammarBuilder();
            PipeClient = new NamedPipeClient();
            VoiceControl = new AircraftVoiceController();

            Init();

            // Keep the console window open.  
            while (true)
            {
                Console.ReadLine();
            }
        }

        

        static async Task Init()
        {
            await PipeClient.Connect();
            VoiceControl.Attach(PipeClient);

            GrammarBuilder.Init();

            VoiceControl.OnVoiceEngineRunningStateChanged += (object _, bool IsRunning) =>
            {
                if (IsRunning)
                {
                    VoiceRecog.Start();
                }
                else
                {
                    VoiceRecog.Stop();
                }
            };

            VoiceControl.OnVoiceEngineLanguageChanged += (object _, string Language) =>
            {
                VoiceRecog.Init(Language);
            };

            VoiceControl.OnGrammarStructChanged += (object _, GrammarStruct Grammar) =>
            {
                GrammarBuilder.SetRules(Grammar);
            };

            VoiceControl.OnGrammarSourceChanged += (object sender, (string[] Airlines, string[] NamedWaypoints) GrammarSource) =>
            {
                Grammar[] grammars = GrammarBuilder.CreateGrammar(GrammarSource.Airlines, GrammarSource.NamedWaypoints);
                VoiceRecog.SetGrammar(grammars);
            };


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