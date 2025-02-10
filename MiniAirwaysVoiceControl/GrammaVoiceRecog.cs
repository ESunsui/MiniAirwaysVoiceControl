using System;
using System.Globalization;
using System.Speech.Recognition;

namespace SpeechRecognitionApp
{
    class GrammaVoiceRecog
    {
        SpeechRecognitionEngine recognizer;
        public delegate void OnSpeechRecognizedHandler(string Text);
        public event OnSpeechRecognizedHandler OnSpeechRecognized;

        public delegate void OnEngineStateChangeHandler(bool Success);
        public event OnEngineStateChangeHandler OnEngineStateChanged;

        public void Init()
        {
            recognizer =
              new SpeechRecognitionEngine(
                new System.Globalization.CultureInfo("en-US")
                );

            // Create and load a dictation grammar.  
            recognizer.LoadGrammar(new DictationGrammar());

            // Add a handler for the speech recognized event.  
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            recognizer.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);

            // Configure input to the speech recognizer.  
            recognizer.SetInputToDefaultAudioDevice();
        }


        public void Init(string language, Grammar grammar)
        {
            recognizer = new SpeechRecognitionEngine(new CultureInfo(language));
            if (grammar == null)
            {
                recognizer.LoadGrammar(new DictationGrammar());
            }
            else
            {
                recognizer.LoadGrammar(grammar);
            }

            // Add a handler for the speech recognized event.  
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            recognizer.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);

            // Configure input to the speech recognizer.  
            recognizer.SetInputToDefaultAudioDevice();
        }

        public void Start()
        {
            // Start asynchronous, continuous speech recognition.  
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            recognizer.RecognizeAsyncStop();
        }


        // Handle the SpeechRecognized event.  
        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            OnSpeechRecognized?.Invoke(e.Result.Text);
        }
        void recognizer_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            OnEngineStateChanged?.Invoke(e?.Error == null);
        }
    }
}