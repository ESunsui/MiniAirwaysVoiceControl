using System.Diagnostics;
using System.Speech.Recognition;
using SpeechRecognitionApp;
using Windows.Devices.Sensors;
using static MiniAirwaysVoiceControl.GrammaVoiceRecog;
using static MiniAirwaysVoiceControl.MiniAirwaysVoiceControlInterface;

namespace MiniAirwaysVoiceControl
{
    public class Program
    {
        static GrammaVoiceRecog VoiceRecog;
        static AircraftVoiceControlGrammarBuilder GrammarBuilder;
        static NamedPipeClient PipeClient;
        static AircraftVoiceController VoiceControl;

        static GrammarSource testGrammarSource;

        static void Main(string[] args)
        {
            VoiceRecog = new GrammaVoiceRecog();
            GrammarBuilder = new AircraftVoiceControlGrammarBuilder();
            PipeClient = new NamedPipeClient();
            VoiceControl = new AircraftVoiceController();

            testGrammarSource = new GrammarSource() 
            {
                Airlines = new string[] { "United", "Delta" },
                NamedWaypoints = new string[] { "Alpha", "Bravo", "Charlie", "Delta" }
            };

            Init();

            // Keep the console window open.  
            while (true)
            {
                string t = Console.ReadLine();
                if (t.Length > 0)
                {
                    VoiceRecog.SimulateAsync(t);
                }
            }
        }

        

        static async Task Init()
        {
            GrammarBuilder.Init();
            await PipeClient.Connect();
            VoiceControl.Attach(PipeClient);

            #region Voice Controller Handlers

            VoiceControl.OnConnectToInputDevice += (object? _, bool IsConnected) => 
            { 
                VoiceRecog.SetDefaultInput(); 
            };

            VoiceControl.OnVoiceEngineLanguageChanged += (object? _, string Language) => 
            { 
                VoiceRecog.Init(Language); 
            };

            VoiceControl.OnGrammarStructChanged += (object? _, GrammarStruct Grammar) => 
            { 
                GrammarBuilder.SetRules(Grammar);
                try
                {
                    var grammars = GrammarBuilder.CreateGrammar(testGrammarSource.Airlines, testGrammarSource.NamedWaypoints);
                    VoiceRecog.SetGrammar(grammars);
                    VoiceControl.Send(new GrammarInitResult()
                    {
                        IsSuccess = true,
                        ErrorMessage = ""
                    });
                    VoiceRecog.ClearGrammar();
                }
                catch (Exception ex)
                {
                    VoiceControl.Send(new GrammarInitResult()
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    });
                }
                
            };

            VoiceControl.OnVoiceEngineRunningStateChanged += (object? _, bool IsRunning) => 
            { 
                VoiceRecog.SetRunningState(IsRunning); 
            };

            VoiceControl.OnGrammarSourceChanged += (object? _, GrammarSource grammarSource) => 
            {
                try
                {
                    var grammars = GrammarBuilder.CreateGrammar(grammarSource.Airlines, grammarSource.NamedWaypoints);
                    VoiceRecog.SetGrammar(grammars);
                    VoiceControl.Send(new GrammarInitResult()
                    {
                        IsSuccess = true,
                        ErrorMessage = ""
                    });
                }
                catch (Exception ex)
                {
                    VoiceControl.Send(new GrammarInitResult()
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    });
                }
            };

            #endregion

            #region Speech Recog Handlers

            VoiceRecog.OnInputDeviceSet += (object? _, string deviceName) =>
            {
                VoiceControl.Send(new InputDeviceConnectResult() 
                {
                    IsConnected = true,
                    Message = deviceName
                });
            };

            VoiceRecog.OnInputDeviceSetFailed += (object? _, string error) =>
            {
                VoiceControl.Send(new InputDeviceConnectResult()
                {
                    IsConnected = false,
                    Message = error
                });
            };

            VoiceRecog.OnSRInited += (object? _, string lang) =>
            {
                VoiceControl.Send(new LanguageInitResult()
                {
                    Language = lang,
                    IsSuccess = true,
                    SRPackageNotInstalled = false,
                    ErrorMessage = ""
                });
            };

            VoiceRecog.OnSRPackageNotInstalled += (object? _, string lang) =>
            {
                VoiceControl.Send(new LanguageInitResult()
                {
                    Language = lang,
                    IsSuccess = false,
                    SRPackageNotInstalled = true,
                    ErrorMessage = ""
                });
            };

            VoiceRecog.OnSRInitFailed += (object? _, string error) =>
            {
                VoiceControl.Send(new LanguageInitResult()
                {
                    Language = "",
                    IsSuccess = false,
                    SRPackageNotInstalled = false,
                    ErrorMessage = error
                });
            };

            VoiceRecog.OnSpeechRecogRunningStateChanged += (object? _, bool IsRunning) =>
            {
                VoiceControl.Send(new SREngineRunningState()
                {
                    IsRunning = IsRunning
                });
            };

            VoiceRecog.OnSpeechHypothesized += (object? _, string Hypothesis) =>
            {
                VoiceControl.Send(
                    GrammarBuilder.ExtractGrammar(ResultType.Hypothesized, "N/A", Hypothesis)
                );
            };

            VoiceRecog.OnSpeechRecognized += (object? _, (string Grammar, string Result) RecogResult) =>
            {
                VoiceControl.Send(
                    GrammarBuilder.ExtractGrammar(ResultType.Recognized, RecogResult.Grammar, RecogResult.Result)
                );
            };

            VoiceRecog.OnSpeechRejected += (object? _, string __) =>
            {
                VoiceControl.Send(
                    GrammarBuilder.ExtractGrammar(ResultType.Rejected, null, null)
                );
            };

            #endregion

        }
    }
}