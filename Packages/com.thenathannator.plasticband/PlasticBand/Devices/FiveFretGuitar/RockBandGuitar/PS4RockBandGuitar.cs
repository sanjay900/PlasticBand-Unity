using System.Collections.Generic;
using System.Runtime.InteropServices;
using PlasticBand.Devices.LowLevel;
using PlasticBand.LowLevel;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

// PlasticBand reference doc:
// https://github.com/TheNathannator/PlasticBand/blob/main/Docs/Instruments/5-Fret%20Guitar/Rock%20Band/PS4.md

namespace PlasticBand.Devices.LowLevel
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PS4RockBandGuitarState_NoReportId : IInputStateTypeInfo
    {
        public FourCC format => HidDefinitions.InputFormat;

        private fixed byte unused1[4];

        [InputControl(name = "dpad", layout = "Dpad", format = "BIT", sizeInBits = 4, defaultState = 8)]
        [InputControl(name = "dpad/up", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7")]
        [InputControl(name = "dpad/right", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=1,maxValue=3")]
        [InputControl(name = "dpad/down", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=3,maxValue=5")]
        [InputControl(name = "dpad/left", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=5, maxValue=7")]

        [InputControl(name = "strumUp", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, defaultState = 8, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7")]
        [InputControl(name = "strumDown", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, defaultState = 8, parameters = "minValue=3,maxValue=5,nullValue=8")]

        [InputControl(name = "blueFret", layout = "Button", bit = 4)]
        [InputControl(name = "greenFret", layout = "Button", bit = 5)]
        [InputControl(name = "redFret", layout = "Button", bit = 6)]
        [InputControl(name = "yellowFret", layout = "Button", bit = 7)]

        [InputControl(name = "orangeFret", layout = "Button", bit = 8)]

        [InputControl(name = "selectButton", layout = "Button", bit = 12)]
        [InputControl(name = "startButton", layout = "Button", bit = 13)]

        [InputControl(name = "soloGreen", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x4020")]
        [InputControl(name = "soloRed", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x4040")]
        [InputControl(name = "soloYellow", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x4080")]
        [InputControl(name = "soloBlue", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x4010")]
        [InputControl(name = "soloOrange", layout = "MaskButton", format = "USHT", bit = 0, parameters = "mask=0x4100")]
        public ushort buttons1;

        [InputControl(name = "psButton", layout = "Button", bit = 0, displayName = "PlayStation")]
        public byte buttons2;

        // TODO: Normalization
        [InputControl(name = "whammy", layout = "Axis")]
        public byte whammy;

        // TODO: Normalization
        [InputControl(name = "tilt", layout = "Axis")]
        public byte tilt;

        // TODO: The position of this is assumed because RockBandGuitar requires it, needs verification
        // TODO: Define specific ranges for each of the notches if necessary
        [InputControl(name = "pickupSwitch", layout = "Axis")]
        public byte pickupSwitch;

        private fixed byte unused2[68];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PS4RockBandGuitarState_ReportId : IInputStateTypeInfo
    {
        public FourCC format => HidDefinitions.InputFormat;

        public byte reportId;
        public PS4RockBandGuitarState_NoReportId state;
    }

    [InputControlLayout(stateType = typeof(PS4RockBandGuitarState_NoReportId), hideInUI = true)]
    internal class PS4RockBandGuitar_NoReportId : PS4RockBandGuitar { }

    [InputControlLayout(stateType = typeof(PS4RockBandGuitarState_ReportId), hideInUI = true)]
    internal class PS4RockBandGuitar_ReportId : PS4RockBandGuitar { }
}

namespace PlasticBand.Devices
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || ((UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && HIDROGEN_FORCE_REPORT_IDS)
    using DefaultState = PS4RockBandGuitarState_ReportId;
#else
    using DefaultState = PS4RockBandGuitarState_NoReportId;
#endif

    /// <summary>
    /// A PS4 Rock Band guitar.
    /// </summary>
    [InputControlLayout(stateType = typeof(DefaultState), displayName = "PlayStation 4 Rock Band Guitar")]
    public class PS4RockBandGuitar : RockBandGuitar
    {
        /// <summary>
        /// The current <see cref="PS4RockBandGuitar"/>.
        /// </summary>
        public static new PS4RockBandGuitar current { get; private set; }

        /// <summary>
        /// A collection of all <see cref="PS4RockBandGuitar"/>s currently connected to the system.
        /// </summary>
        public new static IReadOnlyList<PS4RockBandGuitar> all => s_AllDevices;
        private static readonly List<PS4RockBandGuitar> s_AllDevices = new List<PS4RockBandGuitar>();

        internal new static void Initialize()
        {
            // Stratocaster
            HidReportIdLayoutFinder.RegisterLayout<PS4RockBandGuitar,
                PS4RockBandGuitar_ReportId, PS4RockBandGuitar_NoReportId>(0x0738, 0x8261);

            // Jaguar
            HidReportIdLayoutFinder.RegisterLayout<PS4RockBandGuitar,
                PS4RockBandGuitar_ReportId, PS4RockBandGuitar_NoReportId>(0x0E6F, 0x0173);
        }

        /// <summary>
        /// Sets this device as the current <see cref="PS4RockBandGuitar"/>.
        /// </summary>
        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            s_AllDevices.Add(this);
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            s_AllDevices.Remove(this);
            if (current == this)
                current = null;
        }
    }
}
