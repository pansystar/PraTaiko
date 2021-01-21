#define DEMOMUSIC
#define 文化祭ver
//#define OLD
//#define PLAYFIRST
//#define DEMOMUSIC_FROM_MEMORY
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DxLibDLL;
using System.Collections.ObjectModel;
using Pansystar;
using static Pansystar.Extensions;
using static DxLibDLL.DX;
using System.Collections;
using System.Xml.Linq;
using System.Globalization;
using DxLibForCS;

namespace PraTaiko
{
    class SSongSelect : BaseScene
    {
        class SelfDxImage : DxImage<SelfDxImage>, IGameAction
        {
            void IGameAction.Update()
            {
            }
            void IGameAction.Draw()
            {
                Draw();
            }
            void IGameAction.Finish()
            {
                Finish();
            }
        }
        class SelfDxSound : DxSound<SelfDxSound> { }

        public string ChartCurrentFilePath(ICommand c)
        {
            return ChartList.GetCurrent().FilePath;
        }
        public string GenreCurrentFilePath(ICommand c)
        {
            return GenreList.GetCurrent().Path;
        }

        public void SetDemoPlayMode(ICommand c, bool pm)
        {
            Demo.SetPlayMode(pm);
        }
        public void SetDemoPlayMode(ICommand c)
        {
            Demo.SetPlayMode();
        }

        static SSongSelect SongSelect;
        static CRelation Relation;
		static PDSongSelectPassData SongSelectPassData;

        static Action<IGameAction> AddAction;

		CGameActionControl GameActionControl;
		CSceneOut SceneOut;

		enum ENowInner { Song = 0, Course }
		ENowInner NowInner;
		
		class CEscExit
		{
			static CEscExit EscExit;
			bool exit = false;
			const int limit = 60;
			const int startFrame = 10;
			public void Update()
			{
				if (Key.GetCount(Key.Escape) == startFrame + limit)
				{
					exit = true;
				}
				if (Key.GetCount(Key.Escape) == 0 && exit)
				{
					PrintMessage("Escapeによる終了処理を行いました。");

					Environment.Exit(0);
				}
			}
			public void Draw()
			{
				if (exit)
				{
					DrawBox(0, 10, 410, 60, GetColor(255, 255, 255), FALSE);
					SetDrawBlendMode(DX_BLENDMODE_ALPHA, 191);
					DrawBox(0, 10, 410, 60, GetColor(255, 255, 255), TRUE);
					SetDrawBlendMode(DX_BLENDMODE_ALPHA, 255);
					DrawString(20, 20, "ESCを離すと終了します。", GetColor(255, 215, 0), GetColor(0, 0, 0));
				}
				else if (Key.GetCount(Key.Escape) > startFrame)
				{
					double per = ((double)(Key.GetCount(Key.Escape) - startFrame) / limit);
					DrawBox(0, 10, 410, 60, GetColor(255, 255, 255), FALSE);
					SetDrawBlendMode(DX_BLENDMODE_ALPHA, 191);
					DrawBox(0, 10, (int)(410 * per), 60, GetColor(255, 255, 255), TRUE);
					SetDrawBlendMode(DX_BLENDMODE_ALPHA, 255);
                    DrawString(20, 20, "長押しで終了:" + (int)(per * 100) + "%", GetColor(255, 215, 0), GetColor(0, 0, 0));
				}
			}
			public static CEscExit Construct(SSongSelect s)
			{
				if (EscExit == null)
				{
					EscExit = new CEscExit();
				}
				return EscExit;
			}
			CEscExit() { }
		}
		CEscExit EscExit;

        class CConfig
        {
            static SSongSelect S;
            static CGameActionControl G;
            const string f = @"Config.xml";
            public void Start()
            {
                XElement element = null;
                XAttribute attribute;

                if (File.Exists(MainConfig.SongSelect + f))
                {
                    element = XElement.Load(MainConfig.SongSelect + f);
                }
                else
                {
                    ErrorBox("SongSelectのConfig.xmlを見つけることが出来ませんでした。\nファイルを作成してください。", MainConfig.SongSelect + f, MessageBoxButtons.OK, true);
                }

                foreach (var item in element.Elements("category"))
                {
                    attribute = item.Attribute("name");
                    switch (attribute.Value)
                    {
                        case "Demo":
                            CDemo.Set(item);
                            break;
                        case "Bg":
                            CBgImageControl.Set(item);
                            break;
                        case "GenreBg":
                            CGenreImageControl.Set(item);
                            break;
                        case "Level":
                            CLevelAtSongImageControl.Set(item);
                            break;
                        case "PlayConfig":
                            CPlayConfigImageControl.Set(item);
                            break;
                        case "Other":
                            COtherImageControl.Set(item);
                            break;
                    }
                }
            }
            public static CConfig Construct(SSongSelect s)
            {
                return new CConfig(s);
            }
            CConfig(SSongSelect s)
            {
                S = s;
                G = s.GameActionControl;
            }
        }
        CConfig Config;

        class CBgImageControl : IGameAction
        {
            static CBgImageControl BgImageControl;
            static CChartList ChartList;
            static CGenreList GenreList;
            SelfDxImage BgImage;

            const string _BG_FOLDER = @"folder";
            const string _BG_LOOP = @"loop";
            const string _BG_BG = @"Bg.png";

            int GenreIndex;
            int fade = 255;
            int fadeSpeed = 0;

            void SetGenreIndex(int index)
            {
                GenreIndex = index;
                if (GenreIndex == ChartList.GetCurrent().GenreIndex)
                {
                    fade = 255;
                }
                else
                {
                    fade = 0;
                }
            }

            public static void Set(XElement e)
            {
                if (BgImageControl != null)
                {
                    ErrorBox("BgImageを複数表示することは出来ません。\nファイルを訂正してください。", MainConfig.SongSelect + @"Config.xml", MessageBoxButtons.OK, MainConfig.SongSelect + @"Config.xml");
                }
                BgImageControl = new CBgImageControl(e);
            }

            void IGameAction.Update()
            {
            }

            void a()
            {
                SetDrawBlendMode(DX_BLENDMODE_ALPHA, fade);
                if (GenreIndex == ChartList.GetCurrent().GenreIndex)
                {
                    if (fade < 256)
                    {
                        fade += fadeSpeed;
                    }
                    else if (fade > 255)
                    {
                        fade = 255;
                    }
                }
                else
                {
                    if (fade > 0)
                    {
                        fade -= fadeSpeed;
                    }
                    else if (fade < 0)
                    {
                        fade = 0;
                    }
                }
                BgImage.DrawTile(GenreList.GetCurrent().Bg);
            }

            void IGameAction.Draw()
            {
                SetDrawMode(DX_DRAWMODE_BILINEAR);
                a();
            }

            void IGameAction.Finish()
            {
            }

            CBgImageControl(XElement e)
            {
                BgImage = new SelfDxImage();
                ChartList = SongSelect.ChartList;
                GenreList = SongSelect.GenreList;
                string Folder = "";

                if ((string)e.Attribute(_BG_FOLDER) != null)
                {
                    #region folder
                    Folder = e.Attribute(_BG_FOLDER).Value;
                    if (Folder != "")
                    {
                        switch (Folder.Replace('/', '\\').Last())
                        {
                            case '\\':
                                break;
                            default:
                                Folder += '\\';
                                break;
                        }
                    }
                    #endregion
                }
                BgImage.SetImage(MainConfig.SongSelect + Folder + _BG_BG);

                if ((string)e.Element(_BG_LOOP) != null)
                {
                    float msxf, msyf;
                    msxf = msyf = 0;
                    foreach(var attr in e.Element(_BG_LOOP).Attributes())
                    {
                        switch (attr.Name.LocalName)
                        {
                            case "x":
                                float.TryParse(attr.Value, out msxf);
                                break;
                            case "y":
                                float.TryParse(attr.Value, out msyf);
                                break;
                        }
                    }
                    BgImage.SetMoveSpeed(msxf, msyf);
                }
                AddAction(this);
            }
        }
        class CGenreImageControl : IGameAction
        {
            static CGenreImageControl GenreImageControl;
            static CChartControl ChartControl;
            static CChartList ChartList;
            static CGenreList GenreList;

            static int Count = 20;
            static void SetCount(int c)
            {
                Count = c;
            }
            static void UpdateCount()
            {
                Count++;
            }

            static int IntervalX = 20;

            static int LeftPieces = 1;
            static int RightPieces = 1;
            void SetOtherPieces(string str)
            {
                int l, r;
                string[] s = str.Split(',');
                if (!int.TryParse(s[0], out l))
                {
                }
                if (!int.TryParse(s[1], out r))
                {
                }
                LeftPieces = l;
                RightPieces = r;
            }

            public static int OtherWidth { get; private set; }
            public static int OtherHeight { get; private set; }

            static void SetOtherSize(string str)
            {
                int w, h;
                string[] s = str.Split(',');
                if (!int.TryParse(s[0], out w))
                {
                }
                if (!int.TryParse(s[1], out h))
                {
                }
                OtherWidth = w;
                OtherHeight = h;
            }

            public static int PieceWidth { get; private set; }
            public static int PieceHeight { get; private set; }

            public static int CenterMoveX { get; private set; } = 0;
            public static void SetCenterMoveX(int mx = 0)
            {
                CenterMoveX = mx;
            }
            public static void UpdateCenterMoveX(CCenter c)
            {
                if (CenterMoveX != 0)
                {
                    if (CenterMoveX > 0)
                    {
                        CenterMoveX -= (c.SongWidth / 2 + IntervalX) / 10;
                        if (CenterMoveX < 0)
                        {
                            CenterMoveX = 0;
                        }
                    }
                    else
                    {
                        CenterMoveX += (c.SongWidth / 2 + IntervalX) / 10;
                        if (CenterMoveX > 0)
                        {
                            CenterMoveX = 0;
                        }
                    }
                }

            }

