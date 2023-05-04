using System.Collections.Generic;
using System.Runtime.InteropServices;
using PlasticBand.Devices.LowLevel;
using PlasticBand.LowLevel;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

// PlasticBand reference doc:
// https://github.com/TheNathannator/PlasticBand/blob/main/Docs/Instruments/Pro%20Guitar/PS3%20and%20Wii.md

namespace PlasticBand.Devices.LowLevel
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PS3WiiProGuitarState_NoReportId : IInputStateTypeInfo
    {
        public FourCC format => HidDefinitions.InputFormat;

        [InputControl(name = "buttonWest", layout = "Button", bit = 0, displayName = "Square")]
        [InputControl(name = "buttonSouth", layout = "Button", bit = 1, displayName = "Cross")]
        [InputControl(name = "buttonEast", layout = "Button", bit = 2, displayName = "Circle")]
        [InputControl(name = "buttonNorth", layout = "Button", bit = 3, displayName = "Triangle")]

        [InputControl(name = "selectButton", layout = "Button", bit = 8)]
        [InputControl(name = "startButton", layout = "Button", bit = 9)]

        [InputControl(name = "psButton", layout = "Button", bit = 12, displayName = "PlayStation")]
        public ushort buttons;

        [InputControl(name = "dpad", layout = "Dpad", format = "BIT", sizeInBits = 4, defaultState = 8)]
        [InputControl(name = "dpad/up", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7")]
        [InputControl(name = "dpad/right", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=1,maxValue=3")]
        [InputControl(name = "dpad/down", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=3,maxValue=5")]
        [InputControl(name = "dpad/left", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=5, maxValue=7")]
        public byte dpad;

        private fixed byte unused1[2];

        [InputControl(name = "fret1", layout = "Integer", format = "BIT", bit = 0,  sizeInBits = 5)]
        [InputControl(name = "fret2", layout = "Integer", format = "BIT", bit = 5,  sizeInBits = 5)]
        [InputControl(name = "fret3", layout = "Integer", format = "BIT", bit = 10, sizeInBits = 5)]
        public ushort frets1;

        [InputControl(name = "fret4", layout = "Integer", format = "BIT", bit = 0,  sizeInBits = 5)]
        [InputControl(name = "fret5", layout = "Integer", format = "BIT", bit = 5,  sizeInBits = 5)]
        [InputControl(name = "fret6", layout = "Integer", format = "BIT", bit = 10, sizeInBits = 5)]
        [InputControl(name = "soloFlag", layout = "Button", bit = 15)] // TODO: Handle fret and solo flags directly like they are on standard RB guitars
        public ushort frets2;

        [InputControl(name = "velocity1", layout = "Axis", format = "BIT", bit = 0, sizeInBits = 7)]
        [InputControl(name = "greenFret", layout = "Button", bit = 7)]
        public byte velocity1;

        [InputControl(name = "velocity2", layout = "Axis", format = "BIT", bit = 0, sizeInBits = 7)]
        [InputControl(name = "redFret", layout = "Button", bit = 7)]
        public byte velocity2;

        [InputControl(name = "velocity3", layout = "Axis", format = "BIT", bit = 0, sizeInBits = 7)]
        [InputControl(name = "yellowFret", layout = "Button", bit = 7)]
        public byte velocity3;

        [InputControl(name = "velocity4", layout = "Axis", format = "BIT", bit = 0, sizeInBits = 7)]
        [InputControl(name = "blueFret", layout = "Button", bit = 7)]
        public byte velocity4;

        [InputControl(name = "velocity5", layout = "Axis", format = "BIT", bit = 0, sizeInBits = 7)]
        [InputControl(name = "orangeFret", layout = "Button", bit = 7)]
        public byte velocity5;

        [InputControl(name = "velocity6", layout = "Axis", format = "BIT", bit = 0, sizeInBits = 7)]
        public byte velocity6;

        // TODO: Auto-calibration sensor support
        public byte autoCal_Microphone; // NOTE: When the sensor isn't activated, this
        public byte autoCal_Light; // and this just duplicate the tilt axis

        // TODO: Needs verification
        [InputControl(name = "tilt", layout = "DiscreteButton", noisy = true, parameters = "minValue=0x40,maxValue=0x7F,nullValue=0x40")]
        public byte tilt;

        [InputControl(name = "spPedal", layout = "Button", bit = 7)]
        public byte pedal;

        private fixed byte unused3[8];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PS3WiiProGuitarState_ReportId : IInputStateTypeInfo
    {
        public FourCC format => HidDefinitions.InputFormat;

        public byte reportId;
        public PS3WiiProGuitarState_NoReportId state;
    }

    [InputControlLayout(stateType = typeof(PS3WiiProGuitarState_NoReportId), hideInUI = true)]
    internal class PS3ProGuitar_NoReportId : PS3ProGuitar { }

    [InputControlLayout(stateType = typeof(PS3WiiProGuitarState_ReportId), hideInUI = true)]
    internal class PS3ProGuitar_ReportId : PS3ProGuitar { }

    [InputControlLayout(stateType = typeof(PS3WiiProGuitarState_NoReportId), hideInUI = true)]
    internal class WiiProGuitar_NoReportId : WiiProGuitar { }

    [InputControlLayout(stateType = typeof(PS3WiiProGuitarState_ReportId), hideInUI = true)]
    internal class WiiProGuitar_ReportId : WiiProGuitar { }
}

namespace PlasticBand.Devices
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || ((UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && HIDROGEN_FORCE_REPORT_IDS)
    using DefaultState = PS3WiiProGuitarState_ReportId;
#else
    using DefaultState = PS3WiiProGuitarState_NoReportId;
#endif

    /// <summary>
    /// A PS3 Pro Guitar.
    /// </summary>
    [InputControlLayout(stateType = typeof(DefaultState), displayName = "PlayStation 3 Rock Band Pro Guitar")]
    public class PS3ProGuitar : ProGuitar
    {
        /// <summary>
        /// The current <see cref="PS3ProGuitar"/>.
        /// </summary>
        public static new PS3ProGuitar current { get; private set; }

        /// <summary>
        /// A collection of all <see cref="PS3ProGuitar"/>s currently connected to the system.
        /// </summary>
        public new static IReadOnlyList<PS3ProGuitar> all => s_AllDevices;
        private static readonly List<PS3ProGuitar> s_AllDevices = new List<PS3ProGuitar>();

        internal new static void Initialize()
        {
            // Mustang
            HidReportIdLayoutFinder.RegisterLayout<PS3ProGuitar,
                PS3ProGuitar_ReportId, PS3ProGuitar_NoReportId>(0x12BA, 0x2430);

            // MIDI Pro Adapter (Mustang)
            HidReportIdLayoutFinder.RegisterLayout<PS3ProGuitar,
                PS3ProGuitar_ReportId, PS3ProGuitar_NoReportId>(0x12BA, 0x2438);

            // Squire
            HidReportIdLayoutFinder.RegisterLayout<PS3ProGuitar,
                PS3ProGuitar_ReportId, PS3ProGuitar_NoReportId>(0x12BA, 0x2530);

            // MIDI Pro Adapter (Squire)
            HidReportIdLayoutFinder.RegisterLayout<PS3ProGuitar,
                PS3ProGuitar_ReportId, PS3ProGuitar_NoReportId>(0x12BA, 0x2538);
        }

        /// <summary>
        /// Sets this device as the current <see cref="PS3ProGuitar"/>.
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

    /// <summary>
    /// A Wii Pro Guitar.
    /// </summary>
    [InputControlLayout(stateType = typeof(DefaultState), displayName = "Wii Rock Band Pro Guitar")]
    public class WiiProGuitar : ProGuitar
    {
        /// <summary>
        /// The current <see cref="WiiProGuitar"/>.
        /// </summary>
        public static new WiiProGuitar current { get; private set; }

        /// <summary>
        /// A collection of all <see cref="WiiProGuitar"/>s currently connected to the system.
        /// </summary>
        public new static IReadOnlyList<WiiProGuitar> all => s_AllDevices;
        private static readonly List<WiiProGuitar> s_AllDevices = new List<WiiProGuitar>();

        internal new static void Initialize()
        {
            // Mustang
            HidReportIdLayoutFinder.RegisterLayout<WiiProGuitar,
                WiiProGuitar_ReportId, WiiProGuitar_NoReportId>(0x1BAD, 0x3430);

            // MIDI Pro Adapter (Mustang)
            HidReportIdLayoutFinder.RegisterLayout<WiiProGuitar,
                WiiProGuitar_ReportId, WiiProGuitar_NoReportId>(0x1BAD, 0x3438);

            // Squire
            HidReportIdLayoutFinder.RegisterLayout<WiiProGuitar,
                WiiProGuitar_ReportId, WiiProGuitar_NoReportId>(0x1BAD, 0x3530);

            // MIDI Pro Adapter (Squire)
            HidReportIdLayoutFinder.RegisterLayout<WiiProGuitar,
                WiiProGuitar_ReportId, WiiProGuitar_NoReportId>(0x1BAD, 0x3538);
        }

        /// <summary>
        /// Sets this device as the current <see cref="WiiProGuitar"/>.
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
