#define DEMOMUSIC
#define NOTE_EFFECT
//#define PLAYFIRST
//#define DEMOMUSIC_FROM_MEMORY

#define NOTE_CLASS
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DxLibDLL;
using static DxLibDLL.DX;
using System.Linq;
using System.Collections.ObjectModel;
using Pansystar;
using static Pansystar.Extensions;
using DxLibForCS;
using System.Xml.Linq;

namespace PraTaiko
{
    class SPlay : BaseScene
	{
		static SPlay Play;
        static PDSongSelectPassData SongSelectPassData;
        
        class CMyStopwatch
        {
            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            static extern bool QueryPerformanceFrequency(out long lpFrequency);

            //周波数
            long Frequency;
            //スタートカウント
            long StartCount;
            //経過カウント取得用
            long GetCount;

            /// <summary>開始</summary>
            public void Start()
            {
                QueryPerformanceCounter(out StartCount);
            }

            /// <summary>経過時間を取得します</summary>
            public long TotalMillisecond
            {
                get
                {
                    QueryPerformanceCounter(out GetCount);
                    return (GetCount - StartCount) * 1000 / Frequency;
                }
            }
            public CMyStopwatch()
            {
                QueryPerformanceFrequency(out Frequency);
            }
        }
        CMyStopwatch MyStopWatch;

        class CConfig
        {
            const string f = @"Config.xml";
            static SPlay P;
            public void Start()
            {
                XElement element = null;
                XAttribute attribute;

                if (File.Exists(MainConfig.SongSelect + f))
                {
                    element = XElement.Load(MainConfig.Play + f);
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
                        case "Taiko":
                            TaikoImageControl.Set(item);
                            break;
                        //case "Other":
                        //    COtherImageControl.Set(item);
                        //    break;
                    }
                }
            }
            public static CConfig Construct(SPlay p)
            {
                return new CConfig(p);
            }
            CConfig(SPlay p)
            {
                P = p;
            }
        }
        CConfig Config;

        class SPlayDxFont : DxFont<SPlayDxFont> { }
        class SPlayDxSound : DxSound<SPlayDxSound> { }
        class SPlayDxImage : DxImage<SPlayDxImage> { }

        class SPlayDebug : Debug<SPlayDebug>
        {
            protected override void LeftDraw()
            {
                Left("タイトル:");
                Left("難易度:" + "  " + "Lv.");                
            }
            protected override void RightDraw()
            {
            }
        }
        SPlayDebug Debug;

        CGameActionControl GameActionControl;
        static Action<IGameAction> AddAction;

		class Images
		{
			static void errorExitMessage(string method)
			{
				MessageBox.Show("基底クラスの" + method + "が呼び出されています。\n\nこのエラーが出た場合、お手数おかけしますが、作成者に連絡をしてください。", "PlayシーンImagesエラー", MessageBoxButtons.OK);
				Environment.Exit(0);
			}
			#region virtualメソッド
			public virtual void setGaugeImage(string dp) { }
			public virtual void setFrameImage(string dp) { }
			public virtual void setGaugePoint(string[] p) { }
			public virtual void setFramePoint(string[] p) { }
			public virtual void setNormalImage(string dp) { }
			public virtual void setClearImage(string dp) { }
			public virtual void setSoulImage(string dp) { }
			public virtual void setSoulPoint(string[] p) { }
			public virtual void setImage(string dp) { errorExitMessage("setImage"); }
			public virtual void setPoint(string[] p) { errorExitMessage("setPoint"); }
			public virtual void setUpper(string dp) { errorExitMessage("setUpper"); }
			public virtual void setStage(string dp) { errorExitMessage("setStage"); }
			public virtual void setNormalSize(string[] s) { errorExitMessage("setNormalSize"); }
			public virtual void setClearSize(string[] s) { errorExitMessage("setClearSize"); }
			public virtual void Update() { }
			public virtual void Draw() { }
			#endregion
		}
		class BgImage : Images
		{
			public class UpperImages
			{
				public Image upperNormal;
				public Image upperClear;

