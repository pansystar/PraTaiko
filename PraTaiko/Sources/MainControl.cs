#define DEMOMUSIC
//#define PLAYFIRST
//#define DEMOMUSIC_FROM_MEMORY

using DxLibDLL;
using static DxLibDLL.DX;
using System.Collections.Generic;
using System.Text;
using static Pansystar.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Windows.Forms;
using Pansystar;
using System;

namespace PraTaiko
{
    enum EScene
    {
        None,
        SongSelect,
        Play,
        Result,
    }
    enum ECourseID
    {
        Empty = -1, Easy = 0, Normal, Hard, Oni, Ura
    }
    static class CourseIDExtensions
    {
        public static string Name(this ECourseID id)
        {
            switch (id)
            {
                case ECourseID.Easy:
                    return "かんたん";
                case ECourseID.Normal:
                    return "ふつう";
                case ECourseID.Hard:
                    return "むずかしい";
                case ECourseID.Oni:
                    return "おに";
                case ECourseID.Ura:
                    return "うら";
                default:
                    return "null";
            }
        }
    }
	static class NScene
	{
		static int Select(this EScene e)
		{
			switch (e)
			{
				case EScene.None:
					return None;
				case EScene.SongSelect:
					return SongSelect;
				case EScene.Play:
					return Play;
				case EScene.Result:
					return Result;
				default:
					ErrorBox("シーンエラー", "エラー", MessageBoxButtons.OK, true);
					return int.MinValue;
			}
		}

		public static int Now { get; private set; } = SongSelect;
		public static void SetNow(LSceneList m, int n) { Now = n; }

		public static int Next { get; private set; } = None;
		public static void SetNext(LSceneList l, int n) { Next = n; }
		public static void SetNext(BaseScene b, EScene e) { Next = e.Select(); }
		public static void SetNext(CSceneOut b, EScene e) { Next = e.Select(); }

		public const int None = -1;
		public const int SongSelect = 100;
		public const int Play = 200;
		public const int Result = 300;
	}
	class LSceneList : Dictionary<int, BaseScene>
	{
		static LSceneList SceneList;
        BaseScene NowScene;
		public void Start()
		{
            NowScene = this[NScene.Now];
            NowScene.Start();
        }
		public void Update()
		{
			if (NScene.Next != NScene.None)
			{
                NowScene.Finish();
				NScene.SetNow(this, NScene.Next);
				NScene.SetNext(this, NScene.None);
                NowScene = this[NScene.Now];
				NowScene.Start();
			}
            NowScene.Update();
		}
		public void Draw()
		{
            NowScene.Draw();
		}
		public void Finish()
		{
            NowScene.Finish();
		}
		public static LSceneList Construct(MainControl m)
		{
			if (SceneList != null)
			{
				throw new ExistingException(SceneList.GetType().Name);
			}
			SceneList = new LSceneList();
			return SceneList;
		}
		LSceneList() { }
	}
    class MainControl
    {
        #region 変数宣言

        COldConfig OldConfig;
        CMainConfig MainConfig;

        CCommand Command;
		
		LSceneList SceneList;

        SSongSelect SongSelect;
		SPlay Play;
		SResult Result;

        StringBuilder exeTitle;

		#endregion
		//List<Player> player;
		void WindowSizeKeyUpdate()
		{
			if (Key.GetCount(Key.Num0) == 1) { SetWindowSize(OldConfig.WindowSize.X, OldConfig.WindowSize.Y); PrintMessage("ウィンドウサイズを既定値の" + OldConfig.WindowSize.X + "x" + OldConfig.WindowSize.Y + "に変更しました。"); return; }
			if (Key.GetCount(Key.Num1) == 1) { SetWindowSize(640, 360); PrintMessage("ウィンドウサイズを640x360に変更しました。"); return; }
			if (Key.GetCount(Key.Num2) == 1) { SetWindowSize(800, 450); PrintMessage("ウィンドウサイズを800x450に変更しました。"); return; }
			if (Key.GetCount(Key.Num3) == 1) { SetWindowSize(960, 540); PrintMessage("ウィンドウサイズを960x540に変更しました。"); return; }
			if (Key.GetCount(Key.Num4) == 1) { SetWindowSize(1120, 630); PrintMessage("ウィンドウサイズを1120x630に変更しました。"); return; }
			if (Key.GetCount(Key.Num5) == 1) { SetWindowSize(1280, 720); PrintMessage("ウィンドウサイズを1280x720に変更しました。"); return; }
			if (Key.GetCount(Key.Num6) == 1) { SetWindowSize(1440, 810); PrintMessage("ウィンドウサイズを1440x810に変更しました。"); return; }
			if (Key.GetCount(Key.Num7) == 1) { SetWindowSize(1600, 900); PrintMessage("ウィンドウサイズを1600x900に変更しました。"); return; }
			if (Key.GetCount(Key.Num8) == 1) { SetWindowSize(1760, 990); PrintMessage("ウィンドウサイズを1760x990に変更しました。"); return; }
			if (Key.GetCount(Key.Num9) == 1) { SetWindowSize(1920, 1080); PrintMessage("ウィンドウサイズを1920x1080に変更しました。"); return; }
		}
		
