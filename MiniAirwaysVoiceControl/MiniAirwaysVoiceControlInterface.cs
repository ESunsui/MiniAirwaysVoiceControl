using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniAirwaysVoiceControl
{
    public class MiniAirwaysVoiceControlInterface
    {
        // SVRS
        public class VOEngineRunningState()
        {
            public bool IsRunning { get; set; }
        }

        // SVLG
        public class VoiceEngineLanguage()
        {
            public string Language { get; set; }
        }

        // SVGR
        public class GrammarSource()
        {
            public string[] Airlines { get; set; }
            public string[] NamedWaypoints { get; set; }
        }

        // SVST
        public class GrammarStruct()
        {
            public string[] AircraftStatRules { get; set; }
            public string[] AircraftTakeoffRules { get; set; }
            public string[] AircraftLandingRules { get; set; }
            public string[] AircraftFlyHeadingRules { get; set; }
            public string[] AircraftVectorToWaypointRules { get; set; }
        }
    }
}
