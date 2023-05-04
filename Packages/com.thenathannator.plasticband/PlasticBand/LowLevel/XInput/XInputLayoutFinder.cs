using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

namespace PlasticBand.LowLevel
{
    using static XInputController;

    /// <summary>
    /// Registers layouts for XInput devices, and performs fixups for devices that require state information to determine the true type.
    /// </summary>
    internal static class XInputLayoutFinder
    {
        /// <summary>
        /// The interface name used for XInput devices.
        /// </summary>
        public const string InterfaceName = "XInput";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        /// <summary>
        /// An XInput state packet.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct XInputState
        {
            public uint packetCount;
            public XInputGamepad gamepad;
        }

        /// <summary>
        /// Gets the current XInput state for the given user index.
        /// </summary>
        // 9_1_0 is used for best compatibility across all versions of Windows
        // Nothing important in the new versions that requires their use
        [DllImport("xinput9_1_0.dll")]
        private static extern int XInputGetState(
            int UserIndex, // DWORD
            out XInputState State // XINPUT_STATE*
        );
#endif

        /// <summary>
        /// Resolves the layout of an XInput device.
        /// </summary>
        private struct XInputLayoutOverride
        {
            public int subType;
            public Func<XInputGamepad, bool> resolve;
            public InputDeviceMatcher matcher;
            public string layoutName;
        }

        /// <summary>
        /// Registered layout resolvers for a given subtype.
        /// </summary>
        private static readonly List<XInputLayoutOverride> s_LayoutOverrides = new List<XInputLayoutOverride>();

        /// <summary>
        /// Initializes the layout resolver.
        /// </summary>
        internal static void Initialize()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            InputSystem.onFindLayoutForDevice += FindXInputDeviceLayout;
#endif
        }

        /// <summary>
        /// Determines the layout to use for the given device description.
        /// </summary>
        private static string FindXInputDeviceLayout(ref InputDeviceDescription description, string matchedLayout,
            InputDeviceExecuteCommandDelegate executeDeviceCommand)
        {
            // Ignore non-XInput devices
            if (description.interfaceName != InterfaceName)
                return null;

            Debug.Log($"[XInputLayoutFinder] Received XInput device. Matched layout: {matchedLayout ?? "None"}, description:\n{description}");

            // Parse capabilities
            if (!Utilities.TryParseJson<XInputCapabilities>(description.capabilities, out var capabilities))
            {
                Debug.LogError($"[XInputLayoutFinder] Failed to parse device capabilities!");
                return DefaultLayoutIfNull(matchedLayout);
            }

            // Check if the subtype has an override registered
            var overrides = s_LayoutOverrides.Where((entry) => entry.subType == (int)capabilities.subType);
            if (!overrides.Any())
            {
                Debug.Log($"[XInputLayoutFinder] No overrides for subtype '{capabilities.subType}'");
                return DefaultLayoutIfNull(matchedLayout);
            }

            // Get device state
            XInputGamepad state = default;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (XInputGetState(capabilities.userIndex, out var packet) != 0)
            {
                Debug.LogError($"[XInputLayoutFinder] Failed to get state for user index {capabilities.userIndex}!");
                return DefaultLayoutIfNull(matchedLayout);
            }

            state = packet.gamepad;
#endif

            // Go through device matchers
            XInputLayoutOverride? matchedEntry = null;
            float greatestMatch = 0f;
            foreach (var entry in overrides)
            {
                // Ignore invalid overrides
                if (string.IsNullOrEmpty(entry.layoutName))
                {
                    Debug.LogWarning($"[XInputLayoutFinder] No layout found on override entry with matcher '{entry.matcher}'!");
                    continue;
                }

                // Ignore non-matching resolvers
                if (!entry.resolve(state))
                {
                    Debug.Log($"[XInputLayoutFinder] Override '{entry.layoutName}' does not match");
                    continue;
                }

                // Keep track of the best match
                float match = entry.matcher.MatchPercentage(description);
                if (match > greatestMatch)
                {
                    greatestMatch = match;
                    matchedEntry = entry;
                }

                Debug.Log($"[XInputLayoutFinder] Matcher for override '{entry.layoutName}': Score: {match}, properties: {entry.matcher}");
            }

            // Use matched entry if available
            if (matchedEntry.HasValue)
            {
                var entry = matchedEntry.GetValueOrDefault();
                if (!string.IsNullOrEmpty(entry.layoutName))
                {
                    Debug.Log($"[XInputLayoutFinder] Using layout '{entry.layoutName}'");
                    return entry.layoutName;
                }
            }

            // Use existing or default layout otherwise
            Debug.Log($"[XInputLayoutFinder] No overrides match.");
            return DefaultLayoutIfNull(matchedLayout);
        }

