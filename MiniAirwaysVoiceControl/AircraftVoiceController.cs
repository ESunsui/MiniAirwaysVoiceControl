using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeechRecognitionApp;
using Newtonsoft.Json;
using static MiniAirwaysVoiceControl.MiniAirwaysVoiceControlInterface;

namespace MiniAirwaysVoiceControl
{
    public class Message()
    {
        public string type;
        public string content;
    }
    internal class AircraftVoiceController
    {
        public event EventHandler<bool> OnVoiceEngineRunningStateChanged;
        public event EventHandler<string> OnVoiceEngineLanguageChanged;
        public event EventHandler<(string[], string[])> OnGrammarSourceChanged;
        public event EventHandler<GrammarStruct> OnGrammarStructChanged;
        NamedPipeClient namedPipeClient;


        public void Attach(NamedPipeClient namedPipeClient)
        {
            this.namedPipeClient = namedPipeClient;
            namedPipeClient.OnMessageReceived += OnMessageReceived;
        }

        public void Detach()
        {
            this.namedPipeClient.OnMessageReceived -= OnMessageReceived;
            this.namedPipeClient = null;
        }

        private void OnMessageReceived(string message)
        {
            Message? msg = JsonConvert.DeserializeObject<Message>(message);
            if (msg != null)
            {
                // Handle the message
                switch (msg.type)
                {
                    case "SVRS":
                        VOEngineRunningState? voEngineRunningState = JsonConvert.DeserializeObject<VOEngineRunningState>(msg.content);
                        if (voEngineRunningState != null)
                        {
                            OnVoiceEngineRunningStateChanged?.Invoke(this, voEngineRunningState.IsRunning);
                        }
                        break;
                    case "SVLG":
                        VoiceEngineLanguage? voiceEngineLanguage = JsonConvert.DeserializeObject<VoiceEngineLanguage>(msg.content);
                        if (voiceEngineLanguage != null)
                        {
                            OnVoiceEngineLanguageChanged?.Invoke(this, voiceEngineLanguage.Language);
                        }
                        break;
                    case "SVGR":
                        GrammarSource? grammarSource = JsonConvert.DeserializeObject<GrammarSource>(msg.content);
                        if (grammarSource != null)
                        {
                            OnGrammarSourceChanged?.Invoke(this, (grammarSource.Airlines, grammarSource.NamedWaypoints));
                        }
                        break;
                    case "SVST":
                        GrammarStruct? grammarStruct = JsonConvert.DeserializeObject<GrammarStruct>(msg.content);
                        if (grammarStruct != null)
                        {
                            OnGrammarStructChanged?.Invoke(this, grammarStruct);
                        }
                        break;
                }
            }
        }

        


    }
}
