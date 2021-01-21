using Pansystar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Pansystar.Extensions;

namespace PraTaiko
{
    class CMainConfig
    {
        static CMainConfig MainConfig;

        public readonly string TextEditor = @"notepad.exe";

        public readonly string Resources = @"Resources\";
        public readonly string Theme = @"Resources\Theme\";
        public readonly string File = @"MainConfig.ini";

        public readonly int DrawWidth = 1280;
        public readonly int DrawHeight = 720;

        public int DrawCenterX
        {
            get
            {
                return DrawWidth / 2;
            }
        }
        public int DrawCenterY
        {
            get
            {
                return DrawHeight / 2;
            }
        }

        public bool Debug { get; private set; } = false;
        public void ChangeDebug()
        {
            Debug = !Debug;
        }
        public void ChangeDebug(bool d)
        {
            Debug = d;
        }

        public string SongSelect { get; private set; }
        public string Play { get; private set; }
        public string Result { get; private set; }
        public string Tone { get; private set; }

        public int WindowWidth { get; private set; } = 1280;
        public int WindowHeight { get; private set; } = 720;

        public string SkinFolder { get; private set; }
        public string ChartFolder { get; private set; }

        public int JudgeGreat { get; private set; } = 60;
        public int JudgeGood { get; private set; } = 120;
        public int JudgeBad { get; private set; } = 180;

        void SetDebug(string str)
        {
            int d;
            if (int.TryParse(str, out d))
            {
                switch (d)
                {
                    case 0:
                        Debug = false;
                        break;
                    case 1:
                        Debug = true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (str.ToLower())
                {
                    case "false":
                        Debug = false;
                        break;
                    case "true":
                        Debug = true;
                        break;
                }
            }
        }
        void SetWindow(string str)
        {
            int w;
            if (int.TryParse(str, out w))
            {
                WindowWidth = w;
                WindowHeight = w / 16 * 9;
            }
            else
            {
                ErrorBox("WindowWidthに不正な値が入力されています。\n訂正してください。", File, MessageBoxButtons.OK, File);
            }
        }

        void SetPath()
        {
            SongSelect = SkinFolder + @"SongSelect\";
            Play = SkinFolder + @"Play\";
            Result = SkinFolder + @"Result\";
            Tone = SkinFolder + @"Tone\";
        }

        void SetSkinFolder(string str)
        {
            str = str.Replace('/', '\\');
            if (Directory.Exists(Theme + str))
            {
                switch (str.Last())
                {
                    case '\\':
                        break;
                    default:
                        str += '\\';
                        break;
                }
                SkinFolder = Theme + str;
                SetPath();
            }
            else
            {
                ErrorBox(str + "フォルダを見つけることが出来ませんでした。\nThemeフォルダ内を確認し、訂正してください。", File, MessageBoxButtons.OK, File);
            }
        }
        void SetChartFolder(string str)
        {
            ChartFolder = Resources + str.Replace('/', '\\');
            if (Directory.Exists(ChartFolder))
            {
                switch (str.Last())
                {
                    case '\\':
                        break;
                    default:
                        str += '\\';
                        break;
                }
            }
            else
            {
                ErrorBox(str + "フォルダを見つけることが出来ませんでした。\nThemeフォルダ内を確認し、訂正してください。", File, MessageBoxButtons.OK, File);
            }
        }

        public static CMainConfig Get()
        {
            return MainConfig;
        }
        public static CMainConfig Construct(MainControl m)
        {
            if (MainConfig != null)
            {
                new ExistingException(MainConfig.GetType().Name);
            }
            MainConfig = new CMainConfig();
            return MainConfig;
        }

        CMainConfig()
        {
            if (!System.IO.File.Exists(File))
            {
                ErrorBox(File + "ファイルがないため、新規作成します。", "MainConfig", MessageBoxButtons.OK, false);
                using (StreamWriter sw = new StreamWriter(File, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    sw.WriteLine("[Main]");
                    sw.WriteLine("Debug=");
                    sw.WriteLine("WindowWidth=1280");
                    sw.WriteLine("SkinFolder=Default");
                    sw.WriteLine("ChartFolder=Chart");
                    sw.WriteLine("JudgeGreat=");
                    sw.WriteLine("JudgeGood=");
                    sw.WriteLine("JudgeBad=");
                }
                PrintMessage(File + "を新規作成しました。");
            }
            TextExtra[] te =
            {
                new TextExtra("Debug=", SetDebug),
                new TextExtra("WindowWidth=",SetWindow),
                new TextExtra("SkinFolder=",SetSkinFolder),
                new TextExtra("ChartFolder=",SetChartFolder),
            };
            GetStreamData(File, te);
        }
    }
}
