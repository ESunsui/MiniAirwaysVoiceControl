using System;
using System.Globalization;
using System.Speech.Recognition;
using static SpeechRecognitionApp.GrammaVoiceRecog;

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

            recognizer.SpeechDetected +=
            new EventHandler<SpeechDetectedEventArgs>(
                SpeechDetectedHandler);
            recognizer.SpeechHypothesized +=
              new EventHandler<SpeechHypothesizedEventArgs>(
                SpeechHypothesizedHandler);
            recognizer.SpeechRecognitionRejected +=
              new EventHandler<SpeechRecognitionRejectedEventArgs>(
                SpeechRecognitionRejectedHandler);
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(
                SpeechRecognizedHandler);
            recognizer.RecognizeCompleted +=
              new EventHandler<RecognizeCompletedEventArgs>(
                RecognizeCompletedHandler);

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
            recognizer.SpeechDetected +=
            new EventHandler<SpeechDetectedEventArgs>(
                SpeechDetectedHandler);
            recognizer.SpeechHypothesized +=
              new EventHandler<SpeechHypothesizedEventArgs>(
                SpeechHypothesizedHandler);
            recognizer.SpeechRecognitionRejected +=
              new EventHandler<SpeechRecognitionRejectedEventArgs>(
                SpeechRecognitionRejectedHandler);
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(
                SpeechRecognizedHandler);
            recognizer.RecognizeCompleted +=
              new EventHandler<RecognizeCompletedEventArgs>(
                RecognizeCompletedHandler);

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

        void recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            OnSpeechRecognized?.Invoke(e.Result.Text);
        }
        void recognizer_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            OnEngineStateChanged?.Invoke(e?.Error == null);
        }

        static void SpeechDetectedHandler(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine(" In SpeechDetectedHandler:");
            Console.WriteLine(" - AudioPosition = {0}", e.AudioPosition);
        }

        // Handle the SpeechHypothesized event.  
        static void SpeechHypothesizedHandler(
          object sender, SpeechHypothesizedEventArgs e)
        {
            Console.WriteLine(" In SpeechHypothesizedHandler:");

            string grammarName = "<not available>";
            string resultText = "<not available>";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
            }

            Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
              grammarName, resultText);
        }

        // Handle the SpeechRecognitionRejected event.  
        static void SpeechRecognitionRejectedHandler(
          object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine(" In SpeechRecognitionRejectedHandler:");

            string grammarName = "<not available>";
            string resultText = "<not available>";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
            }

            Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
              grammarName, resultText);
        }

        // Handle the SpeechRecognized event.  
        static void SpeechRecognizedHandler(
          object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(" In SpeechRecognizedHandler.");

            string grammarName = "<not available>";
            string resultText = "<not available>";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
            }

            Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
              grammarName, resultText);
        }

        // Handle the RecognizeCompleted event.  
        static void RecognizeCompletedHandler(
          object sender, RecognizeCompletedEventArgs e)
        {
            Console.WriteLine(" In RecognizeCompletedHandler.");

            if (e.Error != null)
            {
                Console.WriteLine(
                  " - Error occurred during recognition: {0}", e.Error);
                return;
            }
            if (e.InitialSilenceTimeout || e.BabbleTimeout)
            {
                Console.WriteLine(
                  " - BabbleTimeout = {0}; InitialSilenceTimeout = {1}",
                  e.BabbleTimeout, e.InitialSilenceTimeout);
                return;
            }
            if (e.InputStreamEnded)
            {
                Console.WriteLine(
                  " - AudioPosition = {0}; InputStreamEnded = {1}",
                  e.AudioPosition, e.InputStreamEnded);
            }
            if (e.Result != null)
            {
                Console.WriteLine(
                  " - Grammar = {0}; Text = {1}; Confidence = {2}",
                  e.Result.Grammar.Name, e.Result.Text, e.Result.Confidence);
                Console.WriteLine(" - AudioPosition = {0}", e.AudioPosition);
            }
            else
            {
                Console.WriteLine(" - No result.");
            }

        }

    }
}