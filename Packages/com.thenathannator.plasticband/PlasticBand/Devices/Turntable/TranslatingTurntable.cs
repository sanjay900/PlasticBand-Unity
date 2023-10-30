using System;
using System.Runtime.InteropServices;
using PlasticBand.LowLevel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace PlasticBand.Devices
{
    internal enum TranslatedTurntableButton
    {
        South = 0,
        East = 1,
        West = 2,
        North_Euphoria = 3,

        DpadUp = 4,
        DpadDown = 5,
        DpadLeft = 6,
        DpadRight = 7,

        Start = 8,
        Select = 9,
        System = 10,
    }

    [Flags]
    internal enum TranslatedTurntableButtonMask : ushort
    {
        None = 0,

        South = 1 << TranslatedTurntableButton.South,
        East = 1 << TranslatedTurntableButton.East,
        West = 1 << TranslatedTurntableButton.West,
        North_Euphoria = 1 << TranslatedTurntableButton.North_Euphoria,

        DpadUp = 1 << TranslatedTurntableButton.DpadUp,
        DpadDown = 1 << TranslatedTurntableButton.DpadDown,
        DpadLeft = 1 << TranslatedTurntableButton.DpadLeft,
        DpadRight = 1 << TranslatedTurntableButton.DpadRight,

        Start = 1 << TranslatedTurntableButton.Start,
        Select = 1 << TranslatedTurntableButton.Select,
        System = 1 << TranslatedTurntableButton.System,
    }

    internal interface ITurntableState : IInputStateTypeInfo
    {
        bool south { get; }
        bool east { get; }
        bool west { get; }
        bool north_euphoria { get; }

        bool dpadUp { get; }
        bool dpadDown { get; }
        bool dpadLeft { get; }
        bool dpadRight { get; }

        bool start { get; }
        bool select { get; }
        bool system { get; }

        bool leftGreen { get; }
        bool leftRed { get; }
        bool leftBlue { get; }

        bool rightGreen { get; }
        bool rightRed { get; }
        bool rightBlue { get; }

        sbyte leftVelocity { get; }
        sbyte rightVelocity { get; }

        sbyte effectsDial { get; }
        sbyte crossfader { get; }
    }

    /// <summary>
    /// The format which <see cref="TranslatingTurntable{TState}"/>s translate state into.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TranslatedTurntableState : IInputStateTypeInfo
    {
        public static FourCC Format => new FourCC('T', 'T', 'B', 'L');
        public FourCC format => Format;

        [InputControl(name = "buttonSouth", layout = "Button", bit = (int)TranslatedTurntableButton.South)]
        [InputControl(name = "buttonEast", layout = "Button", bit = (int)TranslatedTurntableButton.East)]
        [InputControl(name = "buttonWest", layout = "Button", bit = (int)TranslatedTurntableButton.West)]
        [InputControl(name = "buttonNorth", layout = "Button", bit = (int)TranslatedTurntableButton.North_Euphoria)]

        [InputControl(name = "dpad", layout = "Dpad", format = "BIT", bit = (int)TranslatedTurntableButton.DpadUp, sizeInBits = 4)]
        [InputControl(name = "dpad/up", bit = (int)TranslatedTurntableButton.DpadUp)]
        [InputControl(name = "dpad/down", bit = (int)TranslatedTurntableButton.DpadDown)]
        [InputControl(name = "dpad/left", bit = (int)TranslatedTurntableButton.DpadLeft)]
        [InputControl(name = "dpad/right", bit = (int)TranslatedTurntableButton.DpadRight)]

        [InputControl(name = "startButton", layout = "Button", bit = (int)TranslatedTurntableButton.Start)]
        [InputControl(name = "selectButton", layout = "Button", bit = (int)TranslatedTurntableButton.Select)]
        [InputControl(name = "systemButton", layout = "Button", bit = (int)TranslatedTurntableButton.System)]
        public ushort buttons;

        [InputControl(name = "leftTableGreen", layout = "Button", bit = 0)]
        [InputControl(name = "leftTableRed", layout = "Button", bit = 1)]
        [InputControl(name = "leftTableBlue", layout = "Button", bit = 2)]
        [InputControl(name = "rightTableGreen", layout = "Button", bit = 4)]
        [InputControl(name = "rightTableRed", layout = "Button", bit = 5)]
        [InputControl(name = "rightTableBlue", layout = "Button", bit = 6)]
        private byte m_TurntableButtons;

        public TurntableButton leftTableButtons
        {
            get => (TurntableButton)(m_TurntableButtons & 0x07);
            set => m_TurntableButtons = (byte)((m_TurntableButtons & ~0x07) | ((byte)value & 0x07));
        }

        public TurntableButton rightTableButtons
        {
            get => (TurntableButton)((m_TurntableButtons >> 4) & 0x07);
            set => m_TurntableButtons = (byte)((m_TurntableButtons & ~(0x07 << 4)) | (((byte)value & 0x07) << 4));
        }

        [InputControl(layout = "Axis", noisy = true)]
        public sbyte leftTableVelocity;

        [InputControl(layout = "Axis", noisy = true)]
        public sbyte rightTableVelocity;

        [InputControl(layout = "Axis", synthetic = true)]
        public sbyte effectsDial;

        [InputControl(layout = "Axis")]
        public sbyte crossfader;
    }

    /// <summary>
    /// A <see cref="Turntable"/> which translates its state data into a common
    /// <see cref="TranslatedTurntableState"/> format.
    /// </summary>
    internal abstract class TranslatingTurntable<TState> : Turntable, IInputStateCallbackReceiver
        where TState : unmanaged, ITurntableState, IInputStateTypeInfo
    {
        private TranslateStateHandler<TState, TranslatedTurntableState> m_Translator;

        private sbyte m_PreviousEffectsDial = 0;

        protected override void FinishSetup()
        {
            base.FinishSetup();
            m_Translator = TranslateState;
            StateTranslator<TState, TranslatedTurntableState>.VerifyDevice(this);
        }

        unsafe void IInputStateCallbackReceiver.OnNextUpdate()
        {
            // The effects dial delta must be reset at the beginning of an update
            using (var buffer = StateEvent.From(this, out var eventPtr))
            {
                ref var state = ref *(TranslatedTurntableState*)buffer.GetUnsafePtr();
                state.effectsDial = 0;
                InputState.Change(this, eventPtr);
            }
        }

        void IInputStateCallbackReceiver.OnStateEvent(InputEventPtr eventPtr)
            => StateTranslator<TState, TranslatedTurntableState>.UpdateState(this, eventPtr, m_Translator);
        bool IInputStateCallbackReceiver.GetStateOffsetForEvent(InputControl control, InputEventPtr eventPtr, ref uint offset)
            => StateTranslator<TState, TranslatedTurntableState>.GetStateOffsetForEvent(this, control, eventPtr, ref offset, m_Translator);

        private TranslatedTurntableState TranslateState(ref TState state)
        {
            var translated = new TranslatedTurntableState()
            {
                crossfader = state.crossfader,
            };

            // Effects dial delta
            // This calculation relies on overflow being defined behavior in C#/.NET
            sbyte effectsDialAbsolute = state.effectsDial;
            translated.effectsDial = (sbyte)(effectsDialAbsolute - m_PreviousEffectsDial);
            m_PreviousEffectsDial = effectsDialAbsolute;

            // Face buttons
            var buttons = TranslatedTurntableButtonMask.None;
            if (state.south) buttons |= TranslatedTurntableButtonMask.South; // A, cross
            if (state.east) buttons |= TranslatedTurntableButtonMask.East; // B, circle
            if (state.west) buttons |= TranslatedTurntableButtonMask.West; // X, square
            if (state.north_euphoria) buttons |= TranslatedTurntableButtonMask.North_Euphoria; // Y, triangle, euphoria

            if (state.dpadUp) buttons |= TranslatedTurntableButtonMask.DpadUp;
            if (state.dpadDown) buttons |= TranslatedTurntableButtonMask.DpadDown;
            if (state.dpadLeft) buttons |= TranslatedTurntableButtonMask.DpadLeft;
            if (state.dpadRight) buttons |= TranslatedTurntableButtonMask.DpadRight;

            if (state.start) buttons |= TranslatedTurntableButtonMask.Start;
            if (state.select) buttons |= TranslatedTurntableButtonMask.Select;
            if (state.system) buttons |= TranslatedTurntableButtonMask.System;

            translated.buttons = (ushort)buttons;

            var left = TurntableButton.None;
            var right = TurntableButton.None;

            if (state.leftGreen) left |= TurntableButton.Green;
            if (state.leftRed) left |= TurntableButton.Red;
            if (state.leftBlue) left |= TurntableButton.Blue;
    
            if (state.rightGreen) right |= TurntableButton.Green;
            if (state.rightRed) right |= TurntableButton.Red;
            if (state.rightBlue) right |= TurntableButton.Blue;

            translated.leftTableButtons = left;
            translated.rightTableButtons = right;

            translated.leftTableVelocity = state.leftVelocity;
            translated.rightTableVelocity = state.rightVelocity;

            return translated;
        }
    }
}