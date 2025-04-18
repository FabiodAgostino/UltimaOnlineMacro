using AutoClicker.Models.System;

namespace AutoClicker.Const
{
    public static class KeyboardMouseConst
    {
        // Codici messaggi per mouse
        public const uint WM_LBUTTONDOWN = 0x0201;

        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_MOUSEMOVE = 0x0200;

        // Costanti per mouse input
        public const int WH_MOUSE_LL = 14;

        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        // Costanti per input di tastiera
        public const int INPUT_KEYBOARD = 1;

        public const int INPUT_MOUSE = 0;
        public const uint KEYEVENTF_KEYDOWN = 0x0000;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public const int WH_KEYBOARD_LL = 13;

        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const int HOTKEY_ID = 1;

        // Array tasti freccia
        public static readonly byte[] ArrowKeys = { VK_UP, VK_RIGHT, VK_DOWN, VK_LEFT };

        public static readonly ArrowCombination[] ValidCombinations = new ArrowCombination[]
       {
            new ArrowCombination(VK_UP, "Freccia SU", VK_RIGHT, "Freccia DESTRA"),      // Su + Destra
            new ArrowCombination(VK_UP, "Freccia SU", VK_LEFT, "Freccia SINISTRA"),     // Su + Sinistra
            new ArrowCombination(VK_DOWN, "Freccia GIÙ", VK_RIGHT, "Freccia DESTRA"),   // Giù + Destra
            new ArrowCombination(VK_DOWN, "Freccia GIÙ", VK_LEFT, "Freccia SINISTRA")   // Giù + Sinistra
       };

        public static readonly string[] ArrowNames = { "Freccia SU", "Freccia DESTRA", "Freccia GIÙ", "Freccia SINISTRA" };

        // Costanti per messaggi di tastiera
        public const uint WM_KEYDOWN = 0x0100;

        public const uint WM_KEYUP = 0x0101;

        // Costanti per virtual key codes
        // Tasti numerici (0-9)
        public const byte VK_0 = 0x30;

        public const byte VK_1 = 0x31;
        public const byte VK_2 = 0x32;
        public const byte VK_3 = 0x33;
        public const byte VK_4 = 0x34;
        public const byte VK_5 = 0x35;
        public const byte VK_6 = 0x36;
        public const byte VK_7 = 0x37;
        public const byte VK_8 = 0x38;
        public const byte VK_9 = 0x39;

        // Tasti alfabetici (A-Z)
        public const byte VK_A = 0x41;

        public const byte VK_B = 0x42;
        public const byte VK_C = 0x43;
        public const byte VK_D = 0x44;
        public const byte VK_E = 0x45;
        public const byte VK_F = 0x46;
        public const byte VK_G = 0x47;
        public const byte VK_H = 0x48;
        public const byte VK_I = 0x49;
        public const byte VK_J = 0x4A;
        public const byte VK_K = 0x4B;
        public const byte VK_L = 0x4C;
        public const byte VK_M = 0x4D;
        public const byte VK_N = 0x4E;
        public const byte VK_O = 0x4F;
        public const byte VK_P = 0x50;
        public const byte VK_Q = 0x51;
        public const byte VK_R = 0x52;
        public const byte VK_S = 0x53;
        public const byte VK_T = 0x54;
        public const byte VK_U = 0x55;
        public const byte VK_V = 0x56;
        public const byte VK_W = 0x57;
        public const byte VK_X = 0x58;
        public const byte VK_Y = 0x59;
        public const byte VK_Z = 0x5A;

        // Tasti funzione (F1-F12)
        public const byte VK_F1 = 0x70;

        public const byte VK_F2 = 0x71;
        public const byte VK_F3 = 0x72;
        public const byte VK_F4 = 0x73;
        public const byte VK_F5 = 0x74;
        public const byte VK_F6 = 0x75;
        public const byte VK_F7 = 0x76;
        public const byte VK_F8 = 0x77;
        public const byte VK_F9 = 0x78;
        public const byte VK_F10 = 0x79;
        public const byte VK_F11 = 0x7A;
        public const byte VK_F12 = 0x7B;

