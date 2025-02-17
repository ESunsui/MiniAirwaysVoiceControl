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
        public event EventHandler<bool> OnConnectToInputDevice;
        NamedPipeClient namedPipeClient;
        List<Message> messages = new();
        CancellationTokenSource cts;


        public void Attach(NamedPipeClient namedPipeClient)
        {
            this.namedPipeClient = namedPipeClient;
            namedPipeClient.OnMessageReceived += OnMessageReceived;
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = new CancellationTokenSource();
            SendLoop(cts.Token);
        }

        public void Detach()
        {
            this.namedPipeClient.OnMessageReceived -= OnMessageReceived;
            this.namedPipeClient = null;
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        private void OnMessageReceived(string message)
        {
            Message? msg = JsonConvert.DeserializeObject<Message>(message);
            if (msg != null)
            {
                // Handle the message
                switch (msg.type)
                {
                    case nameof(InputDeviceConnectResult):
                        InputDeviceConnectResult? inputDeviceConnectResult = JsonConvert.DeserializeObject<InputDeviceConnectResult>(msg.content);
                        if (inputDeviceConnectResult != null)
                        {
                            OnConnectToInputDevice?.Invoke(this, inputDeviceConnectResult.IsConnected);
                        }
                        break;
                    case nameof(SREngineRunningState):
                        SREngineRunningState? voEngineRunningState = JsonConvert.DeserializeObject<SREngineRunningState>(msg.content);
                        if (voEngineRunningState != null)
                        {
                            OnVoiceEngineRunningStateChanged?.Invoke(this, voEngineRunningState.IsRunning);
                        }
                        break;
                    case nameof(SREngineLanguage):
                        SREngineLanguage? voiceEngineLanguage = JsonConvert.DeserializeObject<SREngineLanguage>(msg.content);
                        if (voiceEngineLanguage != null)
                        {
                            OnVoiceEngineLanguageChanged?.Invoke(this, voiceEngineLanguage.Language);
                        }
                        break;
                    case nameof(GrammarSource):
                        GrammarSource? grammarSource = JsonConvert.DeserializeObject<GrammarSource>(msg.content);
                        if (grammarSource != null)
                        {
                            OnGrammarSourceChanged?.Invoke(this, (grammarSource.Airlines, grammarSource.NamedWaypoints));
                        }
                        break;
                    case nameof(GrammarStruct):
                        GrammarStruct? grammarStruct = JsonConvert.DeserializeObject<GrammarStruct>(msg.content);
                        if (grammarStruct != null)
                        {
                            OnGrammarStructChanged?.Invoke(this, grammarStruct);
                        }
                        break;
                }
            }
        }

        private void SendLoop(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    if (namedPipeClient.IsConnected && messages.Count > 0)
                    {

                        Message message = messages[0];
                        messages.RemoveAt(0);
                        await namedPipeClient.Send(JsonConvert.SerializeObject(message));
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            });
        }

        public void Send(IMiniAirwaysVoiceControlInterface msg)
        {
            messages.Add(new Message() { type = msg.GetType().Name, content = JsonConvert.SerializeObject(msg, type:msg.GetType(), null) });
        }
    }
}
