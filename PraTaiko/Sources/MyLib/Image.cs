using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
using System.Text.RegularExpressions;

using static PraTaiko.MainControl;
namespace PraTaiko
{
    public class Image
    {
		static COldConfig MainConfig;
        public int x, y;
        public int repeat;
        public int fadeSpeed;
        public int fade;
        public int sizeX, sizeY;
        public float speedX, speedY;
        public int moveX, moveY;
        public float scrX, scrY;
        public string filePath;
        public int handle;
        public virtual void Update() { }
        public virtual void Draw()
        {
            float sizeXBuf = 0;
            float sizeYBuf = 0;
            switch (repeat)
            {
                case 0:
                    DX.DrawGraph(x, y, handle, 1);
                    break;
                case 1:
                    if (handle != -1)
                    {
                        sizeXBuf = 0;
                        scrX += speedX;
                        if (Math.Abs(scrX) > sizeX)
                        {
                            scrX = scrX % sizeX;
                        }
                        for (int i = -1; sizeXBuf < MainConfig.DrawSize.X; i++)
                        {
                            sizeXBuf = x + scrX + sizeX * i;
                            DX.DrawGraphF(sizeXBuf, y, handle, 1);
                        }
                    }
                    break;
                case 2:
                    if (handle != -1)
                    {
                        sizeYBuf = 0;
                        scrY += speedY;
                        if (Math.Abs(scrY) > sizeY)
                        {
                            scrY = scrY % sizeY;
                        }
                        for (int i = -1; sizeYBuf < MainConfig.DrawSize.X; i++)
                        {
                            sizeYBuf = y + scrY + sizeY * i;
                            DX.DrawGraphF(x, sizeYBuf, handle, 1);
                        }
                    }
                    break;
                case 3:
                    if (handle != -1)
                    {
                        sizeXBuf = 0;
                        sizeYBuf = 0;
                        scrX += speedX;
                        scrY += speedY;
                        if (Math.Abs(scrX) > sizeX)
                        {
                            scrX = scrX % sizeX;
                        }
                        if (Math.Abs(scrY) > sizeY)
                        {
                            scrY = scrY % sizeY;
                        }
                        for (int j = -1; sizeYBuf < MainConfig.DrawSize.Y; j++)
                        {
                            sizeYBuf = y + scrY + sizeY * j;
                            for (int i = -1; sizeXBuf < MainConfig.DrawSize.X; i++)
                            {
                                sizeXBuf = x + scrX + sizeX * i;
                                DX.DrawGraphF(sizeXBuf, sizeYBuf, handle, 1);
                            }
                            sizeXBuf = 0;
                        }
                    }
                    break;
            }
        }
        public virtual void Draw(int x, int y)
        {
            DX.DrawGraph(x, y, handle, 1);
        }
        public int setFadeSpeed(int fs)
        {
            fadeSpeed = fs;
            return 1;
        }
        public int setPoint(string[] p)
        {
            if (p.Length == 2)
            {
                if (!int.TryParse(p[0], out x))
                {
                    switch (p[0])
                    {
                        case "left":
                            x = 0;
                            break;
                        case "right":
                            x = MainConfig.DrawSize.X - sizeX;
                            break;
                        case "center":
                            x = MainConfig.DrawSize.X / 2 - sizeX / 2;
                            break;
                        default:
                            break;
                    }
                }
                if (!int.TryParse(p[1], out y))
                {
                    switch (p[1])
                    {
                        case "top":
                            y = 0;
                            break;
                        case "buttom":
                            y = MainConfig.DrawSize.Y - sizeY;
                            break;
                        case "center":
                            x = MainConfig.DrawSize.Y / 2 - sizeY / 2;
                            break;
                        default:
                            break;
                    }
                }
                return 1;
            }
            else
            {
                return -1;
            }
        }
        public int setPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
            return 1;
        }
        public void SetRepeat(string str)
        {
            repeat = 0;
            string[] s = str.Split(',');
            foreach (var item in s.Select((value, index) => new { value, index }))
            {
                switch (item.index)
                {
                    case 0:
                        repeat += 1;
                        switch (item.value)
                        {
                            case "":
                                break;
                            default:
                                speedX = int.Parse(item.value) / 60;
                                break;
                        }
                        break;
                    case 1:
                        repeat += 2;
                        switch (item.value)
                        {
                            case "":
                                break;
                            default:
                                speedY = int.Parse(item.value) / 60;
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public int setRepeat(string[] r)
        {
            repeat = 0;
            Regex r_xEqual = new Regex("x=");
            Regex r_yEqual = new Regex("y=");
            if (r.Length < 3)
            {
                foreach (var item in r)
                {
                    if (r_xEqual.IsMatch(item))
                    {
                        repeat += 1;
                        speedX = int.Parse(r_xEqual.Replace(item, "")) / 60;
                    }
                    else if (item.Equals("x"))
                    {
                        repeat += 1;
                    }
                    else if (r_yEqual.IsMatch(item))
                    {
                        repeat += 2;
                        speedY = int.Parse(r_yEqual.Replace(item, "")) / 60;
                    }
                    else if (item.Equals("y"))
                    {
                        repeat += 2;
                    }
                }
                return 1;
            }
            else
            {
                return -1;
            }
        }
        public int setRepeat(int r, int sx, int sy)
        {
            repeat = r;
            speedX = sx;
            speedY = sy;

            return 1;
        }
        public virtual int setGenre(string g) { return -2; }

        //genreBg
        public virtual void setCenterImage(string fp) { }
        public virtual void setCenterSongSize(string[] s) { }
        public virtual void setCenterSongSize(int x, int y) { }
        public virtual void setCenterSongPoint(string[] s) { }
        public virtual void setCenterSongPoint(int x, int y) { }
        public virtual void setCenterCourseSize(string[] s) { }
        public virtual void setCenterCourseSize(int sx, int sy) { }

        public virtual void setOtherSongSize(string[] s) { }
        public virtual void setOtherSongSize(int x, int y) { }
        public virtual void setOtherSongNum(string[] s) { }
        public virtual void setOtherSongNum(int x, int y) { }

        public virtual void setSongSize(string[] s) { }
        public virtual void setSongSize(int sx, int sy) { }
        public virtual void setSongWidthInterval(string s) { }

        public Image(string fp = "")
        {
			MainConfig = COldConfig.Get();
            filePath = fp;
            handle = DX.LoadGraph(filePath);
            DX.GetGraphSize(handle, out sizeX, out sizeY);
            repeat = 0;
            speedX = speedY = 0;
            fadeSpeed = 0;
            fade = 255;
        }

        public void Draw(char v)
        {
            DX.DrawRotaGraphF(x, y, 1.0, 0.0, handle, 1);
        }
    }
    public class ExtendImage : Image
    {
        public int height;
        public int width;

        public void setSize(string[] s)
        {
            if (s[0] == "")
            {
                width = sizeX;
            }
            else if (!int.TryParse(s[0], out width))
            {
                //error
            }

            if (s[1] == "")
            {
                height = sizeY;
            }
            else if (!int.TryParse(s[1], out height))
            {
                //error
            }
        }
        public override void Draw()
        {
            DX.DrawExtendGraph(x, y, x + width, y + height, handle, 1);
        }
        public override void Draw(int x, int y)
        {
            DX.DrawExtendGraph(x, y, x + width, y + height, handle, 1);
        }
        public ExtendImage(string fp = "") : base(fp)
        {

        }
    }
}
