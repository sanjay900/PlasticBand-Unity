using System.Collections.Generic;
using System.Runtime.InteropServices;
using PlasticBand.Devices.LowLevel;
using PlasticBand.LowLevel;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

// PlasticBand reference doc:
// https://github.com/TheNathannator/PlasticBand/blob/main/Docs/Instruments/5-Fret%20Guitar/Rock%20Band/PS3%20and%20Wii.md

namespace PlasticBand.Devices.LowLevel
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PS3WiiRockBandGuitarState_NoReportId : IInputStateTypeInfo
    {
        public FourCC format => HidDefinitions.InputFormat;

        [InputControl(name = "blueFret", layout = "Button", bit = 0)]
        [InputControl(name = "greenFret", layout = "Button", bit = 1)]
        [InputControl(name = "redFret", layout = "Button", bit = 2)]
        [InputControl(name = "yellowFret", layout = "Button", bit = 3)]

        [InputControl(name = "orangeFret", layout = "Button", bit = 4)]
        [InputControl(name = "tilt", layout = "Button", bit = 5)]

        [InputControl(name = "selectButton", layout = "Button", bit = 8)]
        [InputControl(name = "startButton", layout = "Button", bit = 9)]

        [InputControl(name = "psButton", layout = "Button", bit = 12, displayName = "PlayStation")]

        [InputControl(name = "soloGreen", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x0042")]
        [InputControl(name = "soloRed", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x0044")]
        [InputControl(name = "soloYellow", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x0048")]
        [InputControl(name = "soloBlue", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x0041")]
        [InputControl(name = "soloOrange", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x0050")]
        public ushort buttons;

        [InputControl(name = "dpad", layout = "Dpad", format = "BIT", sizeInBits = 4, defaultState = 8)]
        [InputControl(name = "dpad/up", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7")]
        [InputControl(name = "dpad/right", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=1,maxValue=3")]
        [InputControl(name = "dpad/down", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=3,maxValue=5")]
        [InputControl(name = "dpad/left", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=5, maxValue=7")]

        [InputControl(name = "strumUp", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, defaultState = 8, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7")]
        [InputControl(name = "strumDown", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, defaultState = 8, parameters = "minValue=3,maxValue=5,nullValue=8")]
        public byte dpad;

        public fixed byte unused1[2];

        [InputControl(name = "whammy", layout = "Axis")]
        public byte whammy;

        // TODO: Define specific ranges for each of the notches
        [InputControl(name = "pickupSwitch", layout = "Axis")]
        public byte pickupSwitch;

        public fixed byte unused2[21];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PS3WiiRockBandGuitarState_ReportId : IInputStateTypeInfo
    {
        public FourCC format => HidDefinitions.InputFormat;

        public byte reportId;
        public PS3WiiRockBandGuitarState_NoReportId state;
    }

    [InputControlLayout(stateType = typeof(PS3WiiRockBandGuitarState_NoReportId), hideInUI = true)]
    internal class PS3RockBandGuitar_NoReportId : PS3RockBandGuitar { }

    [InputControlLayout(stateType = typeof(PS3WiiRockBandGuitarState_ReportId), hideInUI = true)]
    internal class PS3RockBandGuitar_ReportId : PS3RockBandGuitar { }

    [InputControlLayout(stateType = typeof(PS3WiiRockBandGuitarState_NoReportId), hideInUI = true)]
    internal class WiiRockBandGuitar_NoReportId : WiiRockBandGuitar { }

    [InputControlLayout(stateType = typeof(PS3WiiRockBandGuitarState_ReportId), hideInUI = true)]
    internal class WiiRockBandGuitar_ReportId : WiiRockBandGuitar { }
}

namespace PlasticBand.Devices
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || ((UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && HIDROGEN_FORCE_REPORT_IDS)
    using DefaultState = PS3WiiRockBandGuitarState_ReportId;
#else
    using DefaultState = PS3WiiRockBandGuitarState_NoReportId;
#endif

    [InputControlLayout(stateType = typeof(DefaultState), displayName = "PlayStation 3 Rock Band Guitar")]
    internal class PS3RockBandGuitar : RockBandGuitar
    {
        internal new static void Initialize()
        {
            HidReportIdLayoutFinder.RegisterLayout<PS3RockBandGuitar,
                PS3RockBandGuitar_ReportId, PS3RockBandGuitar_NoReportId>(0x12BA, 0x0200);
        }
    }

    [InputControlLayout(stateType = typeof(DefaultState), displayName = "Wii Rock Band Guitar")]
    internal class WiiRockBandGuitar : RockBandGuitar
    {
        internal new static void Initialize()
        {
            // RB1 guitars
            HidReportIdLayoutFinder.RegisterLayout<WiiRockBandGuitar,
                WiiRockBandGuitar_ReportId, WiiRockBandGuitar_NoReportId>(0x1BAD, 0x0004);

            // RB2 and later
            HidReportIdLayoutFinder.RegisterLayout<WiiRockBandGuitar,
                WiiRockBandGuitar_ReportId, WiiRockBandGuitar_NoReportId>(0x1BAD, 0x3010);
        }
    }
}