        // Tasti direzionali
        public const byte VK_UP = 0x26;

        public const byte VK_DOWN = 0x28;
        public const byte VK_LEFT = 0x25;
        public const byte VK_RIGHT = 0x27;

        // Tasti di navigazione
        public const byte VK_ESCAPE = 0x1B;

        public const byte VK_TAB = 0x09;
        public const byte VK_RETURN = 0x0D; // Enter
        public const byte VK_SPACE = 0x20;
        public const byte VK_BACK = 0x08; // Backspace
        public const byte VK_HOME = 0x24;
        public const byte VK_END = 0x23;
        public const byte VK_INSERT = 0x2D;
        public const byte VK_DELETE = 0x2E;
        public const byte VK_PRIOR = 0x21; // Page Up
        public const byte VK_NEXT = 0x22; // Page Down

        // Tasti di controllo
        public const byte VK_CONTROL = 0x11;

        public const byte VK_MENU = 0x12; // ALT
        public const byte VK_SHIFT = 0x10;
        public const byte VK_LWIN = 0x5B; // Tasto Windows sinistro
        public const byte VK_RWIN = 0x5C; // Tasto Windows destro
        public const byte VK_APPS = 0x5D; // Tasto Menu contestuale
        public const byte VK_SNAPSHOT = 0x2C; // Print Screen
        public const byte VK_SCROLL = 0x91; // Scroll Lock
        public const byte VK_PAUSE = 0x13; // Pause/Break
        public const byte VK_CAPITAL = 0x14; // Caps Lock
        public const byte VK_NUMLOCK = 0x90; // Num Lock

        // Tasti OEM / locali
        public const byte VK_OEM_1 = 0xBA; // È nella tastiera italiana

        public const byte VK_OEM_PLUS = 0xBB; // + (più)
        public const byte VK_OEM_COMMA = 0xBC; // , (virgola)
        public const byte VK_OEM_MINUS = 0xBD; // - (meno)
        public const byte VK_OEM_PERIOD = 0xBE; // . (punto)
        public const byte VK_OEM_2 = 0xBF; // / (slash)
        public const byte VK_OEM_3 = 0xC0; // ` (apice inverso/backtick, tilde)
        public const byte VK_OEM_4 = 0xDB; // [ (parentesi quadra aperta)
        public const byte VK_OEM_5 = 0xDC; // \ (backslash)
        public const byte VK_OEM_6 = 0xDD; // ] (parentesi quadra chiusa)
        public const byte VK_OEM_7 = 0xDE; // ' (apostrofo)
        public const byte VK_OEM_8 = 0xDF; // Vari a seconda della tastiera
        public const byte VK_OEM_102 = 0xE2; // < > | (tastiera italiana)

        // Tasti del tastierino numerico
        public const byte VK_NUMPAD0 = 0x60;

        public const byte VK_NUMPAD1 = 0x61;
        public const byte VK_NUMPAD2 = 0x62;
        public const byte VK_NUMPAD3 = 0x63;
        public const byte VK_NUMPAD4 = 0x64;
        public const byte VK_NUMPAD5 = 0x65;
        public const byte VK_NUMPAD6 = 0x66;
        public const byte VK_NUMPAD7 = 0x67;
        public const byte VK_NUMPAD8 = 0x68;
        public const byte VK_NUMPAD9 = 0x69;
        public const byte VK_MULTIPLY = 0x6A; // * (moltiplicazione)
        public const byte VK_ADD = 0x6B; // + (addizione)
        public const byte VK_SEPARATOR = 0x6C; // , (separatore)
        public const byte VK_SUBTRACT = 0x6D; // - (sottrazione)
        public const byte VK_DECIMAL = 0x6E; // . (decimale)
        public const byte VK_DIVIDE = 0x6F; // / (divisione)

