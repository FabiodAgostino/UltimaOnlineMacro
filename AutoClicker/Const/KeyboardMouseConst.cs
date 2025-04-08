using AutoClicker.Models.System;

namespace AutoClicker.Const
{
    public static class KeyboardMouseConst
    {
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;

        // Codici messaggi per mouse
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_MOUSEMOVE = 0x0200;

        // Costanti per mouse input
        public const int WH_MOUSE_LL = 14;

        // Costanti per mouse_event

        // Codici tasti freccia
        public const int VK_LEFT = 0x25;    // Freccia sinistra
        public const int VK_UP = 0x26;      // Freccia su
        public const int VK_RIGHT = 0x27;   // Freccia destra
        public const int VK_DOWN = 0x28;    // Freccia giù
        public const int VK_F10 = 0x79;  // 121 in decimale

        // Codici tasti modificatori per combinazioni
        public const int VK_SHIFT = 0x10;   // Tasto Shift
        public const int VK_CONTROL = 0x11; // Tasto Control
        public const int VK_ALT = 0x12;     // Tasto Alt

        // Flags per mouse_event
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

        public static readonly int[] ArrowKeys = { VK_UP, VK_RIGHT, VK_DOWN, VK_LEFT };

        public static readonly ArrowCombination[] ValidCombinations = new ArrowCombination[]
       {
            new ArrowCombination(VK_UP, "Freccia SU", VK_RIGHT, "Freccia DESTRA"),      // Su + Destra
            new ArrowCombination(VK_UP, "Freccia SU", VK_LEFT, "Freccia SINISTRA"),     // Su + Sinistra
            new ArrowCombination(VK_DOWN, "Freccia GIÙ", VK_RIGHT, "Freccia DESTRA"),   // Giù + Destra
            new ArrowCombination(VK_DOWN, "Freccia GIÙ", VK_LEFT, "Freccia SINISTRA")   // Giù + Sinistra
       };

        public static readonly string[] ArrowNames = { "Freccia SU", "Freccia DESTRA", "Freccia GIÙ", "Freccia SINISTRA" };


    }
}
