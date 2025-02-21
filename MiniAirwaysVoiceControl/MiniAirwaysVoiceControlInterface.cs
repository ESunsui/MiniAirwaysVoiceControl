using Newtonsoft.Json;
using static MiniAirwaysVoiceControl.GrammaVoiceRecog;

namespace MiniAirwaysVoiceControl
{
    public class MiniAirwaysVoiceControlInterface
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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
            public string[] RunwayNames { get; set; }
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
