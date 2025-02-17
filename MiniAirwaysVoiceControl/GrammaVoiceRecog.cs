using System;
using System.Globalization;
using System.Speech.Recognition;
using NAudio.CoreAudioApi;

namespace MiniAirwaysVoiceControl
{
    internal class GrammaVoiceRecog
    {
        SpeechRecognitionEngine recognizer;

        public event EventHandler<string> OnSRPackageNotInstalled;
        public event EventHandler<string> OnSRInitFailed;

        public event EventHandler<string> OnInputDeviceSet;
        public event EventHandler<string> OnInputDeviceSetFailed;

        public event EventHandler<bool> OnSpeechRecogRunningStateChanged;

        public event EventHandler<string> OnSpeechHypothesized;
        public event EventHandler<string> OnSpeechRejected;
        public event EventHandler<(string, string)> OnSpeechRecognized;


        public void Init(string language)
        {
            try
            {
                recognizer = new SpeechRecognitionEngine(new CultureInfo(language));
            }
            catch (System.ArgumentException e)
            {
                if (e.ParamName == "culture")
                {
                    Console.WriteLine("Invalid language or corresponding language speech recognition package is not installed: " + language);
                    OnSRPackageNotInstalled?.Invoke(this, language);
                    return;
                }
                else
                {
                    Console.WriteLine("Error initializing speech recognition engine: " + e.Message);
                    OnSRInitFailed?.Invoke(this, e.Message);
                    return;
                }
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
        }

        public void SetGrammar(Grammar[]? grammar = null)
        {
            recognizer.UnloadAllGrammars();
            if (grammar == null)
                recognizer.LoadGrammar(new DictationGrammar());
            else
            {
                foreach (Grammar g in grammar)
                {
                    recognizer.LoadGrammar(g);
                }
            }
        }

        public void ClearGrammar()
        {
            recognizer.UnloadAllGrammars();
        }

        public bool SetDefaultInput()
        {
            try
            {
                recognizer.SetInputToDefaultAudioDevice();
                using (var enumerator = new MMDeviceEnumerator())
                {
                    var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);

                    Console.WriteLine("Default Audio Input Device: " + defaultDevice.FriendlyName);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error setting input: " + e.Message);
                return false;
            }
        }

        public void Start()
        {
            // Start asynchronous, continuous speech recognition.  
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            OnSpeechRecogRunningStateChanged?.Invoke(this, true);
        }

        public void Stop()
        {
            recognizer.RecognizeAsyncCancel();
            recognizer.RecognizeAsyncStop();
            OnSpeechRecogRunningStateChanged?.Invoke(this, false);
        }

        public void SimulateAsync(string text)
        {
            recognizer.EmulateRecognizeAsync(text);
        }


        // Handle the SpeechRecognized event.  

        void SpeechDetectedHandler(object sender, SpeechDetectedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[Detected]");
            Console.ResetColor();
            Console.WriteLine("\tT: {0}", e.AudioPosition);
        }

        // Handle the SpeechHypothesized event.  
        void SpeechHypothesizedHandler(
          object sender, SpeechHypothesizedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Hypothesized]");
            Console.ResetColor();

            string grammarName = "N/A";
            string resultText = "N/A";
            string resultConfidence = "N/A";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
                resultConfidence = e.Result.Confidence.ToString();
            }

            Console.WriteLine("\tG: {0}, R: {1}, C: {2}",
              grammarName, resultText, resultConfidence);
            OnSpeechHypothesized?.Invoke(this, resultText);
        }

        // Handle the SpeechRecognitionRejected event.  
        void SpeechRecognitionRejectedHandler(
          object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Rejected]");
            Console.ResetColor();

            string grammarName = "N/A";
            string resultText = "N/A";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
            }

            Console.WriteLine("\tG: {0}, R: {1}",
              grammarName, resultText);
            OnSpeechRejected?.Invoke(this, null);
        }

        // Handle the SpeechRecognized event.  
        void SpeechRecognizedHandler(
          object sender, SpeechRecognizedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Accepted]");
            Console.ResetColor();

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

            Console.WriteLine("\tG: {0}, R: {1}",
              grammarName, resultText);
            OnSpeechRecognized?.Invoke(this, (grammarName, resultText));
        }

        // Handle the RecognizeCompleted event.  
        void RecognizeCompletedHandler(
          object sender, RecognizeCompletedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[Completed]");
            Console.ResetColor();

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