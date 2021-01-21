using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pansystar.Extensions;
using DxLibDLL;
using DxLibForCS;
namespace PraTaiko
{
	class SResult : BaseScene
	{
		static SResult Result;
        class SResultDxFont : DxFont<SResultDxFont>
        {
        }
        public static bool clear = false;
		public static bool max = false;
		public static int clearGauge = 5000;
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
		Image bg;
		Image desc;
		class GaugeImage : Image
		{
			public GaugeImage(string fp) : base(fp)
			{
			}
			public override void Draw()
			{
				base.Draw();
			}
		}
		GaugeImage gaugeNone;

		Image gaugeClearPiece1;
		Image gaugeClearPiece2;

		Sound kanakou;
        SResultDxFont FontResult;
		GaugeImage gaugeMax;
		public void dataInit()
		{

		}
		public override void Start()
		{
			bg = new Image(OldConfig.ImageFolder + @"\result\bg.png");
			desc = new Image(OldConfig.ImageFolder + @"\result\bg_desc.png");
            FontResult = new SResultDxFont();
            FontResult.SetFont("ＤＦＰ勘亭流", 30, 2);

			gaugeNone = new GaugeImage(OldConfig.ImageFolder + @"\result\gauge_none.png");
			gaugeClearPiece1 = new Image(OldConfig.ImageFolder + @"\result\gauge_clear_piece1.png");
			gaugeClearPiece2 = new Image(OldConfig.ImageFolder + @"\result\gauge_clear_piece2.png");
			gaugeMax = new GaugeImage(OldConfig.ImageFolder + @"\result\gauge_max.png");

			kanakou = new Sound(OldConfig.SoundFolder + @"\kanakou.ogg");
		}
		public override void Update()
		{
			if (Key.GetCount(Key.Escape) == 1)
			{
				NScene.SetNext(this, EScene.SongSelect);
			}
			if (kanakou.CheckNow() == 0)
			{
				kanakou.Play();
			}
		}

        const int X1 = 760;
        const int Y1 = 190;
        const int INTERVAL_Y = 40;
        readonly int Y2 = Y1 + INTERVAL_Y;
        readonly int Y3 = Y1 + INTERVAL_Y * 2;

        const int X2 = 930;

        const int COMBO_X = 950;

		public override void Draw()
		{
			bg.Draw();
			desc.Draw();

            FontResult.Draw(X1, Y1, "良", DX.GetColor(255, 255, 83), DxColor.Black);
            FontResult.Draw(X1, Y2, "可", DxColor.White, DxColor.Black);
            FontResult.Draw(X1, Y3, "不可", DxColor.White, DxColor.Black);

            FontResult.Draw(X2 - FontResult.Width(PDPlayPassData.greatSum.ToString()), Y1, PDPlayPassData.greatSum.ToString(), DxColor.White, DxColor.Black);
            FontResult.Draw(X2 - FontResult.Width(PDPlayPassData.goodSum.ToString()), Y1, PDPlayPassData.goodSum.ToString(), DxColor.White, DxColor.Black);
            FontResult.Draw(X2 - FontResult.Width(PDPlayPassData.badSum.ToString()), Y1, PDPlayPassData.badSum.ToString(), DxColor.White, DxColor.Black);

            FontResult.Draw(COMBO_X, Y1, "最大コンボ数", DX.GetColor(255, 79, 0), DxColor.Black);
            FontResult.Draw(30 * 3 + COMBO_X, Y2, "連打数", DX.GetColor(255, 79, 0), DxColor.Black);
            FontResult.Draw(1235 - FontResult.Width(maxCombo.ToString()), Y1, maxCombo.ToString(), DxColor.White, DxColor.Black);
            FontResult.Draw(1235 - FontResult.Width(rollSum.ToString()), Y1, rollSum.ToString(), DxColor.White, DxColor.Black);

            gaugeNone.Draw(525, 117);

            if (gauge < 10000)
			{
				for (int i = 0; i < gauge / 200; i++)
				{
					if (i < 39)
					{
						gaugeClearPiece1.Draw(531 + 14 * i, 123);
					}
					else
					{
						gaugeClearPiece2.Draw(531 + 14 * i, 123);
					}
				}
			}
			else
			{
				gaugeMax.Draw(525, 117);
			}
			//player[Player.elem].Draw(150, 116, 192, 4, DX.GetColor(32, 32, 32));
		}
		public override void Finish()
		{
            SResultDxFont.Finish();
			DX.DeleteSoundMem(kanakou.Handle);
		}
		public static SResult Construct(MainControl m)
		{
			if (Result != null)
			{
				PrintMessage("選曲画面エラー");
			}
			Result = new SResult();
			return Result;
		}
		SResult()
		{
		}
	}
}
