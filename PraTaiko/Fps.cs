using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DxLibDLL.DX;
using DxLibForCS;

namespace PraTaiko
{
    static class Fps
    {
        static CMainConfig Conf;
        class FpsFont : DxFont<FpsFont> { }
        static FpsFont font;
        static int mStartTime;
        static int mCount;
        public static float mFps { get; private set; }
        public const int BASE_FPS = 60;
        public const int FPS = 60;
        static bool DrawFlag = false;
        public static void ChangeDrawFlag(ICommand c)
        {
            DrawFlag = !DrawFlag;
        }
        static Fps()
        {
            mStartTime = 0;
            mCount = 0;
            mFps = 0;
            Conf = CMainConfig.Get();
            font = new FpsFont().SetFont("ＤＦＰ勘亭流", 20, 2);
        }
        public static bool Update()
        {
            if (mCount == 0)
            { //1フレーム目なら時刻を記憶
                mStartTime = GetNowCount();
            }
            if (mCount == BASE_FPS)
            { //60フレーム目なら平均を計算する
                int t = GetNowCount();
                mFps = 1000f / ((t - mStartTime) / (float)BASE_FPS);
                mCount = 0;
                mStartTime = t;
            }
            mCount++;
            return true;
        }
        public static void Draw()
        {
            if (DrawFlag)
            {
                font.Draw(Conf.DrawWidth - font.Width("FPS:"+mFps.ToString("F1")), Conf.DrawHeight - font.Size-4, "FPS:" + mFps.ToString("F1"), DxColor.White, DxColor.Black);
            }
        }
        public static void Wait()
        {
            int tookTime = GetNowCount() - mStartTime;  //かかった時間
            int waitTime = mCount * 1000 / FPS - tookTime;  //待つべき時間
            if (waitTime > 0)
            {
                WaitTimer(waitTime);
            }
        }
    }
}