            public static int OtherMoveX { get; private set; }
            public static void SetOtherMoveX(int mx = 0)
            {
                OtherMoveX = mx;
            }
            public static void UpdateOtherMoveX()
            {
                if (OtherMoveX != 0)
                {
                    if (OtherMoveX > 0)
                    {
                        OtherMoveX -= (OtherWidth + IntervalX) / 10;
                        if (OtherMoveX < 0)
                        {
                            OtherMoveX = 0;
                        }
                    }
                    else
                    {
                        OtherMoveX += (OtherWidth + IntervalX) / 10;
                        if (OtherMoveX > 0)
                        {
                            OtherMoveX = 0;
                        }
                    }
                }

            }

            public class CCenter
            {
                public int[] Handles { get; private set; } = new int[9];

                public int SongX { get; private set; }
                public int SongY { get; private set; }

                public int SongWidth { get; private set; }
                public int SongHeight { get; private set; }

                public int CourseY { get; private set; }

                public int CourseWidth { get; private set; }
                public int CourseHeight { get; private set; }

                public static float diffPointX, diffPointY;
                public int diffPointXUpdate(ENowInner n, int s)
                {
                    switch (n)
                    {
                        case ENowInner.Song:
                            if (diffPointX > 0)
                            {
                                diffPointX -= s;
                                if (diffPointX < 0)
                                {
                                    diffPointX = 0;
                                }
                            }
                            if (diffPointX < 0)
                            {
                                diffPointX += s;
                                if (diffPointX > 0)
                                {
                                    diffPointX = 0;
                                }
                            }
                            //diffPointX = 0;
                            break;
#if !文化祭ver
                            case Program.SongSelect.NowSelect.course:
                                if (diffPointX < courseSizeX - songSizeX)
                                {
                                    diffPointX += s;
                                    if (diffPointX > courseSizeX - songSizeX)
                                    {
                                        diffPointX = courseSizeX - songSizeX;
                                    }
                                }
                                break;
#endif
                    }
                    return (int)diffPointX;
                }
                public int diffPointYUpdate(ENowInner n, int s)
                {
                    switch (n)
                    {
                        case ENowInner.Song:
                            if (diffPointY > 0)
                            {
                                diffPointY -= s;
                                if (diffPointY < 0)
                                {
                                    diffPointY = 0;
                                }
                            }
                            diffPointY = 0;
                            break;
#if !文化祭ver
                            case Program.SongSelect.NowSelect.course:
                                if (diffPointX == courseSizeX - songSizeX)
                                {
                                    if (diffPointY < courseSizeY - songSizeY)
                                    {
                                        diffPointY += s;
                                        if (diffPointY > courseSizeY - songSizeY)
                                        {
                                            diffPointY = courseSizeY - songSizeY;
                                        }
                                    }
                                }
                                break;

#endif
                    }
                    return (int)diffPointY;
                }
                public void SetImage(string str)
                {
                    var Image = new CImage();
                    Image.SetImage(str);
                    PieceWidth = Image.Width / 3;
                    PieceHeight = Image.Height / 3;
                    LoadDivGraph(str, 9, 3, 3, PieceWidth, PieceHeight, out Handles[0]);
                }
                public void SetSongPoint(string str)
                {
                    string[] s = str.Split(',');
                    foreach (var item in s.Select((value, index) => new { value, index }))
                    {
                        int x, y;
                        switch (item.index)
                        {
                            case 0:
                                #region X座標
                                if (!int.TryParse(item.value, out x))
                                {
                                    switch (item.value)
                                    {
                                        case "left":
                                            x = 0;
                                            break;
                                        case "right":
                                            x = MainConfig.DrawWidth - SongWidth;
                                            break;
                                        case "center":
                                            x = MainConfig.DrawWidth / 2 - SongWidth / 2;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                SongX = x;
                                #endregion
                                break;
                            case 1:
                                #region Y座標
                                if (!int.TryParse(item.value, out y))
                                {
                                    switch (item.value)
                                    {
                                        case "top":
                                            y = 0;
                                            break;
                                        case "buttom":
                                            y = MainConfig.DrawHeight - SongHeight;
                                            break;
                                        case "center":
                                            y = MainConfig.DrawHeight / 2 - SongHeight / 2;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                SongY = y;
                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
                public void SetSongSize(string str)
                {
                    int w, h;
                    string[] s = str.Split(',');
                    if (!int.TryParse(s[0], out w))
                    {
                    }
                    if (!int.TryParse(s[1], out h))
                    {
                    }
                    SongWidth = w;
                    SongHeight = h;
                }

                public void Draw()
                {
                    var c = ChartList.GetCurrent();
                    var g = GenreList.GetCurrent();

                    if (SongSelect.NowInner == ENowInner.Song)
                    {
                        diffPointXUpdate(SongSelect.NowInner, 48);
                    }
                    else
                    {
                        //diffPointXUpdate(Program.songSelect.nowSelect, 21);
                    }
                    diffPointYUpdate(SongSelect.NowInner, 5);

                    SetDrawMode(DX_DRAWMODE_NEAREST);

                    if (Count < 15)
                    {
                        diffPointX = 80 - SongWidth;
                        UpdateCount();

                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + CenterMoveX, SongY - (int)diffPointY, g.GenreBg.Handles[0], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth + CenterMoveX, SongY - (int)diffPointY + 0, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 - PieceWidth + CenterMoveX, SongY - (int)diffPointY + PieceHeight, g.GenreBg.Handles[1], 1);
                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + (SongWidth + (int)diffPointX) - PieceWidth + CenterMoveX, SongY - (int)diffPointY, g.GenreBg.Handles[2], 1);

                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + CenterMoveX, SongY - (int)diffPointY + PieceHeight, MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, g.GenreBg.Handles[3], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth + CenterMoveX, SongY - (int)diffPointY + PieceHeight, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 - PieceWidth + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, g.GenreBg.Handles[4], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + (SongWidth + (int)diffPointX) - PieceWidth + CenterMoveX, SongY - (int)diffPointY + PieceHeight, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, g.GenreBg.Handles[5], 1);

                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, g.GenreBg.Handles[6], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 - PieceWidth + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY), g.GenreBg.Handles[7], 1);
                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + (SongWidth + (int)diffPointX) - PieceWidth + CenterMoveX, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, g.GenreBg.Handles[8], 1);

                        SetDrawMode(DX_DRAWMODE_BILINEAR);
                        Origin.DrawExtendVStringToHandle(MainConfig.DrawCenterX - 20 + 160 + (int)(diffPointX * 0.935) / 2 + CenterMoveX, 135 + -(int)(diffPointY * 0.8), 1.0, c.ExRateY, c.Title, DX.GetColor(255, 255, 255), c.FontHandle, g.EdgeColor);

                        //foreach (var item in genre.bg.bgPlus)
                        //{
                        //    item.Draw(Program.config.windowSize.x / 2 - (SongWidth + (int)diffPointX) / 2 + CenterMoveX, SongHeight);
                        //}
                    }
                    else
                    {
                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2, SongY - (int)diffPointY, Handles[0], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth, SongY - (int)diffPointY + 0, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 - PieceWidth, SongY - (int)diffPointY + PieceHeight, Handles[1], 1);
                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + (SongWidth + (int)diffPointX) - PieceWidth, SongY - (int)diffPointY, Handles[2], 1);

                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2, SongY - (int)diffPointY + PieceHeight, MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, Handles[3], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth, SongY - (int)diffPointY + PieceHeight, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 - PieceWidth, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, Handles[4], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + (SongWidth + (int)diffPointX) - PieceWidth, SongY - (int)diffPointY + PieceHeight, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, Handles[5], 1);

                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, Handles[6], 1);
                        DrawExtendGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + PieceWidth, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, MainConfig.DrawCenterX + (SongWidth + (int)diffPointX) / 2 - PieceWidth, SongY - (int)diffPointY + (SongHeight + (int)diffPointY), Handles[7], 1);
                        DrawGraph(MainConfig.DrawCenterX - (SongWidth + (int)diffPointX) / 2 + (SongWidth + (int)diffPointX) - PieceWidth, SongY - (int)diffPointY + (SongHeight + (int)diffPointY) - PieceHeight, Handles[8], 1);

