using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

using static Pansystar.Extensions;

namespace PraTaiko
{
	class COldConfig
    {
		static COldConfig MainConfig;
        public bool Debug = false;
        public string OpenEditor { get; private set; }
		public Size DrawSize { get; private set; }
		public Size WindowSize { get; private set; }
		public string PTSFolder { get; private set; }
        public string SoundFolder { get; private set; }
        public string ImageFolder { get; private set; }
        public short SongSelectInit { get; private set; }
        public static COldConfig Get()
        {
            return MainConfig;
        }
		public static COldConfig Construct(MainControl m)
		{
			if (MainConfig != null)
			{
                throw new ExistingException(MainConfig.GetType().Name);
			}
            MainConfig = new COldConfig();
			return MainConfig;
		}
        COldConfig()
        {
            string buf;
            string fileName = "config.ini";
            OpenEditor = "notepad.exe";
            #region 正規表現宣言
            Regex r_OpenEditor = new Regex(@"OpenEditor=");
            Regex r_ScreenMode = new Regex(@"^ScreenMode=");
            Regex r_WindowSizeX = new Regex(@"^WindowSizeX=");
            Regex r_PTSFolder = new Regex(@"^PTSFolder=");
            Regex r_ImageFolder = new Regex(@"^ImageFolder=");
            Regex r_SoundFolder = new Regex(@"^SoundFolder=");
            Regex r_SongSelectInit = new Regex(@"^SongSelectInit=");
            Regex r_JudgeRangeGreat = new Regex(@"^JudgeRangeGreat=");
            Regex r_JudgeRangeGood = new Regex(@"^JudgeRangeGood=");
            Regex r_JudgeRangeBad = new Regex(@"^JudgeRangeBad=");
            StreamReader sr = null;
            #endregion
            try
            {
                sr = new StreamReader("config.ini", Encoding.GetEncoding("Shift_JIS"));
            }
            catch (FileNotFoundException)
            {
                switch (MessageBox.Show(fileName + "が見つかりません。ファイルを生成しますか?", "configエラー", MessageBoxButtons.YesNo))
                {
                    case DialogResult.Yes:
                        StreamWriter sw = new StreamWriter(fileName);
                        sw.WriteLine("[config]");
                        sw.WriteLine("OpenEditor=notepad.exe");
                        sw.WriteLine("ScreenMode=window");
                        sw.WriteLine("WindowSizeX=1280");
                        sw.WriteLine("PTSFolder=PTS");
                        sw.WriteLine("ImageFolder=image");
                        sw.WriteLine("SoundFolder=sound");
                        sw.WriteLine("SongSelectInit=song");
                        sw.WriteLine("JudgeRangeGreat=50");
                        sw.WriteLine("JudgeRangeGood=150");
                        sw.WriteLine("JudgeRangeBad=200");
                        sw.Close();
                        sr = new StreamReader(fileName);
                        break;
                    case DialogResult.No:
                        MessageBox.Show("初期化に失敗しました。", "エラー", MessageBoxButtons.OK);
                        Environment.Exit(0);
                        break;
                }
            }
			DrawSize = new Size(1280);
            while (!sr.EndOfStream)
            {
                buf = sr.ReadLine();
                if (r_OpenEditor.IsMatch(buf))
                {
                    #region OpenEditor
                    string str = r_OpenEditor.Replace(buf, "");
                    if (str == "")
                    {
                        OpenEditor = "notepad.exe";
                    }
                    else
                    {
                        if (File.Exists(str) == true)
                        {
                            OpenEditor = str;
                        }
                        else
                        {
                            MessageBox.Show("OpenEditorに記されているパスにアプリケーションを見つけることができませんでした。\n\n" + buf + "\n\n標準のメモ帳アプリを使用することにします。", "configエラー", MessageBoxButtons.OK);
                            Process.Start(OpenEditor, "config.ini");
                        }
                    }
                    #endregion
                }
                else if (r_WindowSizeX.IsMatch(buf))
                {
                    #region WindowSize
                    WindowSize = new Size(int.Parse(r_WindowSizeX.Replace(buf, "")));
                    #endregion
                }
                else if (r_PTSFolder.IsMatch(buf))
                {
                    #region PTSFolder
                    PTSFolder = r_PTSFolder.Replace(buf, "");
                    if (!Directory.Exists(PTSFolder))
                    {
                        MessageBox.Show("PTSFolderを見つけることができませんでした。\n\n" + buf + "\n\nconfig.iniを開き、ソフトを終了します。", "configエラー", MessageBoxButtons.OK);
                        Process.Start("notepad.exe", "config.ini");
                        Environment.Exit(0);
                    }
                    #endregion
                }
                else if (r_SoundFolder.IsMatch(buf))
                {
                    #region SoundFolder
                    SoundFolder = r_SoundFolder.Replace(buf, "");
                    if (!Directory.Exists(SoundFolder))
                    {
                        MessageBox.Show("SoundFolderを見つけることができませんでした。\n\n" + buf + "\n\nconfig.iniを開き、ソフトを終了します。", "configエラー", MessageBoxButtons.OK);
                        Process.Start("notepad.exe", "config.ini");
                        Environment.Exit(0);
                    }
                    #endregion
                }
                else if (r_ImageFolder.IsMatch(buf))
                {
                    #region ImageFolder
                    ImageFolder = r_ImageFolder.Replace(buf, "");
                    if (!Directory.Exists(ImageFolder))
                    {
                        MessageBox.Show("ImageFolderを見つけることができませんでした。\n\n" + buf + "\n\nconfig.iniを開き、ソフトを終了します。", "configエラー", MessageBoxButtons.OK);
                        Process.Start("notepad.exe", "config.ini");
                        Environment.Exit(0);
                    }
                    #endregion
                }
                else if (r_SongSelectInit.IsMatch(buf))
                {
                    #region SongSelectInit
                    switch (r_SongSelectInit.Replace(buf, ""))
                    {
                        case "genre":
                            SongSelectInit = 0;
                            break;
                        case "song":
                            SongSelectInit = 1;
                            break;
                        default:
                            if (DialogResult.Yes == MessageBox.Show("SongSelectInitに、不正な値が入力されているため、「song」として扱います。\n\n"+buf+"\n\nファイルを修正しますか?", "configエラー", MessageBoxButtons.YesNo))
                            {
                                Process.Start("notepad.exe", "config.ini");
                            }
                            SongSelectInit = 1;
                            break;
                    }
                    #endregion
                }
                else if (r_JudgeRangeGreat.IsMatch(buf))
                {
                    #region JudgeRangeGreat
                    PlayConfig.judgeRangeGreat = int.Parse(r_JudgeRangeGreat.Replace(buf, ""))/2;
                    #endregion
                }
                else if (r_JudgeRangeGood.IsMatch(buf))
                {
                    #region JudgeRangeGreat
                    PlayConfig.judgeRangeGood = int.Parse(r_JudgeRangeGood.Replace(buf, "")) / 2;
                    #endregion
                }
                else if (r_JudgeRangeBad.IsMatch(buf))
                {
                    #region JudgeRangeBad
                    PlayConfig.judgeRangeBad = int.Parse(r_JudgeRangeBad.Replace(buf, ""))/2;
                    #endregion
                }
            }
            sr.Close();
        }        
    }
    public class Size
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Size(int x)
        {
            X = x;
            Y = x / 16 * 9;
        }
    }
}