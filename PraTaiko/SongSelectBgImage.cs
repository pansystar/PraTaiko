using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DxLibDLL;

namespace praTaiko
{
    public class SongSelectBgImage : Image
    {
        public string genre;
        public void setGenre(string g, string gn)
        {
            genre = g;
            if (g.Equals(gn))
            {
                fade = 255;
            }
            else
            {
                fade = 0;
            }
        }
        public void Draw(string g)
        {
            if (genre == null || g.Equals(genre))
            {
                float sizeXBuf = 0;
                float sizeYBuf = 0;
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, fade);
                if (fade < 256)
                {
                    fade += fadeSpeed;
                }
                else if (fade > 255)
                {
                    fade = 255;
                }
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
                            for (int i = -1; sizeXBuf < Program.config.windowSize.x; i++)
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
                            for (int i = -1; sizeYBuf < Program.config.windowSize.x; i++)
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
                            for (int j = -1; sizeYBuf < Program.config.windowSize.y; j++)
                            {
                                sizeYBuf = y + scrY + sizeY * j;
                                for (int i = -1; sizeXBuf < Program.config.windowSize.x; i++)
                                {
                                    sizeXBuf = x + scrX + sizeX * i;
                                    DX.DrawGraphF(sizeXBuf, sizeYBuf, handle, 1);
                                }
                                sizeXBuf = 0;
                            }
                        }
                        break;
                }
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255);
            }
            else
            {
                float sizeXBuf = 0;
                float sizeYBuf = 0;
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, fade);
                if (fade > 0)
                {
                    fade -= fadeSpeed;
                }
                else if (fade < 0)
                {
                    fade = 0;
                }

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
                            for (int i = -1; sizeXBuf < Program.config.windowSize.x; i++)
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
                            for (int i = -1; sizeYBuf < Program.config.windowSize.x; i++)
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
                            for (int j = -1; sizeYBuf < Program.config.windowSize.y; j++)
                            {
                                sizeYBuf = y + scrY + sizeY * j;
                                for (int i = -1; sizeXBuf < Program.config.windowSize.x; i++)
                                {
                                    sizeXBuf = x + scrX + sizeX * i;
                                    DX.DrawGraphF(sizeXBuf, sizeYBuf, handle, 1);
                                }
                                sizeXBuf = 0;
                            }
                        }
                        break;
                }
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255);
            }
        }
        public SongSelectBgImage(string fp) : base(fp)
        {
        }
    }
}