				int alpha = 0;
				int alphaDiff = 20;
				public void setUpper(string dp)
				{
					upperNormal = new Image(dp + "normal.png");
					upperNormal.setRepeat(1, -1, 0);
					upperClear = new Image(dp + "clear.png");
					upperClear.setRepeat(1, -1, 0);
				}
				public void Update()
				{
					if (PDPlayPassData.clear)
					{
						alpha += alphaDiff;
						if (alpha > 255)
						{
							alpha = 255;
						}
					}
					else
					{
						alpha -= alphaDiff;
						if (alpha < 0)
						{
							alpha = 0;
						}
					}
				}
				public void Draw()
				{
					upperNormal.Draw();
					DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, alpha);
					upperClear.Draw();
					DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 0);
				}
				public UpperImages()
				{
					upperNormal = new Image("");
					upperClear = new Image("");
				}
			}
			UpperImages upperImages;

			public class StageImages
			{
				Image stage;
				public void setStage(string dp)
				{
					stage = new Image(dp + "stage.png");
					string[] p = { "0", "buttom" };
					stage.setPoint(p);
				}
				public void Draw()
				{
					stage.Draw();
				}
				public StageImages()
				{
					stage = new Image("");
				}
			}
			StageImages stageImages;

			public override void setUpper(string dp)
			{
				upperImages.setUpper(dp);
			}
			public override void setStage(string dp)
			{
				stageImages.setStage(dp);
			}
			public override void Update()
			{
				upperImages.Update();
			}
			public override void Draw()
			{
				upperImages.Draw();
				stageImages.Draw();
			}
			public BgImage()
			{
				upperImages = new UpperImages();
				stageImages = new StageImages();
			}
		}
		class NoteFieldImage : Images
		{
			public static void setHit(bool h)
			{
				hit = h;
			}
			static bool hit = false;
			int value = 0;
			bool don;
			Image f_base;
			Image f_don;
			Image f_kat;
			Image f_hit;
			Image f_gogo;
			public override void setImage(string dp)
			{
				f_base = new Image(dp + "base.png");
				f_don = new Image(dp + "don.png");
				f_kat = new Image(dp + "kat.png");
				f_hit = new Image(dp + "hit.png");
				f_gogo = new Image(dp + "gogo.png");
			}
			public override void setPoint(string[] p)
			{
				int x = int.Parse(p[0]);
				int y = int.Parse(p[1]);

				f_base.setPoint(x, y);
				f_don.setPoint(x, y);
				f_kat.setPoint(x, y);
				f_hit.setPoint(x, y);
				f_gogo.setPoint(x, y);
			}
			public override void Update()
			{
				if (value > 0)
				{
					value--;
					if (value == 0)
					{
						hit = false;
					}
				}
				if (!PlayConfig.Auto)
				{
					if (Key.GetCount(Key.F) == 1 || Key.GetCount(Key.J) == 1)
					{
						value = 6;
						don = true;
					}
					if (Key.GetCount(Key.D) == 1 || Key.GetCount(Key.K) == 1)
					{
						value = 6;
						don = false;
					}
				}
			}
			public override void Draw()
			{
				f_base.Draw();
				if (Play.nowGogo)
				{
					f_gogo.Draw();
				}
				if (value > 0)
				{
					if (hit)
					{
						f_hit.Draw();
					}
					if (don)
					{
						f_don.Draw();
					}
					else
					{
						f_kat.Draw();
					}
				}
			}
			public NoteFieldImage()
			{
				f_base = new Image();
				f_don = new Image();
				f_kat = new Image();
				f_hit = new Image();
				f_gogo = new Image();
			}
		}
		class GaugeImage : Images
		{
			int maxLoop = 0;
			int normalLoop = 0;
			int maxSpeed = 3;
			int normalSpeed = 3;

			int normalNum = 40;
			int gaugePointX;
			int gaugePointY;
			int mask;
			List<Image> max;
			Image frame;
			Image normal_off;
			Image normal_on;

			Image clear_off;
			Image clear_on;

			Image soul;

			public override void setFrameImage(string dp)
			{
				if (!File.Exists(dp + Enum.GetName(typeof(ECourseID), SongSelectPassData.CourseCurrent) + @"\frame.png"))
				{
					frame = new Image(dp + @"default\frame.png");
				}
				else
				{
					frame = new Image(dp + Enum.GetName(typeof(ECourseID), SongSelectPassData.CourseCurrent) + @"\frame.png");
				}
				for (int i = 0; File.Exists(dp + "max_" + (i + 1) + ".png"); i++)
				{
					max.Add(new Image(dp + "max_" + (i + 1) + ".png"));
				}
				if (!File.Exists(dp + Enum.GetName(typeof(ECourseID), SongSelectPassData.CourseCurrent) + @"\mask.png"))
				{
					mask = DX.LoadMask(dp + @"default\mask.png");
				}
				else
				{
					mask = DX.LoadMask(dp + Enum.GetName(typeof(ECourseID), SongSelectPassData.CourseCurrent) + @"\mask.png");
				}
			}
			public override void setFramePoint(string[] p)
			{
				frame.setPoint(p);
				for (int i = 0; i < max.Count; i++)
				{
					max[i].setPoint(frame.x, frame.y);
				}
			}
			public override void setGaugeImage(string dp)
			{
				normal_off = new Image(dp + "normal_off.png");
				normal_on = new Image(dp + "normal_on.png");
				clear_off = new Image(dp + "clear_off.png");
				clear_on = new Image(dp + "clear_on.png");
			}
			public override void setGaugePoint(string[] p)
			{
				gaugePointX = int.Parse(p[0]);
				gaugePointY = int.Parse(p[1]);
			}
			public override void setSoulImage(string dp)
			{
				soul = new Image(dp + "soul.png");
			}
			public override void setSoulPoint(string[] p)
			{
				soul.setPoint(p);
			}
			public override void Draw()
			{
				DX.CreateMaskScreen();
				DX.DrawMask(frame.x, frame.y, mask, DX.DX_MASKTRANS_BLACK);
				for (int i = 0; i < 50; i++)
				{
					if (PDPlayPassData.gauge < 200 * (i + 1))
					{
						if (i < normalNum - 1)
						{
							normal_off.Draw(gaugePointX + normal_off.x + i * normal_off.sizeX, gaugePointY + normal_off.y);
						}
						else
						{
							clear_off.Draw(gaugePointX + clear_off.x + i * clear_off.sizeX, gaugePointY + clear_off.y);
						}
					}
					else
					{
						if (i < normalNum - 1)
						{
							if (PDPlayPassData.clear && normalLoop / normalSpeed > 7)
							{
								clear_on.Draw(gaugePointX + clear_off.x + i * clear_off.sizeX, gaugePointY + clear_off.y);
							}
							else
							{
								normal_on.Draw(gaugePointX + normal_off.x + i * normal_off.sizeX, gaugePointY + normal_off.y);
							}
						}
						else
						{
							clear_on.Draw(gaugePointX + clear_off.x + i * clear_off.sizeX, gaugePointY + clear_off.y);
						}
					}
				}
				normalLoop++;
				if (normalLoop / normalSpeed > 8)
				{
					normalLoop = 0;
				}
				if (PDPlayPassData.max == true)
				{
					max[maxLoop / maxSpeed].Draw();
					maxLoop++;
					if (maxLoop / maxSpeed == 7)
					{
						maxLoop = 0;
					}
				}
				DX.DeleteMaskScreen();
				frame.Draw();
				soul.Draw();
			}
			public GaugeImage()
			{
				frame = new Image();
				normal_off = new Image();
				normal_on = new Image();
				clear_off = new Image();
				clear_on = new Image();
				max = new List<Image>();
				soul = new Image();
				switch (SongSelectPassData.CourseCurrent)
				{
					case ECourseID.Easy:
						normalNum = 50 - 20;
						break;
					case ECourseID.Normal:
					case ECourseID.Hard:
						normalNum = 50 - 15;
						break;
					case ECourseID.Oni:
					case ECourseID.Ura:
						normalNum = 50 - 10;
						break;
				}
			}
		}
		List<Images> images;

		class CEffectNoteList
		{
			class EffectNote
			{
				static int height = 250;
				static int r;
				static int rot_init;
				static int center = 1280 - judgeJustPointX;
				static int cx;
				static int cy;
				public static int nowElem = 0;
				public char noteID;
				public bool fin = true;

				public double displacement;
				int x;
				int y;
				int rot;
				int rot_speed;

				public static void Initialize()
				{
					double x1, y1;
					double x2, y2;
					double x3, y3;

					double G;

					x1 = 0;
					y1 = 0;

					x2 = center / 2;
					y2 = height;

					x3 = 1192 + 32 - judgeJustPointX;
					y3 = height - (115 + 32);

					G = (y2 * x1 - y1 * x2 + y3 * x2 - y2 * x3 + y1 * x3 - y3 * x1);
					cx = Convert.ToInt32(((x1 * x1 + y1 * y1) * (y2 - y3) + (x2 * x2 + y2 * y2) * (y3 - y1) + (x3 * x3 + y3 * y3) * (y1 - y2)) / (2 * G));
					cy = Convert.ToInt32(((x1 * x1 + y1 * y1) * (x2 - x3) + (x2 * x2 + y2 * y2) * (x3 - x1) + (x3 * x3 + y3 * y3) * (x1 - x2)) / (2 * G));
					r = Convert.ToInt32(Math.Sqrt((x1 - cx) * (x1 - cx) + (y1 - cy) * (y1 - cy)));
					rot_init = Convert.ToInt32((Math.Acos((double)center / (2 * r)) * 180 / Math.PI));
					arc = Convert.ToInt32(2 * Math.PI * r * (180 - rot_init * 2) / 360);
					cx += judgeJustPointX;
					cy += Pts.Note.pointY;
				}
				public void Update()
				{
					if (!fin)
					{
						var rad = rot * Math.PI / 180;

						var bx = Convert.ToInt32(cx + Math.Cos(rad) * r);
						var by = Convert.ToInt32(cy + Math.Sin(rad) * r);
						if (bx > 1192 + 32)
						{
							bx = 1192 + 32;
							by = 115 + 32;
						}
						x = bx;
						y = by;
						rot += rot_speed;

						if (rot > 360)
						{
							fin = true;
							rot = rot % 360;
						}
					}
				}
				public void Draw()
				{
					if (!fin)
					{
						switch (noteID)
						{
							case '1':
								DX.DrawRotaGraph(x, y, 1.0, 0.0, Pts.Don.images[0].handle, 1);
								break;
							case '2':
								DX.DrawRotaGraph(x, y, 1.0, 0.0, Pts.Kat.images[0].handle, 1);
								break;
							case '3':
								DX.DrawRotaGraph(x, y, 1.0, 0.0, Pts.DonBig.images[0].handle, 1);
								break;
							case '4':
								DX.DrawRotaGraph(x, y, 1.0, 0.0, Pts.KatBig.images[0].handle, 1);
								break;
						}
					}
				}
				public void Set(char n, double dp)
				{
					noteID = n;
					displacement = dp;
					rot_speed = Convert.ToInt32(displacement);
					x = judgeJustPointX;
					y = Pts.Note.pointY;
					rot = 180 + rot_init;
					fin = false;
				}
			}

			List<EffectNote> en;
			public static int arc;
			public int Size { get; private set; } = 64;
			int NowIndex = 0;
			public void Update()
			{
				en[NowIndex].Update();
				int temp = (NowIndex + 1) % Size;
				while (temp != NowIndex)
				{
					en[temp].Update();
					temp = (temp + 1) % Size;
				}
			}
			public void Draw()
			{
				en[NowIndex].Draw();
				int temp = (NowIndex + 1) % Size;
				while (temp != NowIndex)
				{
					en[temp].Draw();
					temp = (temp + 1) % Size;
				}
			}
			public void Set(char n, double dp)
			{
				en[NowIndex].Set(n, dp);
				NowIndex = (NowIndex + 1) % Size;
			}
			public CEffectNoteList()
			{
				EffectNote.Initialize();
				en = new List<EffectNote>();
				while (en.Count < Size) en.Add(new EffectNote());
			}
		}
		CEffectNoteList EffectNoteList;

		void ImagesInitialize()
		{
			images = new List<Images>();
			string category = "";
			string directory = "";

			Regex r_category = new Regex("^Category:");
			Regex r_directory = new Regex("^Directory:");

			#region bg正規表現宣言
			Regex r_bg_upperFolder = new Regex("^UpperFolder:");
			Regex r_bg_stageFolder = new Regex("^StageFolder:");
			#endregion
			#region noteField正規表現宣言
			Regex r_nField_point = new Regex("^Point:");
			#endregion
			#region gauge正規表現宣言
			Regex r_gauge_normalSize = new Regex("^NormalSize:");
			Regex r_gauge_clearSize = new Regex("^ClearSize:");
			Regex r_gauge_framePoint = new Regex("^FramePoint:");
			Regex r_gauge_gaugePoint = new Regex("^GaugePoint:");
			Regex r_gauge_soulPoint = new Regex("^SoulPoint:");
			#endregion

			if (!File.Exists(OldConfig.ImageFolder + @"\play\playConfig.ini"))
			{
				MessageBox.Show("playConfig.iniが見つかりません。\n\nソフトを終了します。", "playConfig.iniエラー", MessageBoxButtons.OK);
				Environment.Exit(0);
			}
			StreamReader sr = new StreamReader(OldConfig.ImageFolder + @"\play\playConfig.ini", Encoding.GetEncoding("Shift_JIS"));

			string tempReadLine;

			while (!sr.EndOfStream)
			{
				tempReadLine = sr.ReadLine();
				if (r_category.IsMatch(tempReadLine))
				{
					category = r_category.Replace(tempReadLine, "");
					directory = "";
					switch (category)
					{
						case "Bg":
							images.Add(new BgImage());
							break;
						case "NoteField":
							images.Add(new NoteFieldImage());
							break;
						case "Gauge":
							images.Add(new GaugeImage());
							break;
					}
				}
				else if (r_directory.IsMatch(tempReadLine))
				{
					string str = r_directory.Replace(tempReadLine, "");
					if (str != "")
					{
						directory = r_directory.Replace(tempReadLine, "") + @"\";
					}
					else
					{
						directory = "";
					}
				}
				else
				{
					switch (category)
					{
						case "Bg":
							#region Bg
							if (r_bg_upperFolder.IsMatch(tempReadLine))
							{
								string tempUpper = r_bg_upperFolder.Replace(tempReadLine, "");
								string[] tempUpperDirs = Directory.GetDirectories(OldConfig.ImageFolder + @"\play\" + directory + tempUpper + @"\");
								images[images.Count - 1].setUpper(tempUpperDirs[DX.GetRand(tempUpperDirs.Length - 1)] + @"\");
							}
							else if (r_bg_stageFolder.IsMatch(tempReadLine))
							{
								string tempStage = r_bg_stageFolder.Replace(tempReadLine, "");
								string[] tempStageDirs = Directory.GetDirectories(OldConfig.ImageFolder + @"\play\" + directory + tempStage + @"\");
								images[images.Count - 1].setStage(tempStageDirs[DX.GetRand(tempStageDirs.Length - 1)] + @"\");
							}
							#endregion
							break;
						case "NoteField":
							#region NoteField
							if (r_nField_point.IsMatch(tempReadLine))
							{
								string[] p = r_nField_point.Replace(tempReadLine, "").Split(',');
								images[images.Count - 1].setImage(OldConfig.ImageFolder + @"\play\" + directory);
								images[images.Count - 1].setPoint(p);
							}
							#endregion
							break;
						case "Gauge":
							#region Gauge
							if (r_gauge_gaugePoint.IsMatch(tempReadLine))
							{
								string[] p = r_gauge_gaugePoint.Replace(tempReadLine, "").Split(',');
								images[images.Count - 1].setGaugeImage(OldConfig.ImageFolder + @"\play\" + directory);
								images[images.Count - 1].setGaugePoint(p);
							}
							else if (r_gauge_framePoint.IsMatch(tempReadLine))
							{
								string[] p = r_gauge_framePoint.Replace(tempReadLine, "").Split(',');
								images[images.Count - 1].setFrameImage(OldConfig.ImageFolder + @"\play\" + directory);
								images[images.Count - 1].setFramePoint(p);
							}
							else if (r_gauge_soulPoint.IsMatch(tempReadLine))
							{
								string[] p = r_gauge_soulPoint.Replace(tempReadLine, "").Split(',');
								images[images.Count - 1].setSoulImage(OldConfig.ImageFolder + @"\play\" + directory);
								images[images.Count - 1].setSoulPoint(p);
							}
							#endregion
							break;
						default:
							break;
					}
				}
			}
			sr.Close();
		}

		public static int HeadBrankTime = 2000;
		public static int judgeJustPointX = 430;

		class Pts : IDisposable
		{
            #region 定数_難易度
            const string EASY_01 = @"0";
            const string EASY_02 = @"easy";
            const string EASY_03 = @"かんたん";

            const string NORMAL_01 = @"1";
            const string NORMAL_02 = @"normal";
            const string NORMAL_03 = @"ふつう";

            const string HARD_01 = @"2";
            const string HARD_02 = @"hard";
            const string HARD_03 = @"むずかしい";

            const string ONI_01 = @"3";
            const string ONI_02 = @"oni";
            const string ONI_03 = @"おに";

            const string URA_01 = @"4";
            const string URA_02 = @"ura";
            const string URA_03 = @"うら";
            #endregion

            static CTone Tone;
			public static int noteCount = 0;
			public int maxCombo;
			public static int barLineCount;
			public class BarLine
			{
				public class BarImage
				{
					public int handle { get; private set; }
					public BarImage(int h)
					{
						handle = h;
					}
				}
				static BarImage[] barImage;
				public static void Initialize()
				{
					barImage = new BarImage[2];
					barImage[0] = new BarImage(LoadGraph(@"image\play\barLine_1.png"));
				}
                public static void Finish()
                {
                    DeleteGraph(barImage[0].handle);
                }
                public bool disp { get; private set; }
				public double time { get; private set; }
				public double displacement { get; private set; }
				public double point { get; private set; }
				public double movePoint { get; private set; }
				public void Update(int dec, double cX)
				{
					movePoint = dec + point * PlayConfig.Speed + displacement * PlayConfig.Speed * cX;
				}
				public void Draw()
				{
					switch (disp)
					{
						case true:
							DrawRotaGraphF((float)movePoint, 250, 1.0, 0.0, barImage[0].handle, 1);
							break;
						default:
							break;
					}
				}
				public BarLine(bool d, double t, double nb, double nss, double o)
				{
					disp = d;
					time = t;
					displacement = (OldConfig.DrawSize.X - judgeJustPointX) / (60 * 240 / nb) * nss;
					point = (OldConfig.DrawSize.X - judgeJustPointX) / (240 / nb) * (t - o) / 1000 * nss;
				}
			}
			public abstract class Note
			{
				public static int pointY = 250;
				public static double initBpm;
				public char NoteID { get; protected set; }
				public char value { get; protected set; }
				public double Time { get; protected set; }
				public double Bpm { get; protected set; }
				public double effectDisplacement { get; protected set; }
				public double displacement { get; protected set; }
				public bool gogo { get; protected set; }
				public double point { get; protected set; }
				public double movePoint { get; protected set; }
				public bool catchNote { get; protected set; }
				public bool drawNote { get; protected set; }
				public virtual void Update(int dec, double cX)
				{
					movePoint = dec + point * PlayConfig.Speed + displacement * PlayConfig.Speed * cX;
				}
				public virtual bool getCatchNote()
				{
					MessageBox.Show("内部エラー\n\nエラーコード:14001", "内部エラー", MessageBoxButtons.OK);
					return false;
				}
				public virtual void matchCatchNote(bool draw) { }
				public double getGogoValue()
				{
					switch (gogo)
					{
						case true:
							return 1.2;
						default:
							return 1;
					}
				}
				public virtual double getLastTime() { return -1; }
				public virtual void setLast(double lt, double nb, double nss, double o) { }
				public virtual double getMoveLastPoint() { return -1; }
				public virtual void updateCount() { }
				public virtual void updateBalloonNum() { }
				public virtual int getBalloonNum() { return -1; }
				public virtual void setEffectDisplacement()
				{
					effectDisplacement = 0.25 * CEffectNoteList.arc / (60 * 240 / 160);
				}
				public abstract void Draw();
				public Note(char v, char sv, double t, double nb, double nss, double nm, bool gg, double o)
				{
					NoteID = v;
					switch (sv)
					{
						case '5':
						case '6':
						case '7':
							break;
						case '8':
							break;
						case '-':
							value = v;
							break;
						default:
							break;
					}
					catchNote = false;
					drawNote = true;
					gogo = gg;
					Time = t;
					Bpm = nb;
					displacement = nss * ((OldConfig.DrawSize.X - judgeJustPointX) / (60 * 240 / nb));
					point = nss * (t - o) * ((OldConfig.DrawSize.X - judgeJustPointX) / (240 / nb)) / 1000;
				}
			}
			public class Don : Note
			{
				public static NoteImage[] images;
				public static SPlayDxSound se;

				public Don(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o) : base(v, sv, t, nb, nss, nst, gg, o)
				{
				}
				public static void Initialize()
				{
					images = new NoteImage[3];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\don_1_note.png"));

					se = new SPlayDxSound().SetSound(Tone.DonPath);
				}
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                }
				public override bool getCatchNote()
				{
					return catchNote;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
					if (!draw)
					{
						drawNote = false;
					}
				}
				public override void Draw()
				{
					switch (drawNote)
					{
						case true:
							if (movePoint < 1280 + images[0].sizeX && movePoint > 0)
							{
								DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
							}
							break;
						default:
							break;
					}
				}
			}
			public class Kat : Note
			{
				public static NoteImage[] images;
				public static SPlayDxSound se;

				public Kat(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o) : base(v, sv, t, nb, nss, nst, gg, o)
				{
				}

				public static void Initialize()
				{
					images = new NoteImage[3];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\kat_1_note.png"));

                    se = new SPlayDxSound().SetSound(Tone.KatPath);
                }
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                }
                public override bool getCatchNote()
				{
					return catchNote;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
					if (!draw)
					{
						drawNote = false;
					}
				}
				public override void Draw()
				{
					switch (drawNote)
					{
						case true:
							if (movePoint < 1280 + images[0].sizeX && movePoint > 0)
							{
								DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
							}
							break;
						default:
							break;
					}
				}
			}
			public class DonBig : Note
			{
				public static NoteImage[] images;
				public static Sound se;

				public DonBig(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o) : base(v, sv, t, nb, nss, nst, gg, o)
				{
				}

				public static void Initialize()
				{
					images = new NoteImage[3];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\donBig_1_note.png"));

					se = new Sound(@"sound\01_太鼓\don.wav");
				}
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                }
                public bool catchDouble { get; private set; }
				public override bool getCatchNote()
				{
					return catchNote;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
					if (draw == false)
					{
						drawNote = false;
					}
				}
				public override void Draw()
				{
					switch (drawNote)
					{
						case true:
							if (movePoint < 1280 + images[0].sizeX && movePoint > 0)
							{
								DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
							}
							break;
						default:
							break;
					}
				}
			}
			public class KatBig : Note
			{
				public static NoteImage[] images;
				public static Sound se;

				public KatBig(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o) : base(v, sv, t, nb, nss, nst, gg, o)
				{
				}

				public static void Initialize()
				{
					images = new NoteImage[3];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\katBig_1_note.png"));

					se = new Sound(@"sound\01_太鼓\kat.wav");
				}
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                }
                public bool catchDouble { get; private set; }
				public override bool getCatchNote()
				{
					return catchNote;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
					if (!draw)
					{
						drawNote = false;
					}
				}
				public override void Draw()
				{
					switch (drawNote)
					{
						case true:
							if (movePoint < 1280 + images[0].sizeX && movePoint > 0)
							{
								DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
							}
							break;
						default:
							break;
					}
				}

			}
			public class Roll : Note
			{
				static NoteImage[] images;
				public int count = 0;
				public double lastTime;
				public double lastDisplacement;
				public double lastPoint;
				public double moveLastPoint;

				public Roll(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o) : base(v, sv, t, nb, nss, nst, gg, o)
				{
				}

				public override bool getCatchNote()
				{
					return false;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
				}
				public override double getLastTime()
				{
					return lastTime;
				}
				public override double getMoveLastPoint() { return moveLastPoint; }
				public override void setLast(double lt, double nb, double nss, double o)
				{
					lastTime = lt;
					lastDisplacement = nss * ((OldConfig.DrawSize.X - judgeJustPointX) / (60 * 240 / nb));
					lastPoint = nss * (lt - o) * ((OldConfig.DrawSize.X - judgeJustPointX) / (240 / nb)) / 1000;
				}
				public static void Initialize()
				{
					images = new NoteImage[5];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\roll_1_note.png"));
					images[3] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\roll_4_note.png"));
					images[4] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\roll_5_note.png"));
				}
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                    DX.DeleteGraph(images[3].handle);
                    DX.DeleteGraph(images[4].handle);
                }
                public override void updateCount()
				{
					count++;
				}
				public override void Update(int dec, double cX)
				{
					movePoint = dec + point * PlayConfig.Speed + displacement * PlayConfig.Speed * cX;
					moveLastPoint = dec + lastPoint * PlayConfig.Speed + lastDisplacement * PlayConfig.Speed * cX;
				}
				public override void Draw()
				{

					if (movePoint < 1280 + images[0].sizeX && moveLastPoint > 0)
					{
						DX.DrawExtendGraphF((float)movePoint, pointY - images[3].sizeY / 2, (float)moveLastPoint, pointY + images[3].sizeY / 2, images[3].handle, 1);
						DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
						DX.DrawRotaGraphF((float)moveLastPoint - images[4].sizeX / 8, pointY, 1.0, 0.0, images[4].handle, 1);
					}
				}
			}
			public class RollBig : Note
			{
				static NoteImage[] images;
				public int count = 0;
				public double lastTime;
				public double lastDisplacement;
				public double lastPoint;
				public double moveLastPoint;

				public RollBig(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o) : base(v, sv, t, nb, nss, nst, gg, o)
				{
				}

				public override double getLastTime()
				{
					return lastTime;
				}
				public override double getMoveLastPoint() { return moveLastPoint; }
				public override void setLast(double lt, double nb, double nss, double o)
				{
					lastTime = lt;
					lastDisplacement = nss * ((OldConfig.DrawSize.X - judgeJustPointX) / (60 * 240 / nb));
					lastPoint = nss * (lt - o) * ((OldConfig.DrawSize.X - judgeJustPointX) / (240 / nb)) / 1000;
				}
				public static void Initialize()
				{
					images = new NoteImage[5];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\rollBig_1_note.png"));
					images[3] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\rollBig_4_note.png"));
					images[4] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\rollbig_5_note.png"));
				}
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                    DX.DeleteGraph(images[3].handle);
                    DX.DeleteGraph(images[4].handle);
                }
                public override bool getCatchNote()
				{
					return false;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
					if (!draw)
					{
						drawNote = false;
					}
				}
				public override void updateCount()
				{
					count++;
				}

				public override void Update(int dec, double cX)
				{
					movePoint = dec + point * PlayConfig.Speed + displacement * PlayConfig.Speed * cX;
					moveLastPoint = dec + lastPoint * PlayConfig.Speed + lastDisplacement * PlayConfig.Speed * cX;
				}
				public override void Draw()
				{

					if (movePoint < 1280 + images[0].sizeX && moveLastPoint > 0)
					{
						DX.DrawExtendGraphF((float)movePoint - images[4].sizeX / 8, pointY - images[3].sizeY / 2, (float)moveLastPoint, pointY + images[3].sizeY / 2, images[3].handle, 1);
						DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
						DX.DrawRotaGraphF((float)moveLastPoint - images[4].sizeX / 8, pointY, 1.0, 0.0, images[4].handle, 1);
					}
				}
			}
			public class Balloon : Note
			{
				static NoteImage[] images;
				public static SPlayDxSound se;
				public override bool getCatchNote()
				{
					return catchNote;
				}
				public override void matchCatchNote(bool draw)
				{
					catchNote = true;
					if (!draw)
					{
						drawNote = false;
					}
				}
				public int balloonNum;
				public int nowBallonCount;
				public double lastTime;
				public double lastDisplacement;
				public double lastPoint;
				public double moveLastPoint;

				public Balloon(char v, char sv, double t, double nb, double nss, double nst, bool gg, double o, int bn) : base(v, sv, t, nb, nss, nst, gg, o)
				{
					balloonNum = bn;
					nowBallonCount = balloonNum;
				}
				public override double getLastTime()
				{
					return lastTime;
				}
				public override double getMoveLastPoint() { return moveLastPoint; }
				public override void setLast(double lt, double nb, double nss, double o)
				{
					lastTime = lt;
					lastDisplacement = nss * ((OldConfig.DrawSize.X - judgeJustPointX) / (60 * 240 / nb));
					lastPoint = nss * (lt - o) * ((OldConfig.DrawSize.X - judgeJustPointX) / (240 / nb)) / 1000;
				}
				public static void Initialize()
				{
					images = new NoteImage[4];
					images[0] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\balloon_1_note.png"));
					images[3] = new NoteImage(DX.LoadGraph(OldConfig.ImageFolder + @"\play\balloon_4_note.png"));
                    se = new SPlayDxSound().SetSound(Tone.BalloonPath);
                }
                public static void Finish()
                {
                    DX.DeleteGraph(images[0].handle);
                    DX.DeleteGraph(images[3].handle);
                }
                public override void updateBalloonNum()
				{
					nowBallonCount--;
				}
				public override int getBalloonNum()
				{
					return nowBallonCount;
				}
				public override void Update(int dec, double cX)
				{
					double buf = dec + point * PlayConfig.Speed + displacement * PlayConfig.Speed * cX;
					moveLastPoint = dec + lastPoint * PlayConfig.Speed + lastDisplacement * PlayConfig.Speed * cX;

					if (buf <= dec)
					{
						movePoint = dec;
						if (moveLastPoint < dec)
						{
							movePoint = moveLastPoint;
						}
					}
					else
					{
						movePoint = buf;
					}
				}
				public override void Draw()
				{
					switch (drawNote)
					{
						case true:
							if (movePoint < 1280 + images[0].sizeX && movePoint > 0)
							{
								DX.DrawRotaGraphF((float)movePoint, pointY, 1.0, 0.0, images[0].handle, 1);
								DX.DrawRotaGraphF((float)movePoint + images[0].sizeX, pointY, 1.0, 0.0, images[3].handle, 1);
							}
							break;
						default:
							break;
					}
				}
			}
			public class NoteImage
			{
				public int handle { get; private set; }
				public int sizeX;
				public int sizeY;
				public NoteImage(int h)
				{
					handle = h;
					GetGraphSize(handle, out sizeX, out sizeY);
				}
			}

			public string filePath { get; private set; }
			public string Title { get; private set; }
			public double BPM { get; private set; }
			public string Wave { get; private set; }
			public int WaveHandle { get; private set; }
			public double Offset { get; private set; }
			public long OffsetMilliseconds { get; private set; }
			public long LastTimeMiliseconds { get; private set; }
			public ECourseID CourseID { get; private set; }
			public int Level { get; private set; }
			public List<int> balloon;
			public int scoreinit { get; private set; }
			public int scorediff { get; private set; }
			public double goodMag { get; private set; }
			public double badMag { get; private set; }
			public int gauge { get; private set; }

			public Note[] Notes { get; private set; }
			public BarLine[] BarLines { get; private set; }
			public int measureNum { get; private set; }
            
            void SetTitle(string str)
            {
                Title = str;
            }
            void SetBPM(string str)
            {
                double b;
                double.TryParse(str, out b);
                if (b > 0) { BPM = b; }
                else { }
            }
            void SetWave(string str)
            {
                Wave = Path.GetDirectoryName(SongSelectPassData.FilePath) + @"\" + str;
                WaveHandle = LoadSoundMem(Wave);
            }
            void SetOffset(string str)
            {
                double o;
                double.TryParse(str, out o);
                Offset = o;
                OffsetMilliseconds = (long)(Offset * 1000);
            }
            void SetCourse(string str)
            {
                ECourseID courseBuf = ECourseID.Empty;
                switch (str.ToLower())
                {
                    case EASY_01:
                    case EASY_02:
                    case EASY_03:
                        courseBuf = ECourseID.Easy;
                        break;
                    case NORMAL_01:
                    case NORMAL_02:
                    case NORMAL_03:
                        courseBuf = ECourseID.Normal;
                        break;
                    case HARD_01:
                    case HARD_02:
                    case HARD_03:
                        courseBuf = ECourseID.Hard;
                        break;
                    case ONI_01:
                    case ONI_02:
                    case ONI_03:
                        courseBuf = ECourseID.Oni;
                        break;
                    case URA_01:
                    case URA_02:
                    case URA_03:
                        courseBuf = ECourseID.Ura;
                        break;
                    default:
                        MessageBox.Show("ファイルパス:" + filePath + "\n\n" + "無効なコース名を取得しました。\n" + "\n\nソフトを終了します。", "コースエラー", MessageBoxButtons.OK);
                        Environment.Exit(0);
                        break;
                }
                if (courseBuf == SongSelectPassData.CourseCurrent)
                {
                    CourseID = courseBuf;
                }
            }
            void SetLevel(string str)
            {
                if (CourseID != ECourseID.Empty)
                {
                    int l;
                    int.TryParse(str, out l);
                    Level = l;
                }
            }
            void SetBalloon(string str)
            {
                if (CourseID == SongSelectPassData.CourseCurrent)
                {
                    string[] ba = str.Split(',');
                    int bufInt;
                    foreach (var item in ba)
                    {
                        if (int.TryParse(item, out bufInt))
                        {
                            balloon.Add(bufInt);
                        }
                        else if (item == "")
                        {

                        }
                        else
                        {
                            MessageBox.Show("風船の設定に失敗しました。\n\n" + "\n\n選曲画面に戻ります。", "pts風船エラー", MessageBoxButtons.OK);
                            NScene.SetNext(Play, EScene.SongSelect);
                        }
                    }
                }
                else { }
            }
            void SetScoreInit(string str)
            {
                if (CourseID == SongSelectPassData.CourseCurrent)
                {
                    string[] si = str.Split(',');
                    int s;
                    if (!int.TryParse(si[0], out s))
                    {
                        scoreinit = 15;
                    }
                    else
                    {
                        scoreinit = s;
                    }
                }
            }
            void SetScoreDiff(string str)
            {
                if (CourseID == SongSelectPassData.CourseCurrent)
                {
                    string[] sd = str.Split(',');
                    int s;
                    if (!int.TryParse(sd[0], out s))
                    {
                        scorediff = 5;
                    }
                    else
                    {
                        scorediff = s;
                    }
                }
            }

            public void Dispose()
            {
                DeleteSoundMem(WaveHandle);
                Notes = null;
            }
            static Pts()
            {
                Tone = CTone.Get();
            }            
			public Pts()
			{
				noteCount = 0;
				balloon = new List<int>();
				List<Note> note = new List<Note>();
                List<BarLine> barLine = new List<BarLine>();
				maxCombo = 0;
				CourseID = ECourseID.Empty;
#if PLAYFIRST
                Program.filePathSelect = @"TJA\08_ナムコオリジナル\DON'T CUT.pts";
#endif
                #region PTS初期化
                #region 正規表現宣言
                TextExtra[] te = new TextExtra[]
                {
                    new TextExtra("TITLE:", SetTitle),
                    new TextExtra("BPM:", SetBPM),
                    new TextExtra("WAVE:", SetWave),
                    new TextExtra("OFFSET:", SetOffset),
                    new TextExtra("COURSE:", SetCourse),
                    new TextExtra("LEVEL:", SetLevel),
                    new TextExtra("BALLOON:", SetBalloon),
                    new TextExtra("SCOREINIT:", SetScoreInit),
                    new TextExtra("SCOREDIFF:", SetScoreDiff),
                    //new TextExtra("STYLE:", SetStyle),
                };                

				Regex r_sharpStart = new Regex("^#START");
				Regex r_sharpEnd = new Regex("^#END");

				Regex r_sharpN = new Regex("^#N");
				Regex r_sharpE = new Regex("^#E");
				Regex r_sharpM = new Regex("^#M");
				Regex r_sharpBranchEnd = new Regex("^#BRANCHEND");

				Regex r_sharpBpmchange = new Regex("^#BPMCHANGE");
				Regex r_sharpMeasure = new Regex("^#MEASURE");
				Regex r_sharpScrollSpeed = new Regex("^#SCROLL");
				Regex r_sharpBarLineON = new Regex("^#BARLINEON");
				Regex r_sharpBarLineOFF = new Regex("^#BARLINEOFF");
				Regex r_sharpGogoStart = new Regex("^#GOGOSTART");
				Regex r_sharpGogoEnd = new Regex("^#GOGOEND");
				#endregion
				StreamReader sr = new StreamReader(SongSelectPassData.FilePath, Encoding.GetEncoding("Shift_JIS"));
				string buf;
				bool breakPoint = false;

				while (!sr.EndOfStream && !breakPoint)
				{
					buf = sr.ReadLine();
                    if (te.SetString(buf)) { }
                    else if (r_sharpStart.IsMatch(buf))
                    {
                        if (CourseID == SongSelectPassData.CourseCurrent && note.Count == 0)
                        {
                            TextExtra[] commands = new TextExtra[]
                            {
                                //new TextExtra("#BPMCHANGE ", );
                            };
                            double prevTime = 0;
                            double measureSplitTime = 0;
                            double nowBPM = BPM;
                            double nowScrollSpeed = 1;
                            double nowMeasure = 4;
                            int bElem = 0;
                            bool nowBarDraw = true;
                            bool nowGogo = false;
                            char saveRoll = '-';
                            char branch = '-';

                            List<string> temp = new List<string>();

                            var numOnly = new Regex("([^0-9a-zA-Z]+)");

                            while (!r_sharpEnd.IsMatch(buf))
                            {
                                while (true)
                                {
                                    buf = sr.ReadLine();
                                    if (buf == "#END")
                                    {
                                        break;
                                    }                                
                                    if (buf != "")
                                    {
                                        temp.Add(buf);
                                        if (temp[temp.Count - 1][0] == '#')
                                        {
                                            continue;
                                        }
                                        temp[temp.Count - 1] = temp[temp.Count - 1].Split(' ')[0];
                                        temp[temp.Count - 1] = temp[temp.Count - 1].Split('/')[0];
                                        if (temp[temp.Count - 1][temp[temp.Count - 1].Length - 1] == ',')
                                        {
                                            break;
                                        }
                                    }
                                }
                                //1小節音符数
                                int noteNum = 0;
                                #region 1小節の音符数を取得
                                foreach (var item in temp)
                                {
                                    if (item[0] != '#')
                                    {
                                        noteNum += numOnly.Replace(item, "").Length;
                                    }
                                }
                                #endregion

                                if (noteNum == 0) noteNum = 1;
                                double tempPrevTime = prevTime;
                                measureSplitTime = (60000 * nowMeasure) / (nowBPM * noteNum);
                                bool bar = false;
                                foreach (var tempLine in temp)
                                {
                                    if (tempLine[0] == '#')
                                    {
                                        #region #系
                                        if (r_sharpBpmchange.IsMatch(tempLine))
                                        {
                                            nowBPM = double.Parse(r_sharpBpmchange.Replace(tempLine, ""));
                                            if (nowBPM > Note.initBpm)
                                            {
                                                Note.initBpm = nowBPM;
                                            }
                                            measureSplitTime = (60000 * nowMeasure) / (nowBPM * noteNum);
                                        }
                                        else if (r_sharpMeasure.IsMatch(tempLine))
                                        {
                                            string[] measureBuf = r_sharpMeasure.Replace(tempLine, "").Split('/');
                                            nowMeasure = (double)int.Parse(measureBuf[0]) * 4 / int.Parse(measureBuf[1]);
                                            measureSplitTime = (60000 * nowMeasure) / (nowBPM * noteNum);
                                        }
                                        else if (r_sharpScrollSpeed.IsMatch(tempLine))
                                        {
                                            nowScrollSpeed = double.Parse(r_sharpScrollSpeed.Replace(tempLine, ""));
                                        }
                                        else if (r_sharpBarLineOFF.IsMatch(tempLine))
                                        {
                                            nowBarDraw = false;
                                        }
                                        else if (r_sharpBarLineON.IsMatch(tempLine))
                                        {
                                            nowBarDraw = true;
                                        }
                                        else if (r_sharpN.IsMatch(tempLine))
                                        {
                                            branch = 'N';
                                        }
                                        else if (r_sharpE.IsMatch(tempLine))
                                        {
                                            branch = 'E';
                                        }
                                        else if (r_sharpM.IsMatch(tempLine))
                                        {
                                            branch = 'M';
                                        }
                                        else if (r_sharpBranchEnd.IsMatch(tempLine))
                                        {
                                            branch = '-';
                                        }
                                        else if (r_sharpGogoStart.IsMatch(tempLine))
                                        {
                                            nowGogo = true;
                                        }
                                        else if (r_sharpGogoEnd.IsMatch(tempLine))
                                        {
                                            nowGogo = false;
                                        }
                                        else if (r_sharpEnd.IsMatch(tempLine))
                                        {
                                            break;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        if (!bar)
                                        {
                                            barLine.Add(new BarLine(nowBarDraw, prevTime + HeadBrankTime, nowBPM, nowScrollSpeed, OffsetMilliseconds));
                                            bar = true;
                                        }
                                        string reTempLine = tempLine;
                                        // #region 音符の初期化
                                        switch (branch)
                                        {
                                            case '-':
                                            case 'M':
                                                foreach (var tempChar in reTempLine)
                                                {
                                                    switch (tempChar)
                                                    {
                                                        case '0':
                                                        case ',':
                                                            break;
                                                        case '1':
                                                            #region ドン
                                                            char c = tempChar;

                                                            int rp = GetRand(99) + 1;
                                                            if (PlayConfig.ReplacePer < rp)
                                                            {
                                                                c = '1';
                                                            }
                                                            else
                                                            {
                                                                c = '2';
                                                            }
                                                            switch (c)
                                                            {
                                                                case '1':
                                                                    note.Add(new Don(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                                case '2':
                                                                    note.Add(new Kat(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                            }
                                                            maxCombo++;
                                                            #endregion
                                                            break;
                                                        case '2':
                                                            #region カツ
                                                            c = tempChar;
                                                            rp = GetRand(99) + 1;
                                                            if (PlayConfig.ReplacePer < rp)
                                                            {
                                                                c = '2';
                                                            }
                                                            else
                                                            {
                                                                c = '1';
                                                            }
                                                            switch (c)
                                                            {
                                                                case '1':
                                                                    note.Add(new Don(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                                case '2':
                                                                    note.Add(new Kat(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                            }
                                                            maxCombo++;
                                                            #endregion
                                                            break;
                                                        case '3':
                                                            #region ドン(大)
                                                            c = tempChar;
                                                            rp = GetRand(99) + 1;
                                                            if (PlayConfig.ReplacePer < rp)
                                                            {
                                                                c = '3';
                                                            }
                                                            else
                                                            {
                                                                c = '4';
                                                            }
                                                            switch (c)
                                                            {
                                                                case '3':
                                                                    note.Add(new DonBig(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                                case '4':
                                                                    note.Add(new KatBig(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                            }
                                                            maxCombo++;
                                                            #endregion
                                                            break;
                                                        case '4':
                                                            #region カツ(大)
                                                            c = tempChar;
                                                            rp = GetRand(99) + 1;
                                                            if (PlayConfig.ReplacePer < rp)
                                                            {
                                                                c = '4';
                                                            }
                                                            else
                                                            {
                                                                c = '3';
                                                            }
                                                            switch (c)
                                                            {
                                                                case '3':
                                                                    note.Add(new DonBig(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                                case '4':
                                                                    note.Add(new KatBig(c, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                                    break;
                                                            }
                                                            maxCombo++;
                                                            #endregion
                                                            break;
                                                        case '5':
                                                            #region 連打
                                                            note.Add(new Roll(tempChar, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                            saveRoll = '5';
                                                            #endregion
                                                            break;
                                                        case '6':
                                                            #region 連打(大)
                                                            note.Add(new RollBig(tempChar, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds));
                                                            #endregion
                                                            break;
                                                        case '7':
                                                            #region 風船
                                                            if (balloon.Count == 0)
                                                            {
                                                                MessageBox.Show("風船音符の連打数データが演奏時の風船回数よりも少ないため、初期化できません。\n\n選曲画面へ戻ります。", "pts風船エラー", MessageBoxButtons.OK);
                                                                NScene.SetNext(Play, EScene.SongSelect);
                                                            }
                                                            else
                                                            {
                                                                note.Add(new Balloon(tempChar, saveRoll, tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, nowMeasure, nowGogo, OffsetMilliseconds, balloon[bElem]));
                                                                bElem++;
                                                            }
                                                            #endregion
                                                            break;
                                                        case '8':
                                                            #region 終了
                                                            note[note.Count - 1].setLast(tempPrevTime + HeadBrankTime, nowBPM, nowScrollSpeed, OffsetMilliseconds);
                                                            saveRoll = '-';
                                                            #endregion
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                    if (note.Count > 0 && note[note.Count - 1].point < 1600)
                                                    {
                                                        noteCount = note.Count - 1;
                                                    }
                                                    if (tempChar != ',' || reTempLine == ",")
                                                    {
                                                        tempPrevTime = measureSplitTime + tempPrevTime;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    //measureNum += Origin.CountChar(tempLine, ',');
                                }
                                prevTime = prevTime + (60000 * nowMeasure) / (nowBPM);
                                LastTimeMiliseconds = (long)prevTime;
                                temp.RemoveRange(0, temp.Count);
                            }

                        }
                    }
				}
				sr.Close();
				for (int i = 0; i < note.Count; i++)
				{
					note[i].setEffectDisplacement();
				}
				switch (CourseID)
				{
					case ECourseID.Easy:
						gauge = Convert.ToInt32(10000 / (maxCombo * 0.60));
						break;
					case ECourseID.Normal:
						gauge = Convert.ToInt32(10000 / (maxCombo * 0.65));
						break;
					case ECourseID.Hard:
						gauge = Convert.ToInt32(10000 / (maxCombo * 0.70));
						break;
					case ECourseID.Oni:
						gauge = Convert.ToInt32(10000 / (maxCombo * 0.75));
						break;
					case ECourseID.Ura:
						gauge = Convert.ToInt32(10000 / (maxCombo * 0.75));
						break;
				}
				#endregion
				switch (CourseID)
				{
					case ECourseID.Easy:
						goodMag = (double)3 / 4;
						badMag = (double)-1 / 2;
						break;
					case ECourseID.Normal:
						goodMag = (double)3 / 4;
						badMag = -1;
						break;
					case ECourseID.Hard:
						goodMag = (double)3 / 4;
						badMag = (double)-5 / 4;
						break;
					case ECourseID.Oni:
					case ECourseID.Ura:
						goodMag = (double)1 / 2;
						badMag = -2;
						break;
				}
                this.Notes = note.ToArray();
                this.BarLines = barLine.ToArray();
			}
		}
		Pts pts;

		public class BgImage_
		{
			Image bg;
			Image bg_clear;
			int alpha = 0;
			int alphaDiff = 20;
			public BgImage_()
			{
				bg = new Image(OldConfig.ImageFolder + @"\play\bg.png");
				bg_clear = new Image(OldConfig.ImageFolder + @"\play\bg_clear.png");
			}
			public void Draw()
			{
				if (PDPlayPassData.clear == true)
				{
					alpha += alphaDiff;
					if (alpha > 255)
					{
						alpha = 255;
					}
				}
				else
				{
					alpha -= alphaDiff;
					if (alpha < 0)
					{
						alpha = 0;
					}
				}
				bg.Draw();
				DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, alpha);

				bg_clear.Draw();
				DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 0);
			}
		}
		public BgImage_ bg;
		public Image bgChar;

		public Image jP;
		public Image jPGoGo;
		public Image taiko_p1;
        //public Image player_p1;

        SPlayDxImage Combo_1;
        SPlayDxImage Combo_2;

        public class JudgementStringImage : Image
		{
			//public int moveY;
			public int alpha;
			public void setAlpha(int a = 0)
			{
				alpha = a;
				moveY = 6;
			}
			public override void Draw()
			{
				DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, alpha);
				DX.DrawExtendGraph(x, y - moveY, x + sizeX, y - moveY + sizeY, handle, 1);
				DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255);
				if (moveY > 0)
				{
					moveY--;
				}
				if (alpha > 0)
				{
					alpha -= 20;
				}
			}
			public JudgementStringImage(string fp) : base(fp)
			{
				x = judgeJustPointX - sizeX / 2;
				y = Pts.Note.pointY - sizeY * 2;
			}
		}
		public JudgementStringImage judgeGreat;
		public JudgementStringImage judgeGood;
		public JudgementStringImage judgeBad;

        public class Roll : Image, IDisposable
        {
            public int font;
            public int fontSize;
            public int num { get; private set; }
            public int sum { get; private set; }
            public int value;
            public int valueDiff = 40;
            public void updateNum()
            {
                if (value < 1200)
                {
                    num = 0;
                }
                num++;
                sum++;
                if (num > 0)
                {
                    value = int.MaxValue;
                }
            }
            public void resetNum()
            {
                if (value > 1200)
                {
                    value = 1200;
                }
            }
            public new void Draw()
            {
                if (value > 0)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, value);
                    int width = DX.GetDrawStringWidthToHandle(num.ToString(), num.ToString().Length, font);
                    base.Draw();
                    DX.DrawStringToHandle(x + sizeX / 2 - width / 2, y + sizeY / 2 - fontSize / 2 - 16, num.ToString(), DX.GetColor(255, 255, 255), font, DX.GetColor(0, 0, 0));
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255);
                    value -= valueDiff;
                }
                else
                {
                    num = 0;
                }
            }

            public void Dispose()
            {
                DeleteFontToHandle(font);
            }

            public Roll(string fp) : base(fp)
            {
                fontSize = 80;
                font = DX.CreateFontToHandle("ＤＦＰ勘亭流", fontSize, -1, DX.DX_FONTTYPE_ANTIALIASING_4X4, -1, 3);
            }
        }
		public Roll roll;

		public class Balloon : Image,IDisposable
		{
			public int font;
			public int fontSize;
			public List<int> num;
			public List<bool> catchNote;
			public int elem;
			public int value;
			public int valueDiff = 40;
			public void updateNum(int e, double g)
			{
				if (value < 1200)
				{
					elem = e;
				}

				num[elem]--;

				if (num[elem] > 0)
				{
					PDPlayPassData.addScore((int)(300 * g));
				}
				else if (num[elem] == 0)
				{
					PDPlayPassData.addScore((int)(5000 * g));
				}
				value = int.MaxValue;
			}
			public void resetNum()
			{
				if (value > 1200)
				{
					value = 1200;
				}
			}
			public void updateCatchNote()
			{
				catchNote[elem] = true;
			}
			public new void Draw()
			{
				if (num.Count > 0)
				{
					if (value > 0 && num[elem] > 0)
					{
						DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, value);
						int width = DX.GetDrawStringWidthToHandle(num[elem].ToString(), num[elem].ToString().Length, font);
						base.Draw();
						DX.DrawStringToHandle(x + sizeX / 2 - width / 2, y + sizeY / 2 - fontSize / 2, num[elem].ToString(), DX.GetColor(255, 255, 255), font, DX.GetColor(0, 0, 0));
						DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255);
						value -= valueDiff;
					}
					else if (num[elem] <= 0)
					{
						value = 0;
					}
				}
			}
			public bool getCatchNote(int e)
			{
				if (catchNote[e] == true)
				{
					return true;
				}
				if (num[e] <= 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			public int getNum(int e)
			{
				return num[e];
			}

            public void Dispose()
            {
                DeleteFontToHandle(font);
            }

            public Balloon(string fp, List<int> n) : base(fp)
			{
				fontSize = 60;
				font = DX.CreateFontToHandle("ＤＦＰ勘亭流", fontSize, -1, DX.DX_FONTTYPE_ANTIALIASING_4X4, -1, 3);
				num = n;
				catchNote = new List<bool>();
				for (int i = 0; i < num.Count; i++)
				{
					catchNote.Add(false);
				}
				elem = 0;
			}

		}
		public Balloon balloon;

		public Image soul;
		//public Image gaugeClearPiece1;
		//public Image gaugeClearPiece2;
        
		double bufX;
		bool musicStart;
		bool noteCatch;
		long nowTime;
		double noteTime;

		int songTitleEndPointX;
		int judgeImageValue = 500;
		int noteStart;

		bool nowGogo;

		public void noteUpdate(int elem, int big = 1)
		{
			if (!noteCatch)
			{
				int eval = 0;
				bool hit = false;
				if (!pts.Notes[elem].catchNote)
				{
					if (noteTime > nowTime - PlayConfig.judgeRangeGreat && noteTime < nowTime + PlayConfig.judgeRangeGreat)
					{
						noteCatch = true;
						pts.Notes[elem].matchCatchNote(false);
						PDPlayPassData.addGreatSum();
						PDPlayPassData.addCombo();
						PDPlayPassData.setMaxCombo();
						judgeGreat.setAlpha(judgeImageValue);
						judgeGood.setAlpha();
						judgeBad.setAlpha();
						PDPlayPassData.addGauge(1, pts.gauge);
						NoteFieldImage.setHit(true);
						EffectNoteList.Set(pts.Notes[elem].NoteID, pts.Notes[elem].effectDisplacement);
						hit = true;
						eval = 1;
					}
					else if (noteTime > nowTime - PlayConfig.judgeRangeGood && noteTime < nowTime + PlayConfig.judgeRangeGood)
					{
						noteCatch = true;
						pts.Notes[elem].matchCatchNote(false);
						PDPlayPassData.addGoodSum();
						PDPlayPassData.addCombo();
						PDPlayPassData.setMaxCombo();
						judgeGreat.setAlpha();
						judgeGood.setAlpha(judgeImageValue);
						judgeBad.setAlpha();
						PDPlayPassData.addGauge(pts.goodMag, pts.gauge);
						NoteFieldImage.setHit(true);
						EffectNoteList.Set(pts.Notes[elem].NoteID, pts.Notes[elem].effectDisplacement);
						hit = true;
						eval = 2;
					}
					else if (noteTime > nowTime - PlayConfig.judgeRangeBad && noteTime < nowTime + PlayConfig.judgeRangeBad)
					{
						noteCatch = true;
						pts.Notes[elem].matchCatchNote(false);
						PDPlayPassData.addBadSum();
						PDPlayPassData.resetCombo();
						judgeGreat.setAlpha();
						judgeGood.setAlpha();
						judgeBad.setAlpha(judgeImageValue);
						PDPlayPassData.addGauge(pts.badMag, pts.gauge);
					}
					if (hit)
					{
						int comboBonus;
						int score;
						if (PDPlayPassData.combo < 10)
						{
							comboBonus = 0;
						}
						else if (PDPlayPassData.combo < 30)
						{
							comboBonus = 1;
						}
						else if (PDPlayPassData.combo < 50)
						{
							comboBonus = 2;
						}
						else if (PDPlayPassData.combo < 100)
						{
							comboBonus = 4;
						}
						else
						{
							comboBonus = 8;
						}
						score = (int)((pts.scoreinit + pts.scorediff * comboBonus) / 10) * 10;
						score = (int)(score * pts.Notes[elem].getGogoValue() / 10 / eval * big) * 10;
						PDPlayPassData.addScore(score);
						if (PDPlayPassData.combo > 0 && PDPlayPassData.combo % 100 == 0)
						{
							PDPlayPassData.addScore(10000);
						}
					}
				}
			}
		}

		CSceneIn sceneIn;
		ResultData resultData;
        SPlayDxFont FontScore;
        SPlayDxFont FontTitle;
        SPlayDxFont FontCombo;

		long ts;
        const string FONTNAME = "ＤＦＰ勘亭流";
		public static List<string> measureNoteFormat(string n)
		{
			List<string> str;
			str = new List<string>(n.Split(','));
			str.RemoveAt(str.Count - 1);
			return str;
		}

        class BgImageControl:IGameAction
        {
            static BgImageControl Own;
            public static void Set(XElement e)
            {
                Own = new BgImageControl(e);
            }
            BgImageControl(XElement e)
            {
                AddAction(this);
            }
            void IGameAction.Update()
            {
            }
            void IGameAction.Draw()
            {
            }
            void IGameAction.Finish()
            {

            }
        }

        class TaikoImageControl : IGameAction
        {
            static TaikoImageControl Own;
            class TaikoImage
            {
                const int L = 0;
                const int R = 1;
                const int MAX = 8;

                const string TAIKO = @"Taiko.png";
                const string DON = @"Don.png";
                const string KAT = @"Kat.png";

                protected SPlayDxImage Taiko;
                protected SPlayDxImage Don;
                protected SPlayDxImage Kat;

                int[] DonDrawCount;
                int[] KatDrawCount;

                int PointX;
                int PointY;

                public void SetImage(string folder)
                {
                    Taiko.SetImage(folder + TAIKO);
                    Don.SetImage(folder + DON, 2, 1);
                    Kat.SetImage(folder + KAT, 2, 1);
                }
                public void SetPoint(int x, int y)
                {
                    PointX = x;
                    PointY = y;
                }

                public void Update()
                {
                    if (Key.GetCount(Key.F) == 1) { DonDrawCount[L] = MAX; }
                    if (Key.GetCount(Key.J) == 1) { DonDrawCount[R] = MAX; }

                    if (Key.GetCount(Key.D) == 1) { KatDrawCount[L] = MAX; }
                    if (Key.GetCount(Key.K) == 1) { KatDrawCount[R] = MAX; }
                }
                public void Draw()
                {
                    Taiko.Draw(PointX, PointY);

                    if (DonDrawCount[L] > 0)
                    {
                        Don.Draw(PointX, PointY, L);
                        DonDrawCount[L]--;
                    }
                    if (DonDrawCount[R] > 0)
                    {
                        Don.Draw(PointX + Don.Width, PointY, R);
                        DonDrawCount[R]--;
                    }
                    if (KatDrawCount[L] > 0)
                    {
                        Kat.Draw(PointX, PointY, L);
                        KatDrawCount[L]--;
                    }
                    if (KatDrawCount[R] > 0)
                    {
                        Kat.Draw(PointX + Kat.Width, PointY, R);
                        KatDrawCount[R]--;
                    }
                }
                public TaikoImage()
                {
                    Taiko = new SPlayDxImage();
                    Don = new SPlayDxImage();
                    Kat = new SPlayDxImage();

                    DonDrawCount = Enumerable.Repeat(0, 2).ToArray();
                    KatDrawCount = Enumerable.Repeat(0, 2).ToArray();
                }
            }
            TaikoImage P1;
            
            const string E_FOLDER = @"folder";
            const string E_TYPE = @"type";
            const string E_POINT = @"point";

            const string A_VALUE = @"value";
            const string A_P1 = @"P1";
            const string A_X = @"x";
            const string A_Y = @"y";

            public static void Set(XElement e)
            {
                Own = new TaikoImageControl(e);
            }
            TaikoImageControl(XElement e)
            {
                P1 = new TaikoImage();
                string Folder = "";
                if ((string)e.Attribute(E_FOLDER) != null)
                {
                    #region folder
                    Folder = e.Attribute(E_FOLDER).Value;
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
                foreach(var t in e.Elements())
                {
                    switch (t.Name.LocalName)
                    {
                        case E_TYPE:
                            string tFolder = "";
                            if ((string)t.Attribute(E_FOLDER) != null)
                            {
                                #region tfolder
                                tFolder = t.Attribute(E_FOLDER).Value;
                                if (tFolder != "")
                                {
                                    switch (tFolder.Replace('/', '\\').Last())
                                    {
                                        case '\\':
                                            break;
                                        default:
                                            tFolder += '\\';
                                            break;
                                    }
                                }
                                P1.SetImage(MainConfig.Play + Folder + tFolder);
                                #endregion
                            }
                            if ((string)t.Attribute(A_VALUE) != null)
                            {
                                switch (t.Attribute(A_VALUE).Value)
                                {
                                    case A_P1:
                                        if ((string)t.Element(E_POINT) != null)
                                        {
                                            int x = 0;
                                            int y = 0;
                                            foreach (var p in t.Element(E_POINT).Attributes())
                                            {
                                                switch (p.Name.LocalName)
                                                {
                                                    case A_X:
                                                        int.TryParse(p.Value, out x);
                                                        break;
                                                    case A_Y:
                                                        int.TryParse(p.Value, out y);
                                                        break;
                                                }
                                            }
                                            P1.SetPoint(x, y);
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                AddAction(this);
            }

            void IGameAction.Update()
            {
                P1.Update();
            }
            void IGameAction.Draw()
            {
                P1.Draw();
            }
            void IGameAction.Finish()
            {
            }
        }
        public override void Start()
		{
            GameActionControl = CGameActionControl.Construct(this);
            AddAction = GameActionControl.AddAction;

            Config = CConfig.Construct(this);

            //Relation

            Config.Start();

            Debug = new SPlayDebug();
            
			EffectNoteList = new CEffectNoteList();
            
			ImagesInitialize();

			Pts.noteCount = 0;
			noteStart = 0;
			Pts.barLineCount = 0;
			Pts.Don.Initialize();
			Pts.Kat.Initialize();
			Pts.DonBig.Initialize();
			Pts.KatBig.Initialize();
			Pts.Roll.Initialize();
			Pts.RollBig.Initialize();
			Pts.Balloon.Initialize();

			Pts.BarLine.Initialize();

			PDPlayPassData.resetResult();
			switch (SongSelectPassData.CourseCurrent)
			{
				case ECourseID.Easy:
					PDPlayPassData.clearGauge = 200 * 30;
					break;
				case ECourseID.Normal:
				case ECourseID.Hard:
					PDPlayPassData.clearGauge = 200 * 35;
					break;
				case ECourseID.Oni:
				case ECourseID.Ura:
					PDPlayPassData.clearGauge = 200 * 40;
					break;
			}
			resultData = new ResultData();

            FontScore = new SPlayDxFont().SetFont(FONTNAME, 30);
            FontScore.SetSpace(-1);

            FontTitle = new SPlayDxFont().SetFont(FONTNAME, 48, 3);

            FontCombo = new SPlayDxFont().SetFont(FONTNAME, 48, 3);
            FontCombo.SetSpace(-2);
            
			musicStart = false;

			pts = new Pts();

			sceneIn = new CSceneIn(MainConfig.SongSelect + @"scene.png", -22);

            int tempTitleWidth = FontTitle.Width(pts.Title);
            if (tempTitleWidth > 420)
            {
                FontTitle.SetExRate((double)420 / FontTitle.Width(pts.Title), 1.0);
                tempTitleWidth = FontTitle.Width(pts.Title);
            }
            songTitleEndPointX = 1210 - tempTitleWidth;

			bg = new BgImage_();
			bgChar = new Image(MainConfig.Play + @"bg_char.png");

			//
			jP = new Image(MainConfig.Play+ @"judgePoint.png");
			jP.setPoint(judgeJustPointX, Pts.Note.pointY);

			jPGoGo = new Image(OldConfig.ImageFolder + @"\play\judgePointGOGO.png");
			jPGoGo.setPoint(judgeJustPointX, Pts.Note.pointY);

			//
			taiko_p1 = new Image(OldConfig.ImageFolder + @"\play\taiko_p1.png");
			taiko_p1.setPoint(0, 178);

			//
			judgeGreat = new JudgementStringImage(OldConfig.ImageFolder + @"\play\judgement\great.png");
			judgeGood = new JudgementStringImage(OldConfig.ImageFolder + @"\play\judgement\good.png");
			judgeBad = new JudgementStringImage(OldConfig.ImageFolder + @"\play\judgement\bad.png");

			//
			soul = new Image(OldConfig.ImageFolder + @"\play\soul.png");

			//
			roll = new Roll(OldConfig.ImageFolder + @"\play\roll.png");
			roll.setPoint(180, 0);

			//
			balloon = new Balloon(OldConfig.ImageFolder + @"\play\balloon.png", pts.balloon);
			balloon.setPoint(440, 21);

			bufX = 1;

            Combo_1 = new SPlayDxImage().SetImage(MainConfig.Play + "combo_1.png");
            Combo_1.SetCenterPoint(256, 278);

            Combo_2 = new SPlayDxImage().SetImage(MainConfig.Play + "combo_2.png");
            Combo_2.SetCenterPoint(256, 278);

            MyStopWatch = new CMyStopwatch();
            MyStopWatch.Start();
            State = EState.OK;
		}

		public override void Update()
		{
			if (State == EState.Null)
			{
				Start();
			}
            ts = MyStopWatch.TotalMillisecond;
            bufX = (double)-ts * 60 / 1000;
            nowTime = ts + pts.OffsetMilliseconds;

            if (pts.LastTimeMiliseconds < nowTime)
			{
				if (CheckSoundMem(pts.WaveHandle) == 0)
				{
					NScene.SetNext(this, EScene.Result);                    
				}
			}
			if (!musicStart && ts > HeadBrankTime)
			{
				PlaySoundMem(pts.WaveHandle, DX_PLAYTYPE_BACK);
				musicStart = true;
			}
			for (int i = 0; i < pts.BarLines.Length; i++)
			{
				pts.BarLines[i].Update(judgeJustPointX, bufX);
			}
			noteCatch = false;
			nowGogo = false;
			int balloonElem = 0;

			//関数でforする                                
			for (int i = 0; i < pts.Notes.Length; i++)
			{
				pts.Notes[i].Update(judgeJustPointX, bufX);
				noteTime = pts.Notes[i].Time;
				switch (PlayConfig.Auto)
				{
					case true:
						#region オート
						if (noteTime < nowTime)
						{
							switch (pts.Notes[i].value)
							{
								case '1':
									if (pts.Notes[i].catchNote == false)
									{
										noteUpdate(i);
										Pts.Don.se.Play();
									}
									break;
								case '2':
									if (pts.Notes[i].catchNote == false)
									{
										noteUpdate(i);
										Pts.Kat.se.Play();
									}
									break;
								case '3':
									if (pts.Notes[i].catchNote == false)
									{
										noteUpdate(i, 2);
										Pts.Don.se.Play();
									}
									break;
								case '4':
									if (pts.Notes[i].catchNote == false)
									{
										noteUpdate(i, 2);
										Pts.Kat.se.Play();
									}
									break;
								case '5':
									if (pts.Notes[i].getLastTime() > ts + pts.OffsetMilliseconds)
									{
										if ((int)bufX % 2 == 0)
										{
											Pts.Don.se.Play();
											EffectNoteList.Set('1', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(100 * pts.Notes[i].getGogoValue()));
											pts.Notes[i].updateCount();
										}
									}
									break;
								case '6':
									if (pts.Notes[i].getLastTime() > ts + pts.OffsetMilliseconds)
									{
										if ((int)bufX % 2 == 0)
										{
											Pts.Don.se.Play();
											EffectNoteList.Set('3', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(200 * pts.Notes[i].getGogoValue()));
											pts.Notes[i].updateCount();
										}
									}
									break;
								case '7':
									if (pts.Notes[i].getLastTime() > ts + pts.OffsetMilliseconds)
									{
										if (pts.Notes[i].getCatchNote() == false)
										{
											if ((int)bufX % 2 == 0)
											{
												Pts.Don.se.Play();
												pts.Notes[i].updateBalloonNum();
												if (pts.Notes[i].getBalloonNum() > 0)
												{
													PDPlayPassData.addScore((int)(300 * pts.Notes[i].getGogoValue()));
												}
												else if (pts.Notes[i].getBalloonNum() == 0)
												{
													PDPlayPassData.addScore((int)(5000 * pts.Notes[i].getGogoValue()));
													Pts.Balloon.se.Play();
													pts.Notes[i].matchCatchNote(false);
												}
											}
										}
									}
									break;
							}
						}
						if (!pts.Notes[i].catchNote && noteTime < nowTime - PlayConfig.judgeRangeBad)
						{
							switch (pts.Notes[i].value)
							{
								case '1':
								case '2':
								case '3':
								case '4':
									pts.Notes[i].matchCatchNote(true);
									PDPlayPassData.resetCombo();
									PDPlayPassData.addBadSum();
									PDPlayPassData.addGauge(pts.badMag, pts.gauge);
									break;
							}
						}
						#endregion
						break;
					case false:
						#region プレイ
						switch (pts.Notes[i].value)
						{
							case '1':
								if (Key.GetCount(Key.F) == 1 || Key.GetCount(Key.J) == 1)
								{
									noteUpdate(i);
								}
								break;
							case '2':
								if (Key.GetCount(Key.D) == 1 || Key.GetCount(Key.K) == 1)
								{
									noteUpdate(i);
								}
								break;
							case '3':
								if (Key.GetCount(Key.F) == 1 || Key.GetCount(Key.J) == 1)
								{
									noteUpdate(i, 2);
								}
								break;
							case '4':
								if (Key.GetCount(Key.D) == 1 || Key.GetCount(Key.K) == 1)
								{
									noteUpdate(i, 2);
								}
								break;
							case '5':
								if (!pts.Notes[i].catchNote)
								{
									if (pts.Notes[i].Time < nowTime && pts.Notes[i].getLastTime() > nowTime)
									{
										if (Key.GetCount(Key.F) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('1', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(100 * pts.Notes[i].getGogoValue()));
										}
										if (Key.GetCount(Key.J) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('1', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(100 * pts.Notes[i].getGogoValue()));
										}
										if (Key.GetCount(Key.D) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('2', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(100 * pts.Notes[i].getGogoValue()));
										}
										if (Key.GetCount(Key.K) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('2', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(100 * pts.Notes[i].getGogoValue()));
										}
									}
									else if (pts.Notes[i].getLastTime() <= nowTime)
									{
										roll.resetNum();
										pts.Notes[i].matchCatchNote(true);
									}
								}
								break;
							case '6':
								if (!pts.Notes[i].catchNote)
								{
									if (pts.Notes[i].Time < nowTime && pts.Notes[i].getLastTime() > nowTime)
									{
										if (Key.GetCount(Key.F) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('3', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(200 * pts.Notes[i].getGogoValue()));
										}
										if (Key.GetCount(Key.J) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('3', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(200 * pts.Notes[i].getGogoValue()));
										}
										if (Key.GetCount(Key.D) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('4', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(200 * pts.Notes[i].getGogoValue()));
										}
										if (Key.GetCount(Key.K) == 1)
										{
											roll.updateNum();
											EffectNoteList.Set('4', pts.Notes[i].effectDisplacement);
											PDPlayPassData.addRollSum();
											PDPlayPassData.addScore((int)(200 * pts.Notes[i].getGogoValue()));
										}
									}
									else if (pts.Notes[i].getLastTime() <= nowTime)
									{
										roll.resetNum();
										pts.Notes[i].matchCatchNote(true);
									}
								}
								break;
							case '7':
								if (!pts.Notes[i].catchNote)
								{
									if (pts.Notes[i].Time < nowTime && pts.Notes[i].getLastTime() > nowTime)
									{
										if (Key.GetCount(Key.F) == 1)
										{
											balloon.updateNum(balloonElem, pts.Notes[i].getGogoValue());
										}
										if (Key.GetCount(Key.J) == 1)
										{
											balloon.updateNum(balloonElem, pts.Notes[i].getGogoValue());
										}
										if (balloon.getCatchNote(balloonElem))
										{
											pts.Notes[i].matchCatchNote(false);
										}
										if (balloon.getNum(balloonElem) <= 0)
										{
											EffectNoteList.Set('3', pts.Notes[i].effectDisplacement);
											Pts.Balloon.se.Play();
										}
									}
									else if (pts.Notes[i].getLastTime() <= nowTime)
									{
										pts.Notes[i].matchCatchNote(true);
										balloon.updateCatchNote();
										balloon.resetNum();
									}
								}
								balloonElem = (balloonElem + 1) % pts.balloon.Count;
								break;
						}
						if (!pts.Notes[i].catchNote && noteTime < nowTime - PlayConfig.judgeRangeBad)
						{
							switch (pts.Notes[i].value)
							{
								case '1':
								case '2':
								case '3':
								case '4':
									pts.Notes[i].matchCatchNote(true);
									PDPlayPassData.resetCombo();
									PDPlayPassData.addBadSum();
									PDPlayPassData.addGauge(pts.badMag, pts.gauge);
									break;
							}
						}
						#endregion
						break;
				}
				if (noteTime < nowTime)
				{
					nowGogo = pts.Notes[i].gogo;
				}
				if (pts.Notes[i].movePoint < 1600)
				{
					Pts.noteCount = i;
				}
				if (pts.Notes[i].movePoint < 0)
				{
					double gmlp = pts.Notes[i].getMoveLastPoint();
					if (gmlp == -1)
					{
						noteStart = i + 1;
					}
					else
					{
						if (gmlp < 0)
						{
							noteStart = i + 1;
						}
					}
				}
				if (pts.Notes[i].movePoint > 1920)
				{
					break;
				}
			}
			if (!PlayConfig.Auto)
			{
				if (Key.GetCount(Key.F) == 1 || Key.GetCount(Key.J) == 1)
				{
					Pts.Don.se.Play();
				}
				if (Key.GetCount(Key.D) == 1 || Key.GetCount(Key.K) == 1)
				{
					Pts.Kat.se.Play();
				}
			}
			if (Key.GetCount(Key.Escape) == 1)
			{
				NScene.SetNext(this, EScene.SongSelect);
			}
			EffectNoteList.Update();
			foreach (var item in images)
			{
				item.Update();
			}
            GameActionControl.Update();
		}
		public override void Draw()
		{
			bg.Draw();
			foreach (var item in images)
			{
				item.Draw();
			}
			bgChar.Draw(0, 0);
			if (nowGogo)
			{
				jPGoGo.Draw('m');
			}
			else
			{
				jP.Draw('m');
			}
			DX.SetDrawMode(DX.DX_DRAWMODE_BILINEAR);
			for (int i = 0; i < pts.BarLines.Length; i++)
			{
				pts.BarLines[i].Draw();
			}
            if (!PlayConfig.NoteOff)
            {
                for (int i = Pts.noteCount; i >= noteStart; i--)
                {
                    pts.Notes[i].Draw();
                }
            }
			DX.SetDrawMode(DX.DX_DRAWMODE_NEAREST);
			taiko_p1.Draw();

            GameActionControl.Draw();
			//player[Player.elem].Draw(210, 235, 96, 3, DX.GetColor(32, 32, 32));

			string str = "" + PDPlayPassData.combo;
			if (PDPlayPassData.combo >= 10 && PDPlayPassData.combo < 100)
			{
                FontCombo.Draw(255 - FontCombo.Width(str) / 2, 232 - FontCombo.Size / 2, str, DX.GetColor(255, 255, 255),DX.GetColor(0,0,0));
                Combo_1.Draw();
            }
			else if (PDPlayPassData.combo >= 100)
            {
                FontCombo.Draw(255 - FontCombo.Width(str) / 2, 232 - FontCombo.Size / 2, str, DX.GetColor(255, 165, 0), DX.GetColor(0, 0, 0));
                Combo_2.Draw();
            }

            DX.SetDrawMode(DX.DX_DRAWMODE_BILINEAR);
            FontTitle.Draw(songTitleEndPointX, 35, pts.Title, DX.GetColor(255, 255, 255));
            DX.SetDrawMode(DX.DX_DRAWMODE_NEAREST);

            FontScore.Draw(165 - FontScore.Width(PDPlayPassData.score.ToString()), 190, PDPlayPassData.score.ToString(), DX.GetColor(255, 255, 255));
            
			EffectNoteList.Draw();
			roll.Draw();
			balloon.Draw();
            #region デバッグ
            //if (MainConfig.Debug)
            //{
            //	DX.DrawStringToHandle(0, 0, "タイトル:" + pts.title, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18, "難易度:" + pts.course, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 2, "コンボ数:" + PDPlayPassData.combo, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 3, "最大コンボ数:" + PDPlayPassData.maxCombo, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 4, "連打数:" + PDPlayPassData.rollSum, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 5, "点数:" + PDPlayPassData.score, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 6, "魂ゲージ:" + PDPlayPassData.gauge, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 7, "良:" + PDPlayPassData.greatSum, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 8, "可:" + PDPlayPassData.goodSum, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //	DX.DrawStringToHandle(0, 18 * 9, "不可:" + PDPlayPassData.badSum, DX.GetColor(255, 255, 255), fontDebug, DX.GetColor(0, 0, 0));
            //}
			#endregion

			#region ジャッジ評価
			judgeGreat.Draw();
			judgeGood.Draw();
			judgeBad.Draw();
			#endregion

			//soul.Draw(480 + 712, 115);
			sceneIn.Draw();
        }
		public override void Finish()
		{
            SPlayDxFont.Finish();
            SPlayDxImage.Finish();
            SPlayDxSound.Finish();
            Debug.Dispose();
            sceneIn.Reset();
            pts.Dispose();
			pts = null;
            balloon.Dispose();
			balloon = null;
            roll.Dispose();
			roll = null;
			State = EState.Null;
		}
		public static SPlay Construct(MainControl m)
		{
			
			if (Play != null)
			{
				PrintMessage("えらー");
			}
			Play = new SPlay();
			return Play;
		}
		SPlay()
		{
			State = EState.Null;
			SongSelectPassData = PDSongSelectPassData.Construct(this);
		}
	}
	public class PDPlayPassData
	{
		public static bool clear = false;
		public static bool max = false;
		public static int clearGauge;
		public static int gauge { get; private set; }
		public static void addGauge(double m, int g)
		{
			gauge += Convert.ToInt32(g * m);
			if (gauge < 0)
			{
				gauge = 0;
			}
			if (gauge > 10000)
			{
				gauge = 10000;
			}
			if (gauge == 10000)
			{
				max = true;
			}
			else
			{
				max = false;
			}
			if (clearGauge <= gauge)
			{
				clear = true;
			}
			else
			{
				clear = false;
			}
		}
		public static int score { get; private set; }
		public static void addScore(int p)
		{
			score += p;
		}

		public static int combo { get; private set; }
		public static void addCombo()
		{
			combo++;
		}
		public static void resetCombo()
		{
			combo = 0;
		}
		public static int maxCombo { get; private set; }
		public static void setMaxCombo()
		{
			if (maxCombo <= combo)
			{
				maxCombo = combo;
			}
		}

		public static int rollSum { get; private set; }
		public static void addRollSum()
		{
			rollSum++;
		}

		public static int greatSum { get; private set; }
		public static void addGreatSum()
		{
			greatSum++;
		}

		public static int goodSum { get; private set; }
		public static void addGoodSum()
		{
			goodSum++;
		}

		public static int badSum { get; private set; }
		public static void addBadSum()
		{
			badSum++;
		}

		public static void resetResult()
		{
			clear = false;
			max = false;
			gauge = 0;
			score = 0;
			combo = 0;
			maxCombo = 0;
			rollSum = 0;
			greatSum = 0;
			goodSum = 0;
			badSum = 0;
		}
	}
}