using Newtonsoft.Json;
using static MiniAirwaysVoiceControl.GrammaVoiceRecog;

namespace MiniAirwaysVoiceControl
{
    public class MiniAirwaysVoiceControlInterface
    {
        public interface IMiniAirwaysSrSeralizeable
        {
            public Message Seralize()
            {
                return new Message()
                {
                    type = this.GetType().Name,
                    content = JsonConvert.SerializeObject(this)
                };
            }
        }
        public class SREngineRunningState : IMiniAirwaysSrSeralizeable
        {
            public bool IsRunning { get; set; }
        }

        public class SREngineLanguage : IMiniAirwaysSrSeralizeable
        {
            public string Language { get; set; }
        }

        public class GrammarSource : IMiniAirwaysSrSeralizeable
        {
            public string[] Airlines { get; set; }
            public string[] AirlineCodes { get; set; }
            public string[] NamedWaypoints { get; set; }
        }

        public class GrammarStruct : IMiniAirwaysSrSeralizeable
        {
            public string[] AircraftStatRules { get; set; }
            public string[] AircraftTakeoffRules { get; set; }
            public string[] AircraftLandingRules { get; set; }
            public string[] AircraftFlyHeadingRules { get; set; }
            public string[] AircraftVectorToDestinationWaypointRules { get; set; }
            public string[] AircraftVectorToNormalWaypointRules { get; set; }
        }

        public class LanguageInitResult : IMiniAirwaysSrSeralizeable
        {
            public string Language { get; set; }
            public bool IsSuccess { get; set; }
            public bool SRPackageNotInstalled { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class InputDeviceConnectResult : IMiniAirwaysSrSeralizeable
        {
            public bool IsConnected { get; set; }
            public string Message { get; set; }
        }

        public class GrammarInitResult : IMiniAirwaysSrSeralizeable
        {
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class SRResult : IMiniAirwaysSrSeralizeable
        { 
            public ResultType Type { get; set; }
            public GrammarType Grammar { get; set; }
            public string Message { get; set; }
            public string Aircraft { get; set; }
            public string Waypoint { get; set; }
            public string Heading { get; set; }
            public string Runway { get; set; }
        }
    }
}
