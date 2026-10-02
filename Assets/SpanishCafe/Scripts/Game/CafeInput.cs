using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpanishCafe
{
    // Keyboard shortcuts that work with the new Input System and the old Input Manager
  
    public static class CafeInput
    {
        public static bool TalkPressed => Pressed(KeyCode.E);
        public static bool HintPressed => Pressed(KeyCode.H);
        public static bool SubtitlesPressed => Pressed(KeyCode.T);
        public static bool ClosePressed => Pressed(KeyCode.Escape);
        public static bool ContinuePressed => Pressed(KeyCode.Return) || Pressed(KeyCode.Space);

        // Returns 0 for the 1 key, 1 for the 2 key, and so on -1 means nothing was pressed
        public static int NumberPressed()
        {
            for (int i = 0; i < 9; i++)
            {
                if (Pressed((KeyCode)((int)KeyCode.Alpha1 + i)) || Pressed((KeyCode)((int)KeyCode.Keypad1 + i)))
                    return i;
            }
            return -1;
        }

        static bool Pressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            var mapped = ToInputSystemKey(key);
            return mapped != Key.None && keyboard[mapped].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        static Key ToInputSystemKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.E: return Key.E;
                case KeyCode.H: return Key.H;
                case KeyCode.T: return Key.T;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                case KeyCode.Keypad1: return Key.Numpad1;
                case KeyCode.Keypad2: return Key.Numpad2;
                case KeyCode.Keypad3: return Key.Numpad3;
                case KeyCode.Keypad4: return Key.Numpad4;
                case KeyCode.Keypad5: return Key.Numpad5;
                case KeyCode.Keypad6: return Key.Numpad6;
                case KeyCode.Keypad7: return Key.Numpad7;
                case KeyCode.Keypad8: return Key.Numpad8;
                case KeyCode.Keypad9: return Key.Numpad9;
                default: return Key.None;
            }
        }
#endif
    }
}