        private static string DefaultLayoutIfNull(string matchedLayout)
            => string.IsNullOrEmpty(matchedLayout) ? nameof(XInputControllerWindows) : null;

        /// <summary>
        /// Registers <typeparamref name="TDevice"/> to the input system as an XInput device using the specified
        /// <see cref="DeviceSubType"/>, with a layout resolver used to identify it.
        /// </summary>
        internal static void RegisterLayout<TDevice>(DeviceSubType subType, Func<XInputGamepad, bool> resolveLayout,
            InputDeviceMatcher matcher = default)
            where TDevice : InputDevice
            => RegisterLayout<TDevice>((int)subType, resolveLayout, matcher);

        /// <summary>
        /// Registers <typeparamref name="TDevice"/> to the input system as an XInput device using the specified
        /// <see cref="XInputNonStandardSubType"/>, with a layout resolver used to identify it.
        /// </summary>
        internal static void RegisterLayout<TDevice>(XInputNonStandardSubType subType, Func<XInputGamepad, bool> resolveLayout,
            InputDeviceMatcher matcher = default)
            where TDevice : InputDevice
            => RegisterLayout<TDevice>((int)subType, resolveLayout, matcher);

        /// <summary>
        /// Registers <typeparamref name="TDevice"/> to the input system as an XInput device using the specified
        /// subtype, with a layout resolver used to identify it.
        /// </summary>
        internal static void RegisterLayout<TDevice>(int subType, Func<XInputGamepad, bool> resolveLayout,
            InputDeviceMatcher matcher = default)
            where TDevice : InputDevice
        {
            // Register to the input system
            InputSystem.RegisterLayout<TDevice>();

            // Add to override list
            s_LayoutOverrides.Add(new XInputLayoutOverride()
            {
                subType = subType,
                resolve = resolveLayout,
                matcher = matcher.empty ? GetMatcher(subType) : matcher,
                layoutName = typeof(TDevice).Name
            });
        }

        /// <summary>
        /// Registers <typeparamref name="TDevice"/> to the input system as an XInput device using the specified
        /// <see cref="DeviceSubType"/>.
        /// </summary>
        internal static void RegisterLayout<TDevice>(DeviceSubType subType)
            where TDevice : InputDevice
            => RegisterLayout<TDevice>((int)subType);

        /// <summary>
        /// Registers <typeparamref name="TDevice"/> to the input system as an XInput device using the specified
        /// <see cref="XInputNonStandardSubType"/>.
        /// </summary>
        internal static void RegisterLayout<TDevice>(XInputNonStandardSubType subType)
            where TDevice : InputDevice
            => RegisterLayout<TDevice>((int)subType);

        /// <summary>
        /// Registers <typeparamref name="TDevice"/> to the input system as an XInput device using the specified subtype.
        /// </summary>
        internal static void RegisterLayout<TDevice>(int subType)
            where TDevice : InputDevice
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            InputSystem.RegisterLayout<TDevice>(matches: GetMatcher(subType));
#endif
        }

        /// <summary>
        /// Gets a matcher that matches XInput Santroller devices with the given subtype.
        /// </summary>
        internal static InputDeviceMatcher GetMatcher(int subType)
        {
            return new InputDeviceMatcher()
                .WithInterface(InterfaceName)
                .WithCapability("subType", subType);
        }
    }
}