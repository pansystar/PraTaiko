using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraTaiko
{
    class CTone
    {
        static CTone Tone;
        static CMainConfig MainConfig;

        const int INIT_INDEX = 0;
        const string BASE = @"Base";
        const string BASE_PATH = BASE + @"\";
        const string DON = @"don.wav";
        const string KAT = @"kat.wav";
        const string BALLOON = @"balloon.wav";

        int index = INIT_INDEX;
        public string DonPath
        {
            get
            {
                string path = MainConfig.Tone + ToneFolders[index] + DON;
                switch (File.Exists(path))
                {
                    case true:
                        return path;
                    default:
                        return MainConfig.Tone + BASE_PATH + DON;
                }
            }
        }
        public string KatPath
        {
            get
            {
                string path = MainConfig.Tone + ToneFolders[index] + KAT;
                switch (File.Exists(path))
                {
                    case true:
                        return path;
                    default:
                        return MainConfig.Tone + BASE_PATH + KAT;
                }
            }
        }
        public string BalloonPath
        {
            get
            {
                string path = MainConfig.Tone + ToneFolders[index] + BALLOON;
                switch (File.Exists(path))
                {
                    case true:
                        return path;
                    default:
                        return MainConfig.Tone + BASE_PATH + BALLOON;
                }
            }
        }
        string[] Names;
        string[] ToneFolders;

        public void SetIndex(ICommand c, int i)
        {
            if (0 <= i && i <= ToneFolders.Length - 1)
            {
                index = i;
            }
        }

        public static CTone Get()
        {
            return Tone;
        }
        public static void Construct()
        {
            Tone = new CTone();
        }
        CTone()
        {
            MainConfig = CMainConfig.Get();
            var lls = Directory.EnumerateDirectories(MainConfig.Tone).Where(s => Path.GetFileName(s) != BASE);
            ToneFolders = lls.ToArray();
            Names = Enumerable.Repeat("", ToneFolders.Length).ToArray();
            for (int i = 0; i < ToneFolders.Length; i++)
            {
                string tf = Path.GetFileName(ToneFolders[i]);
                if (Names.Length >= 2)
                {
                    Names[i] = tf.Split('_')[1];
                }
                else
                {
                    Names[i] = tf;
                }
                ToneFolders[i] = tf + @"\";
            }
        }
    }
}
