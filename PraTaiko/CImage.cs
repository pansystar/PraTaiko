using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace PraTaiko
{
    class CImage : IGameAction
    {
        static CMainConfig MainConfig;
        public Action Draw;
        public void DrawAPoint(float x, float y)
        {
            DrawGraphF(x, y, Handle, TRUE);
        }
        enum EDrawFlag : int
        {
            Normal = 0, LoopX = 1, LoopY = 2,
        }
        EDrawFlag DrawFlag;

        public int Handle { get; private set; } = -1;

        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;

        public float X { get; private set; } = 0;
        public float Y { get; private set; } = 0;

        public float SpeedX { get; private set; } = 0;
        public float SpeedY { get; private set; } = 0;

        public float ScrollX { get; private set; } = 0;
        public float ScrollY { get; private set; } = 0;

        public void SetImage(string str)
        {
            Handle = LoadGraph(str);
            int x, y;
            GetGraphSize(Handle, out x, out y);
            Width = x;
            Height = y;
        }
        public void SetPoint(string str)
        {
            string[] s = str.Split(',');
            foreach (var item in s.Select((value, index) => new { value, index }))
            {
                float x, y;
                switch (item.index)
                {
                    case 0:
                        #region X座標
                        if (!float.TryParse(item.value, out x))
                        {
                            switch (item.value)
                            {
                                case "left":
                                    x = 0;
                                    break;
                                case "right":
                                    x = MainConfig.DrawWidth - Width;
                                    break;
                                case "center":
                                    x = MainConfig.DrawWidth / 2 - Width / 2;
                                    break;
                                default:
                                    break;
                            }
                        }
                        X = x;
                        #endregion
                        break;
                    case 1:
                        #region Y座標
                        if (!float.TryParse(item.value, out y))
                        {
                            switch (item.value)
                            {
                                case "top":
                                    y = 0;
                                    break;
                                case "buttom":
                                    y = MainConfig.DrawHeight - Height;
                                    break;
                                case "center":
                                    y = MainConfig.DrawHeight / 2 - Height / 2;
                                    break;
                                default:
                                    break;
                            }
                        }
                        Y = y;
                        #endregion
                        break;
                    default:
                        break;
                }
            }
        }
        public void SetLoop(string str)
        {
            string[] s = str.Split(',');

            foreach (var item in s.Select((value, index) => new { value, index }))
            {
                switch (item.index)
                {
                    case 0:
                        switch (item.value)
                        {
                            case "":
                                break;
                            default:
                                SpeedX = float.Parse(item.value) / 60;
                                AddDrawFlag(EDrawFlag.LoopX);
                                break;
                        }
                        break;
                    case 1:
                        switch (item.value)
                        {
                            case "":
                                break;
                            default:
                                SpeedY = float.Parse(item.value) / 60;
                                AddDrawFlag(EDrawFlag.LoopY);
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            SetDraw();
        }

        void AddDrawFlag(EDrawFlag f)
        {
            DrawFlag = DrawFlag | f;
        }
        void RemoveDrawFlag(EDrawFlag f)
        {
            DrawFlag = DrawFlag & ~f;
        }

        void SetDraw()
        {
            switch (DrawFlag)
            {
                case EDrawFlag.Normal:
                    Draw = DrawNormal;
                    break;
                case EDrawFlag.LoopX:
                    Draw = DrawLoopX;
                    break;
                case EDrawFlag.LoopY:
                    Draw = DrawLoopY;
                    break;
                case EDrawFlag.LoopX | EDrawFlag.LoopY:
                    Draw = DrawLoopXY;
                    break;
            }
        }

        void DrawNormal()
        {
            DrawGraphF(X, Y, Handle, TRUE);
        }
        void DrawLoopX()
        {
            float sizeXBuf = 0;
            ScrollX += SpeedX;
            if (Math.Abs(ScrollX) > Width)
            {
                ScrollX = ScrollX % Width;
            }
            for (int i = -1; sizeXBuf < MainConfig.DrawWidth; i++)
            {
                sizeXBuf = X + ScrollX + Width * i;
                DrawGraphF(sizeXBuf, Y, Handle, 1);
            }
        }
        void DrawLoopX(int handle)
        {
            float sizeXBuf = 0;
            ScrollX += SpeedX;
            if (Math.Abs(ScrollX) > Width)
            {
                ScrollX = ScrollX % Width;
            }
            for (int i = -1; sizeXBuf < MainConfig.DrawWidth; i++)
            {
                sizeXBuf = X + ScrollX + Width * i;
                DrawGraphF(sizeXBuf, Y, handle, 1);
            }
        }
        void DrawLoopY()
        {
            float sizeYBuf = 0;
            ScrollY += SpeedY;
            if (Math.Abs(ScrollY) > Height)
            {
                ScrollY = ScrollY % Height;
            }
            for (int i = -1; sizeYBuf < MainConfig.DrawHeight; i++)
            {
                sizeYBuf = Y + ScrollY + Height * i;
                DrawGraphF(X, sizeYBuf, Handle, 1);
            }
        }
        void DrawLoopXY()
        {
            float sizeXBuf = 0;
            float sizeYBuf = 0;
            ScrollX += SpeedX;
            ScrollY += SpeedY;
            if (Math.Abs(ScrollX) > Width)
            {
                ScrollX = ScrollX % Width;
            }
            if (Math.Abs(ScrollY) > Height)
            {
                ScrollY = ScrollY % Height;
            }
            for (int j = -1; sizeYBuf < MainConfig.DrawHeight; j++)
            {
                sizeYBuf = Y + ScrollY + Height * j;
                for (int i = -1; sizeXBuf < MainConfig.DrawWidth; i++)
                {
                    sizeXBuf = X + ScrollX + Width * i;
                    DrawGraphF(sizeXBuf, sizeYBuf, Handle, 1);
                }
                sizeXBuf = 0;
            }
        }
        public void DrawLoopXYAHandle(int handle)
        {
            float sizeXBuf = 0;
            float sizeYBuf = 0;
            ScrollX += SpeedX;
            ScrollY += SpeedY;
            if (Math.Abs(ScrollX) > Width)
            {
                ScrollX = ScrollX % Width;
            }
            if (Math.Abs(ScrollY) > Height)
            {
                ScrollY = ScrollY % Height;
            }
            for (int j = -1; sizeYBuf < MainConfig.DrawHeight; j++)
            {
                sizeYBuf = Y + ScrollY + Height * j;
                for (int i = -1; sizeXBuf < MainConfig.DrawWidth; i++)
                {
                    sizeXBuf = X + ScrollX + Width * i;
                    DrawGraphF(sizeXBuf, sizeYBuf, handle, 1);
                }
                sizeXBuf = 0;
            }
        }

        void IGameAction.Update()
        {
        }
        void IGameAction.Draw()
        {
            Draw();
        }

        void IGameAction.Finish()
        {
            throw new NotImplementedException();
        }

        static CImage()
        {
            MainConfig = CMainConfig.Get();
        }
        public CImage()
        {
            Draw = DrawNormal;
        }
        //public CImage(CImage image)
        //{
            
        //}
        public CImage(string fp)
        {
            SetImage(fp);
            Draw = DrawNormal;
        }
    }
}
