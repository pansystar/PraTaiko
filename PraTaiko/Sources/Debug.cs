using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibForCS;

using static DxLibDLL.DX;

namespace PraTaiko
{
    abstract class Debug<T> : IGameAction, IDisposable
    {
        class DebugDxFont : DxFont<DebugDxFont> { }

        CMainConfig MainConfig;
        
        DebugDxFont font;

        int leftCount;
        int rightCount;

        void ResetLeftCount()
        {
            leftCount = 0;
        }
        void ResetRightCount()
        {
            rightCount = 0;
        }

        protected void Left(string str = "")
        {
            Left(str, DxColor.White, DxColor.CharcoalGray);
        }
        protected void Left(string str, uint color)
        {
            Left(str, color, DxColor.CharcoalGray);
        }
        protected void Left(string str, uint color, uint edgeColor)
        {
            font.Draw(0, (font.Size + 2) * leftCount, str, color, edgeColor);
            leftCount++;
        }
        protected void Right(string str = "")
        {
            Right(str, DxColor.White, DxColor.CharcoalGray);
        }
        protected void Right(string str, uint color)
        {
            Right(str, color, DxColor.CharcoalGray);
        }
        protected void Right(string str, uint color, uint edgeColor)
        {
            font.Draw(MainConfig.DrawWidth - font.Width(str), (font.Size + 2) * rightCount, str, color, edgeColor);
            rightCount++;
        }

        protected abstract void LeftDraw();
        protected abstract void RightDraw();

        void IGameAction.Update()
        {
        }
        void IGameAction.Draw()
        {
            if (MainConfig.Debug)
            {
                LeftDraw();
                RightDraw();

                ResetLeftCount();
                ResetRightCount();
            }
        }
        public void Dispose()
        {
            DebugDxFont.Finish();
        }

        void IGameAction.Finish()
        {
        }

        public Debug()
        {
            font = new DebugDxFont().SetFont("ＤＦＰ勘亭流", 18, 2);
            MainConfig = CMainConfig.Get();
        }
    }
}