        // Costanti per codici di scansione
        public const ushort SC_F1 = 0x3B;

        public const ushort SC_F2 = 0x3C;
        public const ushort SC_F3 = 0x3D;
        public const ushort SC_F4 = 0x3E;
        public const ushort SC_F5 = 0x3F;
        public const ushort SC_F6 = 0x40;
        public const ushort SC_F7 = 0x41;
        public const ushort SC_F8 = 0x42;
        public const ushort SC_F9 = 0x43;
        public const ushort SC_F10 = 0x44;
        public const ushort SC_F11 = 0x57;
        public const ushort SC_F12 = 0x58;
        public const ushort SC_UP = 0x48;
        public const ushort SC_DOWN = 0x50;
        public const ushort SC_LEFT = 0x4B;
        public const ushort SC_RIGHT = 0x4D;
        public const ushort SC_ESCAPE = 0x01;
        public const ushort SC_TAB = 0x0F;
        public const ushort SC_LCONTROL = 0x1D;
        public const ushort SC_LALT = 0x38;
        public const ushort SC_LSHIFT = 0x2A;
        public const ushort SC_RSHIFT = 0x36;
        public const ushort SC_RETURN = 0x1C;
        public const ushort SC_SPACE = 0x39;
        public const ushort SC_BACK = 0x0E;
        public const ushort SC_OEM_1 = 0x27; // È nella tastiera italiana
        public const ushort SC_OEM_3 = 0x29; // ` (apice inverso/backtick, tilde)

        // Flag per i tasti estesi
        public const uint EXTENDED_KEY_FLAG = 0x01000000;

        // Costanti per generazione di lParam
        public const uint KEY_PRESSED = 0x00000001;

        public const uint KEY_RELEASED_TRANSITION = 0xC0000001;
        public const uint EXTENDED_KEY_RELEASED_TRANSITION = 0xC1000001;
        public static readonly Keys[] ModifierKeys = { Keys.ControlKey, Keys.ShiftKey, Keys.Menu };

        // Dizionario di mappatura tra VK e codici di scansione
        public static readonly Dictionary<byte, ushort> VirtualKeyToScanCode = new Dictionary<byte, ushort>
        {
            // Tasti funzione
            { VK_F1, SC_F1 },
            { VK_F2, SC_F2 },
            { VK_F3, SC_F3 },
            { VK_F4, SC_F4 },
            { VK_F5, SC_F5 },
            { VK_F6, SC_F6 },
            { VK_F7, SC_F7 },
            { VK_F8, SC_F8 },
            { VK_F9, SC_F9 },
            { VK_F10, SC_F10 },
            { VK_F11, SC_F11 },
            { VK_F12, SC_F12 },

            // Tasti direzionali
            { VK_UP, SC_UP },
            { VK_DOWN, SC_DOWN },
            { VK_LEFT, SC_LEFT },
            { VK_RIGHT, SC_RIGHT },

            // Tasti di controllo e navigazione
            { VK_ESCAPE, SC_ESCAPE },
            { VK_TAB, SC_TAB },
            { VK_CONTROL, SC_LCONTROL },
            { VK_MENU, SC_LALT },
            { VK_SHIFT, SC_LSHIFT },
            { VK_RETURN, SC_RETURN },
            { VK_SPACE, SC_SPACE },
            { VK_BACK, SC_BACK },

            // Tasti OEM
            { VK_OEM_1, SC_OEM_1 }, // È italiana
            { VK_OEM_3, SC_OEM_3 }  // ` (backtick)
        };

        public static readonly HashSet<byte> ExtendedKeys = new HashSet<byte>
        {
            VK_UP,
            VK_DOWN,
            VK_LEFT,
            VK_RIGHT,
            VK_HOME,
            VK_END,
            VK_PRIOR,  // Page Up
            VK_NEXT,   // Page Down
            VK_INSERT,
            VK_DELETE,
            VK_DIVIDE, // / del tastierino numerico
            VK_NUMLOCK
        };
    }
}