                        SetDrawMode(DX_DRAWMODE_BILINEAR);
                        Origin.DrawExtendVStringToHandle(MainConfig.DrawCenterX - 20 + 160 + (int)(diffPointX * 0.935) / 2, 135 + -(int)(diffPointY * 0.8), 1.0, c.ExRateY, c.Title, GetColor(255, 255, 255), c.FontHandle, DX.GetColor(0, 0, 0));
                    }
                }
                public CCenter()
                {
                }
            }
            static CCenter Center;

            public class CLeft
            {
                public int SongX { get; private set; }
                public int SongY { get; private set; }

                public int SongWidth { get; private set; }
                public int SongHeight { get; private set; }

                public void SetSongSize()
                {
                    SongWidth = OtherWidth;
                    SongHeight = OtherHeight;
                }
                public void SetSongPoint(int index)
                {
                    SongX = Center.SongX + (SongWidth * -(index + 1)) - (IntervalX * (index + 1));
                    SongY = Center.SongY;
                }
                public void Draw(int index)
                {
                    var c = ChartList[ChartControl.ChartLefts[index]];
                    var g = GenreList[c.GenreIndex];

                    if (SongSelect.NowInner == ENowInner.Course)
                    {
                        DX.SetDrawBright(128, 128, 128);
                    }
                    SetDrawMode(DX_DRAWMODE_NEAREST);

                    if (index == 0 && OtherMoveX > 0)
                    {
                        DX.DrawGraph(SongX + CenterMoveX, SongY, g.GenreBg.Handles[0], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + CenterMoveX, SongY, SongX + SongWidth - PieceWidth + CenterMoveX, SongY, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + PieceHeight, g.GenreBg.Handles[1], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + CenterMoveX, SongY, g.GenreBg.Handles[2], 1);

                        DX.DrawModiGraph(SongX + CenterMoveX, SongY + PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[3], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[4], 1);
                        DX.DrawModiGraph(SongX + SongWidth - PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[5], 1);

                        DX.DrawGraph(SongX + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[6], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight, SongX + PieceWidth + CenterMoveX, SongY + SongHeight, g.GenreBg.Handles[7], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[8], 1);

                        SetDrawMode(DX_DRAWMODE_BILINEAR);
                        Origin.DrawExtendVStringToHandle(SongX + SongWidth / 2 + CenterMoveX, SongY + 28, 1.0, c.ExRateY, c.Title, DX.GetColor(255, 255, 255), c.FontHandle, g.EdgeColor);

                    }
                    else
                    {
                        DX.DrawGraph(SongX + OtherMoveX, SongY, g.GenreBg.Handles[0], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + OtherMoveX, SongY, SongX + SongWidth - PieceWidth + OtherMoveX, SongY, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + PieceHeight, g.GenreBg.Handles[1], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + OtherMoveX, SongY, g.GenreBg.Handles[2], 1);

                        DX.DrawModiGraph(SongX + OtherMoveX, SongY + PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[3], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[4], 1);
                        DX.DrawModiGraph(SongX + SongWidth - PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[5], 1);

                        DX.DrawGraph(SongX + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[6], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight, SongX + PieceWidth + OtherMoveX, SongY + SongHeight, g.GenreBg.Handles[7], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[8], 1);

                        SetDrawMode(DX_DRAWMODE_BILINEAR);
                        Origin.DrawExtendVStringToHandle(SongX + SongWidth / 2 + OtherMoveX, SongY + 28, 1.0, c.ExRateY, c.Title, DX.GetColor(255, 255, 255), c.FontHandle, g.EdgeColor);

                    }
                    SetDrawBright(255, 255, 255);
                }
            }
            static List<CLeft> Lefts;

            public class CRight
            {
                public int SongX { get; private set; }
                public int SongY { get; private set; }

                public int SongWidth { get; private set; }
                public int SongHeight { get; private set; }

                public void SetSongSize()
                {
                    SongWidth = OtherWidth;
                    SongHeight = OtherHeight;
                }
                public void SetSongPoint(int index)
                {
                    SongX = Center.SongX + Center.SongWidth + (SongWidth * index) + (IntervalX * (index + 1));
                    SongY = Center.SongY;
                }
                public void Draw(int index)
                {
                    var c = ChartList[ChartControl.ChartRights[index]];
                    var g = GenreList[c.GenreIndex];

                    if (SongSelect.NowInner == ENowInner.Course)
                    {
                        DX.SetDrawBright(128, 128, 128);
                    }
                    SetDrawMode(DX_DRAWMODE_NEAREST);

                    if (index == 0 && OtherMoveX < 0)
                    {
                        DX.DrawGraph(SongX + CenterMoveX, SongY, g.GenreBg.Handles[0], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + CenterMoveX, SongY, SongX + SongWidth - PieceWidth + CenterMoveX, SongY, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + PieceHeight, g.GenreBg.Handles[1], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + CenterMoveX, SongY, g.GenreBg.Handles[2], 1);

                        DX.DrawModiGraph(SongX + CenterMoveX, SongY + PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[3], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[4], 1);
                        DX.DrawModiGraph(SongX + SongWidth - PieceWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth + CenterMoveX, SongY + PieceHeight, SongX + SongWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[5], 1);

                        DX.DrawGraph(SongX + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[6], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight, SongX + PieceWidth + CenterMoveX, SongY + SongHeight, g.GenreBg.Handles[7], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + CenterMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[8], 1);
                        SetDrawMode(DX_DRAWMODE_BILINEAR);
                        Origin.DrawExtendVStringToHandle(SongX + SongWidth / 2 + CenterMoveX, SongY + 28, 1.0,c.ExRateY,c.Title, DX.GetColor(255, 255, 255), c.FontHandle, g.EdgeColor);

                    }
                    else
                    {
                        DX.DrawGraph(SongX + OtherMoveX, SongY, g.GenreBg.Handles[0], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + OtherMoveX, SongY, SongX + SongWidth - PieceWidth + OtherMoveX, SongY, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + PieceHeight, g.GenreBg.Handles[1], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + OtherMoveX, SongY, g.GenreBg.Handles[2], 1);

                        DX.DrawModiGraph(SongX + OtherMoveX, SongY + PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[3], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[4], 1);
                        DX.DrawModiGraph(SongX + SongWidth - PieceWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth + OtherMoveX, SongY + PieceHeight, SongX + SongWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[5], 1);

                        DX.DrawGraph(SongX + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[6], 1);
                        DX.DrawModiGraph(SongX + PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight, SongX + PieceWidth + OtherMoveX, SongY + SongHeight, g.GenreBg.Handles[7], 1);
                        DX.DrawGraph(SongX + SongWidth - PieceWidth + OtherMoveX, SongY + SongHeight - PieceHeight, g.GenreBg.Handles[8], 1);
                        SetDrawMode(DX_DRAWMODE_BILINEAR);
                        Origin.DrawExtendVStringToHandle(SongX + SongWidth / 2 + OtherMoveX, SongY + 28, 1.0,c.ExRateY,c.Title, DX.GetColor(255, 255, 255), c.FontHandle, g.EdgeColor);

                    }
                    SetDrawBright(255, 255, 255);
                }
            }
            static List<CRight> Rights;

            public static void Set(XElement e)
            {
                if (GenreImageControl != null)
                {
                    ErrorBox("BgImageを複数表示することは出来ません。\nファイルを訂正してください。", MainConfig.SongSelect + @"Config.xml", MessageBoxButtons.OK, MainConfig.SongSelect + @"Config.xml");
                }
                GenreImageControl = new CGenreImageControl(e);
            }
            void UpdateNowInnerSong()
            {
                if (ChartControl.KeyLeft)
                {
                    SetCount(0);
                    SetCenterMoveX(-(Center.SongWidth / 2 + IntervalX));
                    SetOtherMoveX(-(OtherWidth + IntervalX));
                }
                if (ChartControl.KeyRight)
                {
                    SetCount(0);
                    SetCenterMoveX((Center.SongWidth / 2 + IntervalX));
                    SetOtherMoveX(OtherWidth + IntervalX);
                }
            } 
            void IGameAction.Update()
            {
                UpdateOtherMoveX();
                UpdateCenterMoveX(Center);

                switch (SongSelect.NowInner)
                {
                    case ENowInner.Song:
                        UpdateNowInnerSong();
                        break;
                }
            }

            void IGameAction.Draw()
            {
                Center.Draw();
                for (int i = 0; i < Lefts.Count; i++)
                {
                    Lefts[i].Draw(i);
                }
                for (int i = 0; i < Rights.Count; i++)
                {
                    Rights[i].Draw(i);
                }
            }
            void IGameAction.Finish()
            {
            }

            CGenreImageControl(XElement e)
            {
                Center = new CCenter();
                Lefts = new List<CLeft>();
                Rights = new List<CRight>();

                foreach (var element in e.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "center":
                            foreach(var center in element.Elements())
                            {
                                switch (center.Name.LocalName)
                                {
                                    case "image":
                                        Center.SetImage(MainConfig.SongSelect + center.Value);
                                        break;
                                    case "size":
                                        Center.SetSongSize(center.Value);
                                        break;
                                    case "point":
                                        Center.SetSongPoint(center.Value);
                                        break;
                                }
                            }
                            break;
                        case "other":
                            if ((string)element.Element("pieces") != null)
                            {
                                SetOtherPieces(element.Element("pieces").Value);
                            }
                            if ((string)element.Element("size") != null)
                            {
                                SetOtherSize(element.Element("size").Value);
                            }
                            for (int i = 0; i < LeftPieces; i++)
                            {
                                Lefts.Add(new CLeft());
                                Lefts[i].SetSongSize();
                                Lefts[i].SetSongPoint(i);
                            }
                            for (int i = 0; i < RightPieces; i++)
                            {
                                Rights.Add(new CRight());
                                Rights[i].SetSongSize();
                                Rights[i].SetSongPoint(i);
                            }
                            break;
                    }
                }
                ChartList = SongSelect.ChartList;
                GenreList = SongSelect.GenreList;
                ChartControl = SongSelect.ChartControl;

                AddAction(this);
            }
        }
        class CLevelAtSongImageControl : IGameAction
        {
            const int interval = 15;
            static CLevelAtSongImageControl LevelAtSongImageControl;
            static CChartList ChartList;

            public CImage Base;
            //public CImage EasyBase;
            //public CImage NormalBase;
            //public CImage HardBase;
            //public CImage OniBase;
            public CImage UraBase;

            public CImage Easy;
            public CImage Normal;
            public CImage Hard;
            public CImage Oni;
            public CImage Ura;

            public CImage Star;
            public CImage p1;

            public static void Set(XElement e)
            {
                if (LevelAtSongImageControl != null)
                {
                    ErrorBox("BgImageを複数表示することは出来ません。\nファイルを訂正してください。", MainConfig.SongSelect + @"Config.xml", MessageBoxButtons.OK, MainConfig.SongSelect + @"Config.xml");
                }
                LevelAtSongImageControl = new CLevelAtSongImageControl(e);
            }
            void IGameAction.Update()
            {
            }
            void IGameAction.Draw()
            {
                CChart Chart = ChartList.GetCurrent();
                int CourseCurrent = SongSelect.ChartControl.CourseCurrent;
                switch (SongSelect.NowInner)
                {
                    case ENowInner.Song:
                    case ENowInner.Course:
                        if (CGenreImageControl.CCenter.diffPointX >= 0)
                        {
                            Base.Draw();
                            Easy.DrawAPoint(Base.X + 10, Base.Y + 10);

                            Base.DrawAPoint(Base.X + (Base.Width + interval) * 1, Base.Y);
                            Normal.DrawAPoint(Base.X + (Base.Width + interval) * 1 + 10, Base.Y + 10);

                            Base.DrawAPoint(Base.X + (Base.Width + interval) * 2, Base.Y);
                            Hard.DrawAPoint(Base.X + (Base.Width + interval) * 2 + 10, Base.Y + 10);

                            Base.DrawAPoint(Base.X + (Base.Width + interval) * 3, Base.Y);
                            Oni.DrawAPoint(Base.X + (Base.Width + interval) * 3 + 10, Base.Y + 10);
                            
                            foreach (var c in Chart.Courses)
                            {
                                switch (c.courseID)
                                {
                                    case ECourseID.Easy:
                                        for (int j = 0; j < c.Level; j++)
                                        {
                                            Star.DrawAPoint(487, 505 + j * -17);
                                        }
                                        break;
                                    case ECourseID.Normal:
                                        for (int j = 0; j < c.Level; j++)
                                        {
                                            Star.DrawAPoint(487 + (Base.Width + interval) * 1, 505 + j * -17);
                                        }
                                        break;
                                    case ECourseID.Hard:
                                        for (int j = 0; j < c.Level; j++)
                                        {
                                            Star.DrawAPoint(487 + (Base.Width + interval) * 2, 505 + j * -17);
                                        }
                                        break;
                                    case ECourseID.Oni:
                                        for (int j = 0; j < c.Level; j++)
                                        {
                                            Star.DrawAPoint(487 + (Base.Width + interval) * 3, 505 + j * -17);
                                        }
                                        break;
                                    case ECourseID.Ura:
                                        if (SongSelect.NowInner == ENowInner.Course)
                                        {
                                            UraBase.DrawAPoint(Base.X + (Base.Width + interval) * 4, Base.Y);
                                            Ura.DrawAPoint(Base.X + (Base.Width + interval) * 4 + 10, Base.Y + 10);
                                            for (int j = 0; j < c.Level; j++)
                                            {
                                                Star.DrawAPoint(487 + (Base.Width + interval) * 4, 505 + j * -17);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                }
                switch (SongSelect.NowInner)
                {
                    case ENowInner.Course:
                        switch (Chart.Courses[CourseCurrent].courseID)
                        {
                            case ECourseID.Easy:
                                p1.DrawAPoint(469, 206);
                                break;
                            case ECourseID.Normal:
                                p1.DrawAPoint(469 + (Base.Width + interval) * 1, 206);
                                break;
                            case ECourseID.Hard:
                                p1.DrawAPoint(469 + (Base.Width + interval) * 2, 206);
                                break;
                            case ECourseID.Oni:
                                p1.DrawAPoint(469 + (Base.Width + interval) * 3, 206);
                                break;
                            case ECourseID.Ura:
                                p1.DrawAPoint(469 + (Base.Width + interval) * 4, 206);
                                break;
                        }
                        break;

                }
            }

            void IGameAction.Finish()
            {
            }

            CLevelAtSongImageControl(XElement e)
            {
                ChartList = SongSelect.ChartList;
                Base = new CImage(MainConfig.SongSelect +@"Level\" + "level_song_bg.png");
                Base.SetPoint("475,250");

                UraBase = new CImage(MainConfig.SongSelect + @"Level\" + "level_song_bg_ura.png");

                Easy = new CImage(MainConfig.SongSelect + @"Level\" + "course_easy.png");
                Normal = new CImage(MainConfig.SongSelect + @"Level\" + "course_normal.png");
                Hard = new CImage(MainConfig.SongSelect + @"Level\" + "course_hard.png");
                Oni = new CImage(MainConfig.SongSelect + @"Level\" + "course_oni.png");
                Ura = new CImage(MainConfig.SongSelect + @"Level\" + "course_ura.png");
                Star = new CImage(MainConfig.SongSelect + @"Level\" + "star.png");
                p1 = new CImage(MainConfig.SongSelect + @"Level\" + "p1.png");
                AddAction(this);
            }
        }
        class CPlayConfigImageControl:IGameAction
        {
            const string _FOLDER = @"folder";
            const string _POINT1P = @"point1p";
            const string _INTERVAL = @"interval";

            static CPlayConfigImageControl Own;

            int IntervalX = 0;

            SelfDxImage Base;

            SelfDxImage Auto;
            SelfDxImage X2;
            SelfDxImage X3;
            SelfDxImage X4;
            SelfDxImage XOutSize;

            SelfDxImage Turn;
            SelfDxImage Shuffle_;
            SelfDxImage Shuffle1;
            SelfDxImage Shuffle2;

            SelfDxImage NoteOff;

            int AddX(int w)
            {
                return w + IntervalX;
            }
            void IGameAction.Update()
            {
            }
            void IGameAction.Draw()
            {
                int x = (int)Base.NowLeftX;
                int y = (int)Base.NowTopY;

                if (PlayConfig.Auto)
                {
                    Auto.Draw(x, y); x += AddX(Auto.Width);
                }
                if (PlayConfig.Speed % 1 == 0)
                {
                    switch ((int)PlayConfig.Speed)
                    {
                        case 1:
                            break;
                        case 2:
                            X2.Draw(x, y); x += AddX(X2.Width);
                            break;
                        case 3:
                            X3.Draw(x, y); x += AddX(X3.Width);
                            break;
                        case 4:
                            X4.Draw(x, y); x += AddX(X4.Width);
                            break;
                        default:
                            XOutSize.Draw(x, y); x += AddX(XOutSize.Width);
                            break;
                    }
                }
                else
                {
                    XOutSize.Draw(x, y); x += AddX(XOutSize.Width);
                }
                switch (PlayConfig.ReplacePer)
                {
                    case 0:
                        break;
                    case 20:
                        Shuffle1.Draw(x, y); x += AddX(Shuffle1.Width);
                        break;
                    case 50:
                        Shuffle2.Draw(x, y); x += AddX(Shuffle2.Width);
                        break;
                    case 100:
                        Turn.Draw(x, y); x += AddX(Turn.Width);
                        break;
                    default:
                        Shuffle_.Draw(x, y); x += AddX(Shuffle_.Width);
                        break;
                }
                if (PlayConfig.NoteOff)
                {
                    NoteOff.Draw(x, y); x += AddX(NoteOff.Width);
                }
            }
            public static void Set(XElement e)
            {
                if (Own != null)
                {
                    ErrorBox("BgImageを複数表示することは出来ません。\nファイルを訂正してください。", MainConfig.SongSelect + @"Config.xml", MessageBoxButtons.OK, MainConfig.SongSelect + @"Config.xml");
                }
                Own = new CPlayConfigImageControl(e);
            }

            void IGameAction.Finish()
            {
            }

            CPlayConfigImageControl(XElement e)
            {
                Base = new SelfDxImage();

                Auto = new SelfDxImage();
                X2 = new SelfDxImage();
                X3 = new SelfDxImage();
                X4 = new SelfDxImage();
                XOutSize = new SelfDxImage();
                Turn = new SelfDxImage();
                Shuffle_ = new SelfDxImage();
                Shuffle1 = new SelfDxImage();
                Shuffle2 = new SelfDxImage();
                NoteOff = new SelfDxImage();

                string Folder = "";

                if ((string)e.Attribute(_FOLDER) != null)
                {
                    #region folder
                    Folder = e.Attribute(_FOLDER).Value;
                    if (Folder != "")
                    {
                        switch (Folder.Replace('/', '\\').Last())
                        {
                            case '\\':
                                break;
                            default:
                                Folder += '\\';
                                break;
                        }
                    }
                    #endregion
                }
                if ((string)e.Element(_POINT1P) != null)
                {
                    #region point
                    string xs = "", ys = "";
                    foreach (var attr in e.Element(_POINT1P).Attributes())
                    {
                        switch (attr.Name.LocalName)
                        {
                            case "x":
                                xs = attr.Value;
                                break;
                            case "y":
                                ys = attr.Value;
                                break;
                        }
                    }
                    Base.SetPoint(xs, ys);
                    #endregion
                }
                if ((string)e.Element(_INTERVAL) != null)
                {
                    #region interval
                    foreach (var attr in e.Element(_INTERVAL).Attributes())
                    {
                        int xs = 0;
                        switch (attr.Name.LocalName)
                        {
                            case "x":
                                int.TryParse(attr.Value, out xs);
                                IntervalX = xs;
                                break;
                        }
                    }
                    #endregion
                }

                Auto.SetImage(MainConfig.SongSelect + Folder + @"Auto.png");
                X2.SetImage(MainConfig.SongSelect + Folder + @"x2.png");
                X3.SetImage(MainConfig.SongSelect + Folder + @"x3.png");
                X4.SetImage(MainConfig.SongSelect + Folder + @"x4.png");
                XOutSize.SetImage(MainConfig.SongSelect + Folder + @"xOutSize.png");
                Shuffle_.SetImage(MainConfig.SongSelect + Folder + @"Shuffle.png");
                Shuffle1.SetImage(MainConfig.SongSelect + Folder + @"Shuffle1.png");
                Shuffle2.SetImage(MainConfig.SongSelect + Folder + @"Shuffle2.png");
                Turn.SetImage(MainConfig.SongSelect + Folder + @"Turn.png");
                NoteOff.SetImage(MainConfig.SongSelect + Folder + @"NoteOff.png");

                AddAction(this);
            }
        }
        class COtherImageControl:IDisposable
        {
            const string _OTHER_FOLDER = @"folder";
            const string _OTHER_FILE = @"file";
            const string _OTHER_POINT = @"point";

            static COtherImageControl OtherImageControl;
            List<SelfDxImage> OtherImages = new List<SelfDxImage>();
            void Add(XElement e)
            {
                int StartCount = OtherImages.Count;
                string Folder = "";
                if ((string)e.Attribute(_OTHER_FOLDER) != null)
                {
                    #region folder
                    Folder = e.Attribute(_OTHER_FOLDER).Value;
                    if (Folder != "")
                    {
                        switch (Folder.Replace('/', '\\').Last())
                        {
                            case '\\':
                                break;
                            default:
                                Folder += '\\';
                                break;
                        }
                    }
                    #endregion
                }
                foreach (var element in e.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "image":
                            #region image
                            var image = new SelfDxImage();
                            if ((string)element.Attribute(_OTHER_FILE) != null)
                            {
                                image.SetImage(MainConfig.SongSelect + Folder + element.Attribute(_OTHER_FILE).Value);
                                if ((string)element.Element(_OTHER_POINT) != null)
                                {
                                    string xs = "", ys = "";
                                    foreach(var attr in element.Element(_OTHER_POINT).Attributes())
                                    {
                                        switch (attr.Name.LocalName)
                                        {
                                            case "x":
                                                xs = attr.Value;
                                                break;
                                            case "y":
                                                ys = attr.Value;
                                                break;
                                        }
                                    }
                                    image.SetPoint(xs, ys);
                                }
                                OtherImages.Add(image);
                            }
                            #endregion
                            break;
                    }
                }
                for (int i = StartCount; i < OtherImages.Count; i++) { AddAction(OtherImages[i]); }
            }
            public static void Set(XElement e)
            {
                if(OtherImageControl == null)
                {
                    OtherImageControl = new COtherImageControl();
                }
                OtherImageControl.Add(e);
            }
            public void Dispose()
            {
                OtherImages.Clear();
            }
            COtherImageControl()
            {
            }
        }

        CTone Tone;

        class CDemo : IRelation
        {
            static CChartList ChartList;
            static CDemo Demo;
            public static void Set(XElement e)
            {
                foreach(var element in e.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "play":
                            foreach(var attr in element.Attributes())
                            {
                                switch (attr.Name.LocalName)
                                {
                                    case "mode":
                                        switch (attr.Value)
                                        {
                                            case "0":
                                                Demo.PlayMode = false;
                                                break;
                                            case "1":
                                                Demo.PlayMode = true;
                                                break;
                                        }
                                        break;
                                    case "timing":
                                        int t;
                                        if(int.TryParse(attr.Value, out t))
                                        {
                                            Demo.StartTiming = t;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            bool PlayMode = false;

            const int INIT_TIMING = 0;
            int StartTiming = 20;

            int Count = INIT_TIMING;

            int Handle = -1;

            public void SetPlayMode(bool pm)
            {
                PlayMode = pm;
                if (!PlayMode)
                {
                    StopDemo();
                }
            }
            public void SetPlayMode()
            {
                PlayMode = !PlayMode;
                if (!PlayMode)
                {
                    StopDemo();
                }
            }

            public void StopDemo()
            {
                StopSoundMem(Handle);
            }

            public bool CheckMemAndASync()
            {
                return CheckSoundMem(Handle) == -1 && CheckHandleASyncLoad(Handle) == -1;
            }

            public int CheckDemo()
            {
                return CheckSoundMem(Handle);
            }
            public int CheckASyncDemo()
            {
                return CheckHandleASyncLoad(Handle);
            }

            public void SetTime(int t)
            {
                SetSoundCurrentTime(t, Handle);
            }
            public void SetDemo(string str)
            {
                SetUseASyncLoadFlag(TRUE);
                SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMPRESS);
                Handle = LoadSoundMem(str);
                SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMNOPRESS);
                SetUseASyncLoadFlag(FALSE);
            }
            void IRelation.Link()
            {
                ChartList = SongSelect.ChartList;
            }
            public void Reset()
            {
                Delete();
                Count = INIT_TIMING;
            }
            public void Delete()
            {
                DeleteSoundMem(Handle);
            }
            public void AddTiming()
            {
                if (Count < StartTiming)
                {
                    Count++;
                }
            }
            public void Update()
            {
                if (Demo.PlayMode)
                {
                    if (CheckMemAndASync())
                    {
                        SetUseASyncLoadFlag(TRUE);
                        SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMPRESS);
                        SetDemo(ChartList.GetCurrent().Wave);
                        SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMNOPRESS);
                        SetUseASyncLoadFlag(FALSE);
                    }
                    if (CheckHandleASyncLoad(Handle) == FALSE && Count == StartTiming && NScene.Now == NScene.SongSelect)
                    {
                        if (CheckSoundMem(Handle) == 0)
                        {
                            SetSoundCurrentTime((int)(ChartList.GetCurrent().Demostart * 1000), Handle);
                            PlaySoundMem(Handle, DX_PLAYTYPE_BACK, 0);
                        }
                    }
                    AddTiming();
                }
            }
            public CDemo()
            {
                Demo = this;
                Relation.Add(this);
            }
        }
        CDemo Demo;

        class CChartControl : IRelation
        {
            static CChartControl ChartControl;

            static CChartList ChartList;
            static CGenreList GenreList;

            public int ChartCurrent { get; private set; }
            public int GenreCurrent { get; private set; }
            public int[] ChartLefts { get; private set; }
            public int[] ChartRights { get; private set; }

            public void FormatChart(int index)
            {
                ChartCurrent = index;
                GenreCurrent = SongSelect.ChartList[index].GenreIndex;
                for (int i = 0; i < ChartLefts.Length; i++)
                {
                    ChartLefts[i] = (ChartCurrent + (ChartList.Count - ((i + 1) % ChartList.Count))) % ChartList.Count;
                }
                for (int i = 0; i < ChartRights.Length; i++)
                {
                    ChartRights[i] = (ChartCurrent + (i + 1)) % ChartList.Count;
                }
            }

            public void MoveChartLeft()
            {
                FormatChart(ChartLefts[0]);
                PrintMessage("選曲中:" + ChartList.GetCurrent().Title);
            }
            public void MoveChartRight()
            {
                FormatChart(ChartRights[0]);
                PrintMessage("選曲中:" + ChartList.GetCurrent().Title);
            }
            public void MoveChartRandom()
            {
                int r = GetRand(ChartList.Count - 1);
                FormatChart(r);
                PrintMessage("選曲中:" + ChartList.GetCurrent().Title);
            }
            public void MoveChartUnitLeft()
            {
                switch (ChartList.SortList.GetCurrentName())
                {
                    case "Genre":
                        int i = (ChartList.GetCurrent().GenreIndex + (GenreList.Count - 1)) % GenreList.Count;
                        ChartControl.FormatChart(GenreList[i].PtsLastIndex);
                        break;
                    case "Title":
                        ChartControl.FormatChart((ChartCurrent + (ChartList.Count - 10)) % ChartList.Count);
                        break;
                }
            }
            public void MoveChartUnitRight()
            {
                switch (ChartList.SortList.GetCurrentName())
                {
                    case "Genre":
                        int i = (ChartList.GetCurrent().GenreIndex + 1) % GenreList.Count;
                        ChartControl.FormatChart(GenreList[i].PtsFirstIndex);
                        break;
                    case "Title":
                        ChartControl.FormatChart((ChartCurrent + 10) % ChartList.Count);
                        break;
                }
            }

            public int CourseCurrent { get; private set; }
            public void SetInitialCourse()
            {
                if (ChartList.GetCurrent().Courses.Count == 1)
                {
                    CourseCurrent = 0;
                    SongSelect.NowInner = ENowInner.Course;
                }
                else if (ChartList.GetCurrent().Courses.Count > 1)
                {
                    int courseBuf = ChartList.GetCurrent().Courses.FindIndex(a => a.courseID == ECourseID.Oni);
                    if (courseBuf > -1)
                    {
                        CourseCurrent = courseBuf;
                    }
                    else
                    {
                        if (ChartList.GetCurrent().Courses.Exists(a => a.courseID == ECourseID.Ura))
                        {
                            CourseCurrent = ChartList.GetCurrent().Courses.Count - 2;
                        }
                        else
                        {
                            CourseCurrent = ChartList.GetCurrent().Courses.Count - 1;
                        }
                    }
                    SongSelect.NowInner = ENowInner.Course;
                }
                else
                {
                    DialogResult dr = MessageBox.Show("ファイルパス:" + ChartList.GetCurrent().FilePath + "\n\n" + "この曲には譜面データが書かれていません。ファイルを開きますか？", "難易度エラー", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        Process.Start(MainConfig.TextEditor, ChartList.GetCurrent().FilePath);
                    }
                }

            }
            public void MoveCourseLeft()
            {
                CourseCurrent = (CourseCurrent + (ChartList.GetCurrent().Courses.Count - 1)) % ChartList.GetCurrent().Courses.Count;
            }
            public void MoveCourseRight()
            {
                CourseCurrent = (CourseCurrent + 1) % ChartList.GetCurrent().Courses.Count;
            }

            public bool KeyLeft
            {
                get
                {
                    return Key.GetCount(Key.D) == 1 || (Key.GetCount(Key.D) > 20 && Key.GetCount(Key.D) % 10 == 0) ||
                           Key.GetCount(Key.INPUT_LEFT) == 1 || (Key.GetCount(Key.INPUT_LEFT) > 20 && Key.GetCount(Key.INPUT_LEFT) % 10 == 0);
                }
            }
            public bool KeyRight
            {
                get
                {
                    return Key.GetCount(Key.K) == 1 || (Key.GetCount(Key.K) > 20 && Key.GetCount(Key.K) % 10 == 0) ||
                           Key.GetCount(Key.INPUT_RIGHT) == 1 || (Key.GetCount(Key.INPUT_RIGHT) > 20 && Key.GetCount(Key.INPUT_RIGHT) % 10 == 0);
                }
            }
            public bool KeyUnitLeft
            {
                get
                {
                    return Key.GetCount(Key.C) == 1;
                }
            }
            public bool KeyUnitRight
            {
                get
                {
                    return Key.GetCount(Key.M) == 1;
                }
            }
            public bool KeyRandom
            {
                get
                {
                    return Key.GetCount(Key.R) == 1;
                }
            }
            public bool KeyMoveChart
            {
                get
                {
                    return KeyLeft || KeyRight || KeyUnitLeft || KeyUnitRight || KeyRandom;
                }
            }
            public bool KeyEnter
            {
                get
                {
                    return Key.GetCount(Key.F) == 1 || Key.GetCount(Key.J) == 1 || Key.GetCount(Key.INPUT_SPACE) == 1;
                }
            }
            public bool KeyBack
            {
                get
                {
                    return Key.GetCount(Key.Escape) == 1 || Key.GetCount(Key.INPUT_Q) == 1 || Key.GetCount(Key.Back) == 1;
                }
            }

            public static CChartControl Construct(SSongSelect s)
            {
                if (ChartControl == null)
                {
                    ChartControl = new CChartControl();
                }
                return ChartControl;
            }

            void IRelation.Link()
            {
                ChartList = SongSelect.ChartList;
                GenreList = SongSelect.GenreList;
                FormatChart(0);
                PrintMessage("選曲中:" + ChartList.GetCurrent().Title);
            }
            CChartControl()
            {
                ChartLefts = new int[7];
                ChartRights = new int[7];
                Relation.Add(this);
            }
        }
		CChartControl ChartControl;

		class CGenre
		{
            const string GENRE_BG = @"GenreBg.png";
            const string BG = @"Bg.png";
			public int Index { get; private set; }
			public bool Open = false;
			public string Path { get; private set; }
			public string Name { get; private set; }
			public uint Color { get; private set; }
			public uint FontColor { get; private set; }
			public uint EdgeColor { get; private set; }
			public int Secret { get; private set; }            
			public class GenreImage : Image
			{
				public int[] Handles { get; private set; }
				public int pieceSizeX { get; private set; }
				public int pieceSizeY { get; private set; }
				public GenreImage(string fp) : base(fp)
				{
                    Handles = Enumerable.Repeat(-1, 9).ToArray();
					pieceSizeX = sizeX / 3;
					pieceSizeY = sizeY / 3;
					LoadDivGraph(fp, 9, 3, 3, pieceSizeX, pieceSizeY, out Handles[0]);
					DeleteGraph(handle);
				}
			}
			public GenreImage GenreBg { get; private set; }
            public SelfDxImage Bg { get; private set; }
			public void SetIndex(int i)
			{
				Index = i;
			}
			public void SetPath(string str)
			{
				Path = str;
			}
			public void SetName(string str)
			{
				Name = str;
			}
			public void SetEdgeColor(string str)
			{
				Color c = ColorTranslator.FromHtml(str);
				EdgeColor = GetColor(c.R, c.G, c.B);
			}
			public void SetSecret(string str)
			{
				Secret = int.Parse(str);
			}
            public void SetBgFolder(string str)
            {
                GenreBg = new GenreImage(System.IO.Path.GetDirectoryName(Path) + @"\" + str + @"\" + GENRE_BG);
                Bg.SetImage(System.IO.Path.GetDirectoryName(Path) + @"\" + str + @"\" + BG);
            }
            public void SetBg(string str)
			{
				//Bg = new GenreImage(System.IO.Path.GetDirectoryName(Path) + @"\GenreBg\GenreBg.png");
			}
            public void SetBackGround(string str)
            {
                //Handle = LoadGraph(System.IO.Path.GetDirectoryName(Path) + @"\" + str);
            }
			public void SetPtsFirstIndex(int index)
			{
				if (PtsFirstIndex == -1)
				{
					PtsFirstIndex = index;
				}
			}
			public void SetPtsLastIndex(int index)
			{
				PtsLastIndex = index;
			}
			public int PtsFirstIndex { get; private set; } = -1;
			public int PtsLastIndex { get; private set; } = -1;

			public static CGenre Construct(CGenreList c)
			{			
				return new CGenre();
			}
			CGenre()
			{
                GenreBg = new GenreImage("");
                Bg = new SelfDxImage();
			}
		}
		class CGenreList
		{
			public CGenre this[int index]
			{
				get
				{
					return Genre[index];
				}
			}
			static CGenreList GenreList;
			List<CGenre> Genre;
			public int Count
			{
				get
				{
					return Genre.Count;
				}
			}
			public int Last
			{
				get
				{
					return Genre.Count - 1;
				}
			}
			public CGenre GetCurrent() { return Genre[SongSelect.ChartControl.GenreCurrent]; }
            public void SetPath(string str)
            {
                Genre.Add(CGenre.Construct(this));
                Genre[Last].SetPath(str);
                Genre[Last].SetIndex(Last);
            }
			public void SetName(string str)
			{
				Genre[Last].SetName(str);
			}
			public void SetEdgeColor(string str)
			{
				Genre[Last].SetEdgeColor(str);
			}
			public void SetSecret(string str)
			{
				Genre[Last].SetSecret(str);
			}
            public void SetBgFolder(string str)
            {
                Genre[Last].SetBgFolder(str);
            }
			public void SetBg(string str)
			{
				//Genre[Last].SetBg(str);
			}
            public void SetBackGround(string str)
            {
                //Genre[Last].SetBackGround(str);
            }
			public static CGenreList Construct(SSongSelect s)
			{
				if (GenreList != null)
				{
					throw new ExistingException(GenreList.GetType().Name);
				}
				GenreList = new CGenreList();
				return GenreList;
			}
			CGenreList()
			{
				Genre = new List<CGenre>();
				TextExtra[] te = 
                {
					new TextExtra("NAME=", SetName),
					new TextExtra("SECRET=", SetSecret),
					new TextExtra("EDGECOLOR=", SetEdgeColor),
                    new TextExtra("BGFOLDER=", SetBgFolder),
                    new TextExtra("BG=", SetBg),
                    new TextExtra("BackGround=", SetBackGround),
                };
                var gfs =
                    from gs
                    in Directory.EnumerateFiles(MainConfig.ChartFolder, "genre.ini", SearchOption.AllDirectories)
                    orderby gs
                    //where g.Length - g.Replace(@"\", "").Length == 2
                    select gs;
                foreach (var gs in gfs)
                {
                    SetPath(gs);
                    GetStreamData(gs, te);
                }
            }
		}
		CGenreList GenreList;

		class CChart
		{
			public int Index { get; private set; }
			public string Title { get; private set; }
			public string Kana { get; private set; } = "";
            public string ID { get; private set; } = @"_empty";
			public string FilePath { get; private set; }
			public int FontHandle { get; private set; }
			public string Wave { get; private set; }
			public int WaveHandle { get; private set; }
			public float Demostart { get; private set; }
			public class Course
			{
				public ECourseID courseID;
				public int Level = -1;
				public bool Branch = false;
				public void SetLevel(string str)
				{
                    int l;
                    if(int.TryParse(str,out l))
                    {
                        Level = l;
                    }
				}
				public Course(ECourseID c)
				{
					courseID = c;
				}
				public Course(ECourseID c, int l)
				{
					courseID = c;
					Level = l;
				}
			}
			public List<Course> Courses;
			public int GenreIndex { get; private set; }
            public double ExRateY { get; private set; } = 1.0;
			public void SetIndex(int index)
			{
				Index = index;
			}
			public void SetTitle(string str)
			{
				Title = str;
			}
            public void SetID(string str)
            {
                ID = str;
            }
			public void SetKana(string str)
			{
				Kana = str;
			}
			public void SetWave(string str)
			{
				Wave = Path.GetDirectoryName(FilePath) + @"\" + str;
			}
            public void SetDemoStart(string str)
            {
                float d;
                if (float.TryParse(str, out d))
                {
                    Demostart = d;
                }
                else
                {
					Demostart = 0;
                }
				SetSoundCurrentTime((int)(Demostart * 1000), WaveHandle);
			}
			public void SetCourse(string str)
			{
				ECourseID courseIDBuf = new ECourseID();

				switch (str.ToLower())
				{
					case "0":
					case "easy":
						courseIDBuf = ECourseID.Easy;
						break;
					case "1":
					case "normal":
						courseIDBuf = ECourseID.Normal;
						break;
					case "2":
					case "hard":
						courseIDBuf = ECourseID.Hard;
						break;
					case "3":
					case "oni":
						courseIDBuf = ECourseID.Oni;
						break;
					case "4":
					case "ura":
						courseIDBuf = ECourseID.Ura;
						break;
					default:
						ErrorBox("ファイルパス:" + FilePath + "\n\n" + "無効なコース名を取得しました。\n" + "aaaaa" + "\n\nソフトを終了します。", "コースエラー", MessageBoxButtons.OK, FilePath);
						break;
				}
				for (int i = 0; i < Courses.Count; i++)
				{
					if (Courses[i].courseID == courseIDBuf)
					{
						ErrorBox("ファイルパス:" + FilePath + "\n\n" + "aaaaa" + "\nこの難易度が複数存在することを確認しました。\nファイルを開くので、訂正してください。\n\nソフトを終了します。", "難易度エラー", MessageBoxButtons.OK, FilePath);
					}
				}
				Courses.Add(new Course(courseIDBuf));
			}
			public void SetLevel(string str)
			{
				try
				{
					Courses[Courses.Count - 1].SetLevel(str);
					Courses.Sort((a, b) => a.courseID - b.courseID);
				}
				catch
				{
					ErrorBox("ファイルパス:" + "" + "\n\n" + "" + "\nレベルが難易度よりも先に書かれています。\nファイルを訂正してください。\n\nソフトを終了します。", "難易度エラー", MessageBoxButtons.OK, FilePath);
				}
			}
			public void SetGenreIndex(int index)
			{
				GenreIndex = index;
			}
            public bool LoadCheckWaveHandle()
            {
                switch (CheckHandleASyncLoad(WaveHandle))
                {
                    case TRUE:
                        return false;
                    case FALSE:
                        return true;
                    default:
                        return false;
                }
            }
			public bool SetWaveHandle()
			{
				if (File.Exists(Wave))
				{
					SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMPRESS);
					WaveHandle = LoadSoundMem(Wave);
                    SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMNOPRESS);
					return true;
				}
				else
				{
					return false;
				}
			}
            public void SetFontHandle()
            {
                FontHandle = Origin.SongFont;
                ExRateY = (double)(CGenreImageControl.OtherHeight - ((CGenreImageControl.PieceHeight + 15) * 2)) / (Origin.GetDrawExtendVStringHeight(1.0,Title,FontHandle));
                if (ExRateY > 1.0)
                {
                    ExRateY = 1.0;
                }                
            }
			public static CChart Construct(CChartList c, string fp, CGenre g)
			{
				return new CChart(fp, g);
			}
			CChart(string fp, CGenre g)
			{
				Courses = new List<Course>();
				FilePath = fp;
			}
		}
        class CChartList : IRelation
        {
            static CChartList ChartList;

            static CChartControl ChartControl;

            TextExtra[] te;
            List<CChart> Chart;

            public class CSort
            {
                public string Name { get; private set; }
                public Action SortAction { get; private set; }
                public CSort(string n, Action sa)
                {
                    Name = n;
                    SortAction = sa;
                }
            }
            public class CSortList : List<CSort>
            {
                public int NowType { get; private set; } = 0;
                public string GetCurrentName()
                {
                    return this[NowType].Name;
                }
                public void Change()
                {
                    NowType = (NowType + 1) % Count;
                    this[NowType].SortAction();
                }
            }
            public CSortList SortList { get; private set; }

            public CChart this[int index]
            {
                get
                {
                    return Chart[index];
                }
            }

            public int Last
            {
                get
                {
                    return Chart.Count - 1;
                }
            }
            public int Count
            {
                get
                {
                    return Chart.Count;
                }
            }
            public CChart GetCurrent()
            {
                return Chart[SongSelect.ChartControl.ChartCurrent];
            }
            void AddRange(CGenre g, string ds)
            {
                var files =
                    from f
                    in Directory.EnumerateFiles(ds, "*.pts", SearchOption.TopDirectoryOnly)
                    orderby f
                    select f;
                foreach (var f in files)
                {
                    Add(f, g);
                }
                var dirs =
                    from d
                    in Directory.EnumerateDirectories(ds, "*", SearchOption.TopDirectoryOnly)
                    orderby d
                    select d;
                foreach (var d in dirs)
                {
                    AddRange(g, d);
                }
            }
            void AddRange(CGenre g)
            {
                var files =
                    from f
                    in Directory.EnumerateFiles(Path.GetDirectoryName(g.Path), "*.pts", SearchOption.TopDirectoryOnly)
                    orderby f
                    select f;
                foreach (var f in files)
                {
                    Add(f, g);
                }
                var dirs =
                    from d
                    in Directory.EnumerateDirectories(Path.GetDirectoryName(g.Path), "*", SearchOption.TopDirectoryOnly)
                    orderby d
                    select d;
                foreach (var d in dirs)
                {
                    AddRange(g, d);
                }
            }
            void Add(string path, CGenre g)
            {
                Chart.Add(CChart.Construct(this, path, g));
                GetStreamData(path, te);
                Chart[Last].SetIndex(Last);
                Chart[Last].SetGenreIndex(g.Index);
                g.SetPtsFirstIndex(Last);
                g.SetPtsLastIndex(Last);
            }

            void SetTitle(string str)
            {
                Chart[Last].SetTitle(str);
            }
            void SetID(string str)
            {
                Chart[Last].SetID(str);
            }
            void SetKana(string str)
            {
                Chart[Last].SetKana(str);
            }
            void SetWave(string str)
            {
                Chart[Last].SetWave(str);
            }
            void SetDemoStart(string str)
            {
                Chart[Last].SetDemoStart(str);
            }
            void SetCourse(string str)
            {
                Chart[Last].SetCourse(str);
            }
            void SetLevel(string str)
            {
                Chart[Last].SetLevel(str);
            }
            public void SetFontHandle()
            {
                foreach (var c in Chart)
                {
                    c.SetFontHandle();
                }
            }

            int CompareIndex(CChart a, CChart b)
            {
                return a.Index - b.Index;
            }
            int CompareTitle(CChart a, CChart b)
            {
                string a_n, b_n;
                if (a.Kana == "") { a_n = a.Title; } else { a_n = a.Kana; }
                if (b.Kana == "") { b_n = b.Title; } else { b_n = b.Kana; }
                return string.Compare(a_n, b_n,StringComparison.Ordinal);
            }
            void SortGenre()
            {
                int current = GetCurrent().Index;
                Chart.Sort(CompareIndex);
                ChartControl.FormatChart(Chart.FindIndex(c => c.Index == current));
                PrintMessage("譜面をジャンル順にソートしました。");
            }
            void SortTitle()
            {
                int current = GetCurrent().Index;
                Chart.Sort(CompareTitle);
                ChartControl.FormatChart(Chart.FindIndex(c => c.Index == current));
                PrintMessage("譜面をタイトル順にソートしました。");
            }

            public void SortChange()
            {
                SortList.Change();
            }
            public static CChartList Construct(SSongSelect s, CGenreList gl)
            {
                if (ChartList != null)
                {
                    throw new ExistingException(ChartList.GetType().Name);
                }
                ChartList = new CChartList(gl);
                return ChartList;
            }

            void IRelation.Link()
            {
                ChartControl = SongSelect.ChartControl;
            }

            CChartList(CGenreList gl)
            {
                te = new TextExtra[] {
                    new TextExtra("TITLE:", SetTitle),
                    new TextExtra("KANA:", SetKana),
                    new TextExtra("ID:", SetID),
                    new TextExtra("WAVE:", SetWave),
                    new TextExtra("DEMOSTART:", SetDemoStart),
                    new TextExtra("COURSE:", SetCourse),
                    new TextExtra("LEVEL:", SetLevel),
                    //new TextExtra("#BRANCHSTART:", setBranch),
                };
                Chart = new List<CChart>();
                for (int i = 0; i < gl.Count; i++)
                {
                    AddRange(gl[i]);
                }
                SortList = new CSortList()
                {
                    new CSort("Genre", SortGenre),
                    new CSort("Title", SortTitle),
                };
                Relation.Add(this);
            }
        }
		CChartList ChartList;

        class SelfDebug : Debug<SelfDebug>, IRelation
        {
            const string ON = "ON";
            const string OFF = "OFF";

            string SwitchBool(bool b)
            {
                switch (b)
                {
                    case true:
                        return ON;
                    default:
                        return OFF;
                }
            }
            uint SwitchColor(bool b)
            {
                switch (b)
                {
                    case true:
                        return GetColor(255, 255, 0);
                    default:
                        return GetColor(255, 255, 255);
                }
            }
            uint SwitchColor(float speed)
            {
                if (speed == 1.0f)
                {
                    return GetColor(255, 255, 255);
                }
                else
                {
                    return GetColor(255, 255, 0);
                }
            }
            uint SwitchEdgeColor(bool b)
            {
                switch (b)
                {
                    case true:
                        return GetColor(255, 255, 0);
                    default:
                        return GetColor(255, 255, 255);
                }
            }

            uint SwitchRPColor(int per)
            {
                switch (per)
                {
                    case 0:
                        return GetColor(255, 255, 255);
                    default:
                        return GetColor(255, 255, 0);
                }
            }

            static CChartControl ChartControl;
            static CChartList ChartList;
            static CGenreList GenreList;

            void IRelation.Link()
            {
                ChartList = SongSelect.ChartList;
                GenreList = SongSelect.GenreList;
                ChartControl = SongSelect.ChartControl;
            }
            string AllowString(int i)
            {
                switch (ChartControl.CourseCurrent == i)
                {
                    case true:
                        return "→";
                    default:
                        return "";
                }
            }
            protected override void LeftDraw()
            {
                Left("並び順:" + ChartList.SortList.GetCurrentName() + " " + (ChartControl.ChartCurrent + 1).ToString() + "/" + ChartList.Count.ToString());
                Left("ジャンル:" + GenreList.GetCurrent().Name);
                Left("タイトル:" + ChartList.GetCurrent().Title);
                Left("ふりがな:" + ChartList.GetCurrent().Kana);
                switch (SongSelect.NowInner)
                {
                    case ENowInner.Song:
                        SetDrawBright(127, 127, 127);
                        for (int i = 0; i < ChartList.GetCurrent().Courses.Count; i++)
                        {
                            if (ChartList.GetCurrent().Courses[i] != null)
                            {
                                Left(ChartList.GetCurrent().Courses[i].courseID.Name() + ">Lv." + ChartList.GetCurrent().Courses[i].Level.ToString(), DxColor.Orange);
                            }
                        }
                        SetDrawBright(255, 255, 255);
                        break;
                    case ENowInner.Course:
                        for (int i = 0; i < ChartList.GetCurrent().Courses.Count; i++)
                        {
                            if (ChartList.GetCurrent().Courses[i] != null)
                            {
                                Left(AllowString(i) + ChartList.GetCurrent().Courses[i].courseID.Name() + ">Lv." + ChartList.GetCurrent().Courses[i].Level.ToString(), DxColor.Orange);
                            }
                        }
                        break;
                }
            }
            protected override void RightDraw()
            {
                Right("オート:" + SwitchBool(PlayConfig.Auto), SwitchColor(PlayConfig.Auto));
                Right("スピード:x" + PlayConfig.Speed, SwitchColor(PlayConfig.Speed));
                Right("音符入れ替え率:" + PlayConfig.ReplacePer + "%", SwitchRPColor(PlayConfig.ReplacePer));
                Right("音符非表示:" + SwitchBool(PlayConfig.NoteOff), SwitchColor(PlayConfig.NoteOff));
                Right("音量:" + SoundControl.Get().State);
            }
            public SelfDebug()
            {
                Relation.Add(this);
            }
        }
		SelfDebug Debug;
        
        SelfDxSound SEDon;
        SelfDxSound SEKat;
        SelfDxSound SEBack;
        SelfDxSound SEFactorMove;
        SelfDxSound SEOption;
        
		static int demoToneInterval = 0;
		static int demoTone = 2;

        public override void Start()
		{
			if (State == EState.Null)
			{
                Relation = CRelation.Construct();

				GameActionControl = CGameActionControl.Construct(this);
                AddAction = GameActionControl.AddAction;

                Tone = CTone.Get();
                Demo = new CDemo();
                
                SEDon = new SelfDxSound().SetSound(MainConfig.SongSelect + @"Sound\Don.wav");
                SEKat = new SelfDxSound().SetSound(MainConfig.SongSelect + @"Sound\Kat.wav");
                SEBack = new SelfDxSound().SetSound(MainConfig.SongSelect + @"Sound\Back.wav");
                SEFactorMove = new SelfDxSound().SetSound(MainConfig.SongSelect + @"Sound\FactorMove.wav");
                SEOption = new SelfDxSound().SetSound(MainConfig.SongSelect + @"Sound\Option.wav");
                
                GenreList = CGenreList.Construct(this);
				ChartList = CChartList.Construct(this, GenreList);
				ChartControl = CChartControl.Construct(this);
				EscExit = CEscExit.Construct(this);

                Config = CConfig.Construct(this);

                Debug = new SelfDebug();

                Relation.Start();
                
                Config.Start();

                AddAction(Debug);
                ChartList.SetFontHandle();
                
				State = EState.OK;
			}
			SceneOut = new CSceneOut(MainConfig.SongSelect + @"scene.png", -22);
		}
        public override void Update()
        {
            GameActionControl.Update();
            if (demoTone == 0)
            {
                DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_FILE);
                //int d = DX.LoadSoundMem(toneDir[toneElem] + @"\don.wav");
                //DX.PlaySoundMem(d, DX.DX_PLAYTYPE_BACK);
                demoTone = 1;
            }
            if (demoTone == 1 && demoToneInterval > 24)
            {
                DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_FILE);
                //int k = DX.LoadSoundMem(toneDir[toneElem] + @"\kat.wav");
                //DX.PlaySoundMem(k, DX.DX_PLAYTYPE_BACK);
                demoTone = 2;
            }
            else
            {
                demoToneInterval++;
            }
            SetCreateSoundDataType(DX_SOUNDDATATYPE_MEMNOPRESS);
            if (Key.GetCount(Key.INPUT_O) == 1) { ChartList.SortChange(); SEFactorMove.Play(); }

            if (ChartControl.KeyLeft)
            {
                #region 左選択
                switch (NowInner)
                {
                    case ENowInner.Song:
                        ChartControl.MoveChartLeft();
                        break;
                    case ENowInner.Course:
                        ChartControl.MoveCourseLeft();
                        break;
                }
                SEKat.Play();
                #endregion
            }
            if (ChartControl.KeyRight)
            {
                #region 右選択
                switch (NowInner)
                {
                    case ENowInner.Song:
                        ChartControl.MoveChartRight();
                        break;
                    case ENowInner.Course:
                        ChartControl.MoveCourseRight();
                        break;
                }
                SEKat.Play();
                #endregion
            }
            if (ChartControl.KeyUnitLeft)
            {
                switch (NowInner)
                {
                    case ENowInner.Song:
                        ChartControl.MoveChartUnitLeft();
                        SEFactorMove.Play();
                        break;
                }
            }
            if (ChartControl.KeyUnitRight)
            {
                switch (NowInner)
                {
                    case ENowInner.Song:
                        ChartControl.MoveChartUnitRight();
                        SEFactorMove.Play();
                        break;
                }
            }
            if (ChartControl.KeyRandom)
            {
                #region 譜面ランダム
                switch (NowInner)
                {
                    case ENowInner.Song:
                        ChartControl.MoveChartRandom();
                        SEFactorMove.Play();
                        break;
                }
                #endregion
            }
            if (ChartControl.KeyEnter)
            {
                #region 決定時
                switch (NowInner)
                {
                    case ENowInner.Song:
                        if (CGenreImageControl.CCenter.diffPointX == 0)
                        {
                            ChartControl.SetInitialCourse();
                            SEDon.Play();
                        }
                        break;
                    case ENowInner.Course:
                        SceneOut.Start();
                        SEDon.Play();
                        break;
                }
                #endregion
            }
            if (ChartControl.KeyBack)
            {
                #region 戻り時
                switch (NowInner)
                {
                    case ENowInner.Course:
                        NowInner = ENowInner.Song;
                        SEBack.Play();
                        break;
                    default:
                        break;
                }
                #endregion
            }
            if (ChartControl.KeyMoveChart)
            {
                #region 譜面選択移動時
                switch (NowInner)
                {
                    case ENowInner.Song:
                        Demo.Reset();
                        break;
                    default:
                        break;
                }
                #endregion
            }
            #region 設定
            if (Key.GetCount(Key.F1) == 1)
            {
#region オート
                PlayConfig.ChangeAuto();
                SEOption.Play();
#endregion
            }
            if (Key.GetCount(Key.F2) == 1)
            {
#region スピード
                PlayConfig.ChangeSpeed();
                SEOption.Play();
#endregion
            }
            if (Key.GetCount(Key.F3) == 1)
            {
                #region 音符入れ替え
                PlayConfig.ChangeReplacePer();
                SEOption.Play();
                #endregion
            }
            if (Key.GetCount(Key.F4) == 1)
            {
                #region ドロン
                PlayConfig.ChangeNoteOff();
                SEOption.Play();
                #endregion
            }
            #endregion
            Demo.Update();
            EscExit.Update();
        }
        public override void Draw()
        {
            GameActionControl.Draw();
            
            DrawStringToHandle(MainConfig.DrawCenterX - GetDrawStringWidthToHandle(GenreList.GetCurrent().Name, GenreList.GetCurrent().Name.Length, Origin.SongFont2) / 2, 2 + 40, GenreList.GetCurrent().Name, GetColor(0, 0, 0), Origin.SongFont2, GetColor(0, 0, 0));

            SetDrawBlendMode(DX_BLENDMODE_ALPHA, 255);
            DrawStringToHandle(-2 + MainConfig.DrawCenterX - GetDrawStringWidthToHandle(GenreList.GetCurrent().Name, GenreList.GetCurrent().Name.Length, Origin.SongFont2) / 2, 40, GenreList.GetCurrent().Name, GetColor(255, 255, 255), Origin.SongFont2, GenreList.GetCurrent().EdgeColor);

            EscExit.Draw();
            SceneOut.Draw(EScene.Play);
        }
		public override void Finish()
		{
			SongSelectPassData.SetFilePath(this, ChartList.GetCurrent().FilePath);
			SongSelectPassData.SetCourseCurrent(this, ChartList.GetCurrent().Courses[ChartControl.CourseCurrent].courseID);
            Demo.Delete();
			NowInner = ENowInner.Song;
			SceneOut.Reset();
		}

        public static SSongSelect Get(CCommand c)
        {
            return SongSelect;
        }
		public static SSongSelect Construct(MainControl m)
		{
			if (SongSelect != null)
			{
				PrintMessage("選曲画面エラー");
			}
			SongSelect = new SSongSelect();
			return SongSelect;
		}
		SSongSelect()
		{
			SongSelectPassData = PDSongSelectPassData.Construct(this);
			State = EState.Null;
		}
	}
	class PDSongSelectPassData
	{
		static PDSongSelectPassData SongSelectPassData;

		public string FilePath { get; private set; }
        public int Style = 1;
		public void SetFilePath(SSongSelect s, string fp)
		{
			FilePath = fp;
		}
		public ECourseID CourseCurrent { get; private set; }
		public void SetCourseCurrent(SSongSelect s, ECourseID c)
		{
			CourseCurrent = c;
		}
		static PDSongSelectPassData Construct()
		{
			if (SongSelectPassData == null)
			{
				SongSelectPassData = new PDSongSelectPassData();
			}
			return SongSelectPassData;
		}
		public static PDSongSelectPassData Construct(SPlay s)
		{
			return Construct();
		}
		public static PDSongSelectPassData Construct(SSongSelect a)
		{
			return Construct();
		}
		PDSongSelectPassData()
		{
		}
	}
}