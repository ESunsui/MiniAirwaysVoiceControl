namespace MiniAirwaysVoiceControl
{
    public class MiniAirwaysVoiceControlInterface
    {
        public interface IMiniAirwaysVoiceControlInterface
        {

        }
        public class SREngineRunningState : IMiniAirwaysVoiceControlInterface
        {
            public bool IsRunning { get; set; }
        }

        public class SREngineLanguage : IMiniAirwaysVoiceControlInterface
        {
            public string Language { get; set; }
        }

        public class GrammarSource : IMiniAirwaysVoiceControlInterface
        {
            public string[] Airlines { get; set; }
            public string[] NamedWaypoints { get; set; }
        }

        public class GrammarStruct : IMiniAirwaysVoiceControlInterface
        {
            public string[] AircraftStatRules { get; set; }
            public string[] AircraftTakeoffRules { get; set; }
            public string[] AircraftLandingRules { get; set; }
            public string[] AircraftFlyHeadingRules { get; set; }
            public string[] AircraftVectorToWaypointRules { get; set; }
        }

        public class LanguageInitResult : IMiniAirwaysVoiceControlInterface
        {
            public string Language { get; set; }
            public bool IsSuccess { get; set; }
            public bool SRPackageNotInstalled { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class InputDeviceConnectResult : IMiniAirwaysVoiceControlInterface
        {
            public bool IsConnected { get; set; }
            public string Message { get; set; }
        }

        public class SRResult : IMiniAirwaysVoiceControlInterface
        { 
            public enum ResultType
            {
                Hypothesized,
                Rejected,
                Recognized
            }

            public ResultType Type { get; set; }
            public string Message { get; set; }
        }
    }
}
