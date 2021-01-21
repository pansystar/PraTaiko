using System.Text;
using static DxLibDLL.DX;
using Pansystar;
using static Pansystar.Extensions;
using System;
using System.Diagnostics;
using System.IO;

namespace PraTaiko
{
    interface ICommand
    {
    }
    class CCommand : ICommand
    {
        static CCommand Command;
        static SSongSelect SS;
        TextExtra[] te;
        StringBuilder sb;
        CMainConfig MainConfig;

        int font;
        int size = 24;
        int handle;

        void ActionAuto(string str)
        {
            int temp;
            if (int.TryParse(str, out temp))
            {
                switch (temp)
                {
                    case 0:
                        PlayConfig.ChangeAuto(false);
                        break;
                    case 1:
                        PlayConfig.ChangeAuto(true);
                        break;
                }
            }
            else
            {
                switch (str.Replace(" ", ""))
                {
                    case "":
                        PlayConfig.ChangeAuto();
                        break;
                }
            }
        }
        void ActionChartOpen(string str)
        {
            Process.Start(MainConfig.TextEditor, SS.ChartCurrentFilePath(this));
        }
        void ActionDebug(string str)
        {
            int temp;
            if (int.TryParse(str, out temp))
            {
                switch (temp)
                {
                    case 0:
                        MainConfig.ChangeDebug(false);
                        break;
                    case 1:
                        MainConfig.ChangeDebug(true);
                        break;
                }
            }
            else
            {
                switch (str.Replace(" ", ""))
                {
                    case "":
                        MainConfig.ChangeDebug();
                        break;
                }
            }
        }
        void ActionDemo(string str)
        {
            int temp;
            if (int.TryParse(str, out temp))
            {
                switch (temp)
                {
                    case 0:
                        SS.SetDemoPlayMode(this, false);
                        break;
                    case 1:
                        SS.SetDemoPlayMode(this, true);
                        break;
                }
            }
            else
            {
                switch (str.Replace(" ", ""))
                {
                    case "":
                        SS.SetDemoPlayMode(this);
                        break;
                }
            }
        }
        void ActionExit(string str)
        {
            Environment.Exit(0);
        }
        void ActionFolderOpen(string str)
        {
            Process.Start("EXPLORER.EXE", Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(MainConfig.Resources))));
        }
        void ActionFps(string str)
        {
            Fps.ChangeDrawFlag(this);
        }
        void ActionGenreOpen(string str)
        {
            Process.Start(MainConfig.TextEditor, SS.GenreCurrentFilePath(this));
        }
        void ActionNoteOff(string str)
        {
            int temp;
            if (int.TryParse(str, out temp))
            {
                switch (temp)
                {
                    case 0:
                        PlayConfig.ChangeNoteOff(false);
                        break;
                    case 1:
                        PlayConfig.ChangeNoteOff(true);
                        break;
                }
            }
            else
            {
                switch (str.Replace(" ", ""))
                {
                    case "":
                        PlayConfig.ChangeNoteOff();
                        break;
                }
            }
        }
        void ActionReplacePer(string str)
        {
            int p;
            if (int.TryParse(str, out p))
            {
                PlayConfig.ChangeReplacePer(p);
            }
        }
        void ActionSpeed(string str)
        {
            float temp;
            if (float.TryParse(str, out temp))
            {
                PlayConfig.ChangeSpeed(temp);
            }
        }
        void ActionTone(string str)
        {
            int i;
            if(int.TryParse(str, out i))
            {
                CTone.Get().SetIndex(this, i);
            }
        }
        void ActionVolume(string str)
        {
            Process.Start("sndvol.exe");
        }

        void SearchAndRun()
        {
            te.SetString(sb.ToString());
        }

        public void Start()
        {
            handle = MakeKeyInput(50, 1, 1, 0);
            SetActiveKeyInput(handle);
            SetKeyInputString("/", handle);
            SetKeyInputStringFont(font);
            Key.SetKeyAcqu(false);
        }
        public void Update()
        {
            if (Key.GetCount(Key.INPUT_SLASH) == 1) Start();
        }
        public void Draw()
        {
            switch (CheckKeyInput(handle))
            {
                case 0:
                    SetDrawBlendMode(DX_BLENDMODE_ALPHA, 128);
                    DrawBox(0, MainConfig.DrawHeight, MainConfig.DrawWidth, MainConfig.DrawHeight - size - 8, GetColor(0, 0, 0), 1);
                    SetDrawBlendMode(DX_BLENDMODE_NOBLEND, 0);
                    DrawKeyInputString(0, MainConfig.DrawHeight - size - 4, handle);
                    break;
                case 1:
                    GetKeyInputString(sb, handle);
                    SearchAndRun();
                    DeleteKeyInput(handle);
                    sb.Clear();
                    Key.SetKeyAcqu(true);
                    break;
                case 2:
                    DeleteKeyInput(handle);
                    sb.Clear();
                    Key.SetKeyAcqu(true);
                    break;
                case -1:
                    break;
                default:
                    PrintMessage("コマンド入力で意図しない状態を確認しました。");
                    break;
            }
        }
        public static CCommand Construct(MainControl m)
        {
            if (Command != null)
            {
                throw new ExistingException(Command.GetType().Name);
            }
            Command = new CCommand(m);
            return Command;
        }
        
        CCommand(MainControl m)
        {
            SS = SSongSelect.Get(this);
            MainConfig = CMainConfig.Get();
            font = CreateFontToHandle("ＤＦＰ勘亭流", size, -1, DX_FONTTYPE_ANTIALIASING);
            sb = new StringBuilder();
            te = new TextExtra[]
            {
                new TextExtra("/auto", ActionAuto),
                new TextExtra("/chart", ActionChartOpen),
                new TextExtra("/debug", ActionDebug),
                new TextExtra("/demo", ActionDemo),
                new TextExtra("/exit", ActionExit),
                new TextExtra("/folder", ActionFolderOpen),
                new TextExtra("/fps", ActionFps),
                new TextExtra("/genre", ActionGenreOpen),
                new TextExtra("/noteoff", ActionNoteOff),
                new TextExtra("/replaceper", ActionReplacePer),
                new TextExtra("/speed", ActionSpeed),
                new TextExtra("/tone", ActionTone),                
                new TextExtra("/volume", ActionVolume),
            };
        }
    }
}