using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace PraTaiko
{
    public static class Key
    {
        public static bool keyAcqu { get; private set; } = true;
		static byte[] tmpKey = new byte[256];
        static int[] count = new int[256];
        public static void SetKeyAcqu(bool ka)
        {
            keyAcqu = ka;
        }
        #region キーID
        public const int Num0 = 11;
        public const int Num1 = 2;
        public const int Num2 = 3;
        public const int Num3 = 4;
        public const int Num4 = 5;
        public const int Num5 = 6;
        public const int Num6 = 7;
        public const int Num7 = 8;
        public const int Num8 = 9;
        public const int Num9 = 10;
        public const int A = 30;
        public const int Add = 78;
        public const int Apps = 221;
        public const int At = 145;
        public const int B = 48;
        public const int Back = 14;
        public const int Backslash = 43;
        public const int C = 46;
        public const int Capslock = 58;
        public const int Colon = 146;
        public const int Comma = 51;
        public const int Convert = 121;
        public const int D = 32;
        public const int INPUT_DECIMAL = 83;
        public const int Delete = 211;
        public const int Divide = 181;
        public const int Down = 208;
        public const int E = 18;
        public const int End = 207;
        public const int Escape = 1;
        public const int F = 33;
        public const int F1 = 59;
        public const int F10 = 68;
        public const int F11 = 87;
        public const int F12 = 88;
        public const int F2 = 60;
        public const int F3 = 61;
        public const int F4 = 62;
        public const int F5 = 63;
        public const int F6 = 64;
        public const int F7 = 65;
        public const int F8 = 66;
        public const int F9 = 67;
        public const int INPUT_G = 34;
        public const int INPUT_H = 35;
        public const int INPUT_HOME = 199;
        public const int INPUT_I = 23;
        public const int INPUT_INSERT = 210;
        public const int J = 36;
        public const int K = 37;
        public const int INPUT_KANA = 112;
        public const int INPUT_KANJI = 148;
        public const int INPUT_L = 38;
        public const int INPUT_LALT = 56;
        public const int INPUT_LBRACKET = 26;
        public const int INPUT_LCONTROL = 29;
        public const int INPUT_LEFT = 203;
        public const int INPUT_LSHIFT = 42;
        public const int INPUT_LWIN = 219;
        public const int M = 50;
        public const int INPUT_MINUS = 12;
        public const int INPUT_MULTIPLY = 55;
        public const int INPUT_N = 49;
        public const int INPUT_NOCONVERT = 123;
        public const int INPUT_NUMLOCK = 69;
        public const int INPUT_NUMPAD0 = 82;
        public const int INPUT_NUMPAD1 = 79;
        public const int INPUT_NUMPAD2 = 80;
        public const int INPUT_NUMPAD3 = 81;
        public const int INPUT_NUMPAD4 = 75;
        public const int INPUT_NUMPAD5 = 76;
        public const int INPUT_NUMPAD6 = 77;
        public const int INPUT_NUMPAD7 = 71;
        public const int INPUT_NUMPAD8 = 72;
        public const int INPUT_NUMPAD9 = 73;
        public const int INPUT_NUMPADENTER = 156;
        public const int INPUT_O = 24;
        public const int INPUT_P = 25;
        public const int INPUT_PAUSE = 197;
        public const int INPUT_PERIOD = 52;
        public const int INPUT_PGDN = 209;
        public const int INPUT_PGUP = 201;
        public const int INPUT_PREVTRACK = 144;
        public const int INPUT_Q = 16;
        public const int R = 19;
        public const int INPUT_RALT = 184;
        public const int INPUT_RBRACKET = 27;
        public const int INPUT_RCONTROL = 157;
        public const int INPUT_RETURN = 28;
        public const int INPUT_RIGHT = 205;
        public const int INPUT_RSHIFT = 54;
        public const int INPUT_RWIN = 220;
        public const int INPUT_S = 31;
        public const int INPUT_SCROLL = 70;
        public const int INPUT_SEMICOLON = 39;
        public const int INPUT_SLASH = 53;
        public const int INPUT_SPACE = 57;
        public const int INPUT_SUBTRACT = 74;
        public const int INPUT_SYSRQ = 183;
        public const int INPUT_T = 20;
        public const int INPUT_TAB = 15;
        public const int INPUT_U = 22;
        public const int INPUT_UP = 200;
        public const int INPUT_V = 47;
        public const int INPUT_W = 17;
        public const int INPUT_X = 45;
        public const int INPUT_Y = 21;
        public const int INPUT_YEN = 125;
        public const int INPUT_Z = 44;
        #endregion
        public static int GetCount(int code)
        {           
            return count[code];
        }
        public static int Reset()
        {
            GetHitKeyStateAll(out tmpKey[0]);
            for (int i = 0; i < 256; i++)
            {
                count[i] = 0;
            }
            return 0;
        }
        public static int Update()
        {
            GetHitKeyStateAll(out tmpKey[0]);
            for (int i = 0; i < 256; i++)
            {
                if (tmpKey[i] != 0 && keyAcqu == true)
                {
                    count[i]++;
                }
                else
                {
                    count[i] = 0;
                }
            }
            return 0;
        }
    }
}