		class CMouse
		{
			Image img;
			int nowx, nowy;
			int x, y;
			int count = 120;
			public CMouse()
			{
				img = new Image("mouse.png");
			}
			public void Draw()
			{
				GetMousePoint(out x, out y);
				if (count > 0)
				{
					count--;
					if (count == 0)
					{
						SetMouseDispFlag(FALSE);
					}
				}
				if (x != nowx || y != nowy)
				{
					count = 120;
					nowx = x;
					nowy = y;
					if (GetMouseDispFlag() == FALSE)
					{
						SetMouseDispFlag(TRUE);
					}
				}
			}
		}
		CMouse Mouse;        

        [Conditional("DEBUG")]
        void MakeLogFile()
        {
            SetOutApplicationLogValidFlag(0);
        }

        void BeforeDxInit()
        {
            ChangeWindowMode(TRUE);
            SetMainWindowText(exeTitle.ToString());
            SetGraphMode(1280, 720, 32);
            SetWindowSize(OldConfig.WindowSize.X, OldConfig.WindowSize.Y);
            SetAlwaysRunFlag(TRUE);
            SetWindowStyleMode(7);
            SetWindowSizeChangeEnableFlag(TRUE);
            SetAeroDisableFlag(TRUE);
        }
        void AfterDxInit()
        {
            SoundControl.Construct();
            CTone.Construct();
            SetSysCommandOffFlag(TRUE);
            ChangeFont("ＤＦＰ勘亭流");
            SetFontSize(30);
            ChangeFontType(DX_FONTTYPE_ANTIALIASING_EDGE_4X4);
            SetDrawScreen(DX_SCREEN_BACK);
            SetDrawMode(DX_DRAWMODE_BILINEAR);
        }

        MainControl()
        {
            PrintInit();
            PrintMessage("ソフトを起動しました。");
            SetOutApplicationLogValidFlag(FALSE);
            exeTitle = new StringBuilder();
            var info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            exeTitle.Append(info.ProductName);
            exeTitle.Append(" ver ");
            exeTitle.Append(info.ProductVersion);
            exeTitle.AddTitleForDebug();

            OldConfig = COldConfig.Construct(this);
            MainConfig = CMainConfig.Construct(this);

            SceneList = LSceneList.Construct(this);

            SongSelect = SSongSelect.Construct(this);
            Play = SPlay.Construct(this);
            Result = SResult.Construct(this);

            SceneList.Add(NScene.SongSelect, SongSelect);
            SceneList.Add(NScene.Play, Play);
            SceneList.Add(NScene.Result, Result);

            MakeLogFile();

            BeforeDxInit();

            if (DxLib_Init() == -1) { PrintMessage("DXライブラリの初期化に失敗しました。"); return; }

            AfterDxInit();

            Command = CCommand.Construct(this);
            Mouse = new CMouse();

#region 初期化
            Origin.Initialize();
            SceneList.Start();
#endregion
            while (ProcessMessage() == 0 && ClearDrawScreen() == 0)
            {
                Fps.Update();
                Key.Update();
                WindowSizeKeyUpdate();
                Command.Update();

                SceneList.Update();                
                SceneList.Draw();

                Command.Draw();
                Mouse.Draw();

                Fps.Draw();
                ScreenFlip();
                Fps.Wait();
            }
            DxLib_End();
        }
        static void Main(string[] args)
		{
			new MainControl();
		}
	}
}
