using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace DxLibForCS
{
    interface IDxSceneBase
    {
        void Start();
        void Update();
        void Draw();
        void Finish();
    }
    public abstract class SceneBase : IDxSceneBase
    {
        static List<IDxSceneBase> sList;

        public abstract void Start();
        public abstract void Update();
        public abstract void Draw();
        public abstract void Finish();

        static SceneBase()
        {
            sList = new List<IDxSceneBase>();
        }
        public SceneBase()
        {
            sList.Add(this as IDxSceneBase);
        }
    }

    public abstract class DxFontBase
    {
        protected const int ANTIALIASING_4X4 = 18;
        protected int Handle;

        public string FontName { get; protected set; }

        public int Size { get; protected set; }

        public float X { get; protected set; }
        public float Y { get; protected set; }

        public double ExRateX { get; protected set; }
        public double ExRateY { get; protected set; }

        public void SetExRate(double eX, double eY)
        {
            ExRateX = eX;
            ExRateY = eY;
        }

        public int Width(string str)
        {
            return GetDrawExtendStringWidthToHandle(ExRateX, str, str.Length, Handle);
        }

        public void SetPoint(float xf, float yf)
        {
            X = xf;
            Y = yf;
        }
        public void SetSpace(int space)
        {
            SetFontSpaceToHandle(space, Handle);
        }

        public void Draw(string str, uint color, uint edgeColor = 0)
        {
            DrawExtendStringFToHandle(X, Y, ExRateX, ExRateY, str, color, Handle, edgeColor);
        }
        public void Draw(int x, int y, string str, uint color, uint edgeColor = 0)
        {
            DrawExtendStringToHandle(x, y, ExRateX, ExRateY, str, color, Handle, edgeColor);
        }
        public void Draw(float xf, float yf, string str, uint color, uint edgeColor = 0)
        {
            DrawExtendStringFToHandle(xf, yf, ExRateX, ExRateY, str, color, Handle, edgeColor);
        }

        protected void Init()
        {
            Handle = -1;
            FontName = "";
            Size = 20;
            X = Y = 0;
            ExRateX = ExRateY = 1.0;
        }
        protected void Delete()
        {
            DeleteFontToHandle(Handle);
        }
        public DxFontBase()
        {
            Init();
        }
    }
    public abstract class DxFont<T> : DxFontBase where T : class
    {
        static List<DxFont<T>> fList;
        static DxFont()
        {
            if (fList == null) fList = new List<DxFont<T>>();
        }
        public static void Finish()
        {
            DeleteAll();
            fList.Clear();
        }
        static void DeleteAll()
        {
            foreach (var fl in fList)
            {
                fl.Delete();
            }
        }
        public T SetFont(string fontName, int size, int edgeSize = -1)
        {
            if (Handle != -1)
            {
                Delete();
            }
            int h = CreateFontToHandle(fontName, size, -1, ANTIALIASING_4X4, -1, edgeSize);
            if (Handle == -1)
            {
                fList.Add(this);
            }
            Handle = h;
            FontName = fontName;
            Size = size;

            return this as T;
        }

        public DxFont()
        {

        }
    }
    
    public abstract class DxImageBase
    {
        protected int[] Handle;

        public string FilePath { get; protected set; }
        public string FileFullPath
        {
            get
            {
                return Path.GetFullPath(FilePath);
            }
        }
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        float NowX
        {
            get
            {
                return PointX + ScrollX;
            }
        }
        float NowY
        {
            get
            {
                return PointY + ScrollY;
            }
        }
        
        public float PointX { get; private set; }
        public float PointY { get; private set; }

        public float NowLeftX
        {
            get
            {
                return NowX;
            }
        }
        public float NowCenterX
        {
            get
            {
                return NowY + Width / 2;
            }
        }
        public float NowRightX
        {
            get
            {
                return PointX + Width;
            }
        }

        public float NowTopY
        {
            get
            {
                return NowY;
            }
        }
        public float NowCenterY
        {
            get
            {
                return NowY + Height/2;
            }
        }
        public float NowButtomY
        {
            get
            {
                return NowY + Height;
            }
        }

        public float MoveSpeedX { get; private set; }
        public float MoveSpeedY { get; private set; }

        public float ScrollX { get; private set; }
        public float ScrollY { get; private set; }

        public void SetPoint(float xf, float yf)
        {
            PointX = xf;
            PointY = yf;
        }
        public void SetPoint(string xs, string ys)
        {
            float x, y;
            int sx, sy;

            GetDrawScreenSize(out sx, out sy);

            if(!float.TryParse(xs, out x))
            {
                switch (xs)
                {
                    case "left":
                        x = 0;
                        break;
                    case "right":
                        x = sx - Width;
                        break;
                    case "center":
                        x = sx / 2 - Width / 2;
                        break;
                    default:
                        break;
                }
            }
            if (!float.TryParse(ys, out y))
            {
                switch (ys)
                {
                    case "top":
                        y = 0;
                        break;
                    case "buttom":
                        y = sy - Height;
                        break;
                    case "center":
                        y = sy / 2 - Height / 2;
                        break;
                    default:
                        break;
                }
            }
            SetPoint(x, y);
        }

        public void SetCenterPoint(float xf, float yf)
        {
            PointX = xf - Width / 2;
            PointY = yf - Height / 2;
        }
        public void SetMoveSpeed(float xf, float yf)
        {
            MoveSpeedX = xf/60;
            MoveSpeedY = yf/60;

            DrawAct = ActTile;
        }

        Action<int> DrawAct;
        void ActRota(int index = 0)
        {
        }
        void ActNormal(int index = 0)
        {
            DrawGraphF(PointX, PointY, Handle[index], 1);
        }
        void ActTile(int index = 0)
        {
            int sx, sy;
            GetDrawScreenSize(out sx, out sy);


            float sizeXBuf = 0;
            float sizeYBuf = 0;

            ScrollX += MoveSpeedX;
            ScrollY += MoveSpeedY;

            if (Math.Abs(ScrollX) > Width)
            {
                ScrollX = ScrollX % Width;
            }
            if (Math.Abs(ScrollY) > Height)
            {
                ScrollY = ScrollY % Height;
            }
            for (int j = -1; sizeYBuf < sy; j++)
            {
                sizeYBuf = PointY + ScrollY + Height * j;
                for (int i = -1; sizeXBuf < sx; i++)
                {
                    sizeXBuf = PointX + ScrollX + Width * i;
                    DrawGraphF(sizeXBuf, sizeYBuf, Handle[index], 1);
                }
                sizeXBuf = 0;
            }
        }

        public void Draw(int index = 0)
        {
            DrawAct(index);
        }
        public void Draw(int x, int y, int index = 0)
        {
            DrawGraph(x, y, Handle[index], 1);
        }
        public void Draw(float xf, float yf, int index = 0)
        {
            DrawGraphF(xf, yf, Handle[index], 1);
        }

        public void DrawTile(DxImageBase image, int index = 0)
        {
            int sx, sy;
            GetDrawScreenSize(out sx, out sy);

            int gx = image.Width;
            int gy = image.Height;

            if (gx == 0) return;

            float sizeXBuf = 0;
            float sizeYBuf = 0;

            ScrollX += MoveSpeedX;
            ScrollY += MoveSpeedY;

            if (Math.Abs(ScrollX) > gx)
            {
                ScrollX = ScrollX % gx;
            }
            if (Math.Abs(ScrollY) > gy)
            {
                ScrollY = ScrollY % gy;
            }
            for (int j = -1; sizeYBuf < sy; j++)
            {
                sizeYBuf = PointY + ScrollY + gy * j;
                for (int i = -1; sizeXBuf < sx; i++)
                {
                    sizeXBuf = PointX + ScrollX + gx * i;
                    image.Draw(sizeXBuf, sizeYBuf, index);
                }
                sizeXBuf = 0;
            }
        }
        public void Delete()
        {
            if (Handle != null)
            {
                foreach (var h in Handle)
                {
                    DeleteGraph(h);
                }
            }
        }
        protected void Init()
        {
            Handle = null;
            Width = 0;
            Height = 0;
            PointX = PointY = 0;
            MoveSpeedX = MoveSpeedY = 0;
            ScrollX = ScrollY = 0;
        }
        public DxImageBase()
        {

            Init();
            DrawAct = ActNormal;
        }
    }
    public abstract class DxImage<T> : DxImageBase where T : class
    {
        static List<DxImage<T>> iList;

        static void DeleteAll()
        {
            foreach (var il in iList)
            {
                il.Delete();
                il.Init();
            }
        }
        public static void Finish()
        {
            DeleteAll();
            iList.Clear();
        }

        /// <summary>
        /// 画像を読み込みます
        /// </summary>
        /// <param name="path">ファイルパス</param>        
        public T SetImage(string path)
        {
            int[] handles = Handle;

            if (handles != null)
            {
                foreach (var h in handles)
                {
                    DeleteGraph(h);
                }
            }
            else
            {
                iList.Add(this);
            }
            FilePath = path;

            Handle = Enumerable.Repeat(-1, 1).ToArray();
            Handle[0] = LoadGraph(path);

            int sx, sy;
            GetGraphSize(Handle[0], out sx, out sy);

            Width = sx;
            Height = sy;
            
            return this as T;
        }
        /// <summary>
        /// 画像を分割して読み込みます
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="xNum">横の数</param>
        /// <param name="yNum">縦の数</param>
        public T SetImage(string path, int xNum, int yNum)
        {
            int[] handles = Handle;

            if (handles != null)
            {
                foreach (var h in handles)
                {
                    DeleteGraph(h);
                }
            }
            else
            {
                iList.Add(this);
            }
            FilePath = path;           

            Bitmap b = new Bitmap(path);

            Handle = Enumerable.Repeat(-1, xNum * yNum).ToArray();
            LoadDivGraph(path, xNum * yNum, xNum, yNum, b.Width / xNum, b.Height / yNum, out Handle[0]);
            
            int sx, sy;
            GetGraphSize(Handle[0], out sx, out sy);

            Width = sx;
            Height = sy;
            
            return this as T;
        }

        static DxImage()
        {
            if (iList == null) iList = new List<DxImage<T>>();
        }
        public DxImage() { }
    }

    public abstract class DxSoundBase
    {
        protected int Handle = -1;
        public int Time { get; private set; }
        public int PlayCount { get; private set; }
        public int CheckNow
        {
            get
            {
                return CheckSoundMem(Handle);
            }
        }
        public int CheckASyncNow
        {
            get
            {
                return CheckHandleASyncLoad(Handle);
            }
        }

        public void Play()
        {
            PlaySoundMem(Handle, DX_PLAYTYPE_BACK, 1);
            PlayCount++;
        }
        public void RePlay()
        {
            SetSoundCurrentTime(Time, Handle);
            PlaySoundMem(Handle, DX_PLAYTYPE_BACK, 0);
        }
        public void Pause()
        {
            StopSoundMem(Handle);
            Time = GetSoundCurrentTime(Handle);
        }
        public void Stop()
        {
            StopSoundMem(Handle);
            Time = 0;
            SetSoundCurrentTime(Time, Handle);
        }
        
        public void SetTime(int t)
        {
            Time = t;
        }

        protected void Init()
        {
        }
        public void Delete()
        {
            DeleteSoundMem(Handle);
        }
    }
    public abstract class DxSound<T> : DxSoundBase where T : class
    {
        static List<DxSound<T>> sList;
        static void DeleteAll()
        {
            foreach (var sl in sList)
            {
                sl.Delete();
                sl.Init();
            }
        }
        public static void Finish()
        {
            DeleteAll();
            sList.Clear();
        }
        public T SetSound(string path)
        {
            int handle = Handle;

            if (handle != -1)
            {
                Delete();
            }
            else
            {
                sList.Add(this);
            }
            Handle = LoadSoundMem(path);
            
            return this as T;
        }
        static DxSound()
        {
            if (sList == null) sList = new List<DxSound<T>>();
        }
        public DxSound()
        {
        }
    }

    public static class DxColor
    {
        public static uint Black
        {
            get
            {
                return GetColor(0, 0, 0);
            }
        }
        public static uint Blue
        {
            get
            {
                return GetColor(0, 0, 255);
            }
        }
        public static uint CharcoalGray
        {
            get
            {
                return GetColor(81, 68, 84);
            }
        }
        public static uint Orange
        {
            get
            {
                return GetColor(255, 165, 0);
            }
        }
        public static uint White
        {
            get
            {
                return GetColor(255, 255, 255);
            }
        }
        public static uint Yellow
        {
            get
            {
                return GetColor(255, 255, 0);
            }
        }
    }
}
