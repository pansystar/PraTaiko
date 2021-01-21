using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace PraTaiko
{
    public static class Origin
    {
        public static int SongFont;
        public static int SongFont2;
        public static int SongFont3;
        public static void Initialize()
        {
            SongFont = DX.CreateFontToHandle("ＤＦＰ勘亭流", 34, -1, DX.DX_FONTTYPE_ANTIALIASING_EDGE_4X4, -1, 3);
            SongFont2 = DX.CreateFontToHandle("ＤＦＰ勘亭流", 30, -1, DX.DX_FONTTYPE_ANTIALIASING_EDGE_4X4, -1, 3);
            SongFont3 = DX.CreateFontToHandle("ＤＦＰ勘亭流", 45, -1, DX.DX_FONTTYPE_ANTIALIASING_EDGE_4X4, -1, 3);
            DX.SetFontSpaceToHandle(-4, SongFont2);
        }
        public static int CountChar(string s, char c)
        {
            return s.Length - s.Replace(c.ToString(), "").Length;
        }
        public static int GetDrawExtendVStringHeight(double ExRateY,string str,int fh)
        {
            float tempY = 0;
            int sy;
            foreach (var c in str)
            {
                sy = (int)(DX.GetFontSizeToHandle(fh) * ExRateY);
                switch (c)
                {
                    case '-':
                    case 'ー':
                    case '～':
                        //sx = (int)(sx / 1.5F);
                        //sx += (int)(3 * sx * (1.0 - ExRateY));
                        //DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, (int)(y + sy / 2 * ExRateY) + tempY, 1.0, ExRateY, sy / 2, sy / 2, 0 * Math.PI / 180, color, fh, eColor, 1, c.ToString());
                        break;
                    case '、':
                        sy /= 2;
                        //DX.DrawExtendStringFToHandle(x, y - sy + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                    case '…':
                        //sx /= 10;
                        //DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, y + sy + tempY, ExRateX, ExRateY, sy, sy, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '.':
                        sy /= 3;
                        //DX.DrawExtendStringFToHandle(x, y - 2 * sy + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                    case '\'':
                        sy /= 3;
                        //DX.DrawExtendStringFToHandle(x, y + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                    case '･':
                        //sx = (int)(sx / 1.5F);
                        sy /= 4;
                        //DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, y + sy + tempY, ExRateX, ExRateY, sy / 2, sy / 2, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '・':
                        //sx /= 2;
                        sy /= 2;
                        //DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, y + sy + tempY, ExRateX, ExRateY, sy, sy, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '【':
                    case '】':
                        break;
                    case ' ':
                    case '　':
                        sy /= 2;
                        //DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sy, y + sy + tempY, ExRateX, ExRateY, sy, sy, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    default:
                        //DX.DrawExtendStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2, y + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                }
                //DX.GetDrawExtendStringSizeToHandle(out sx, out sy, out l, ExRateX, ExRateY, c.ToString(), 1, fh);
                tempY += sy;
            }
            return (int)tempY;
        }
        public static void DrawExtendVStringToHandle(int x, int y, double ExRateX, double ExRateY, string str, uint color, int fh, uint eColor)
        {
            float tempY = 0;
            int sx, sy;
            foreach (var c in str)
            {
                sx = sy = (int)(DX.GetFontSizeToHandle(fh) * ExRateY);
                switch (c)
                {
                    case '～':
                        sx = (int)(sx / 1.5F);
                        sx += (int)(3 * sx * (1.0 - ExRateY));
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2+sx, (int)(y + sy * ExRateY) + tempY, 1.0, ExRateY, sy / 2, sy / 2, 0 * Math.PI / 180, color, fh, eColor, 1, c.ToString());
                        break;
                    case '-':
                    case 'ー':
                        sx = (int)(sx / 1.5F);
                        sx += (int)(3*sx * (1.0 - ExRateY));
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, (int)(y + sy / 2 * ExRateY) + tempY, 1.0, ExRateY, sy / 2, sy / 2, 0 * Math.PI / 180, color, fh, eColor, 1, c.ToString());
                        break;
                    case '、':
                        sy /= 2;
                        DX.DrawExtendStringFToHandle(x, y - sy + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                    case '…':
                        sx /= 10;
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, y + sy + tempY, ExRateX, ExRateY, sy, sy, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '.':
                        sy /= 3;
                        DX.DrawExtendStringFToHandle(x, y - 2 * sy + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                    case '\'':
                        sy /= 3;
                        DX.DrawExtendStringFToHandle(x, y  + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                    case '･':
                        sx = (int)(sx / 1.5F);
                        sy /= 4;
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, y + sy + tempY, ExRateX, ExRateY, sy / 2, sy / 2, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '・':
                        sx /= 2;
                        sy /= 2;
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sx, y + sy + tempY, ExRateX, ExRateY, sy, sy, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '(':
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + 5*sy/3, y + sy/2 + tempY, ExRateY, ExRateX, sy / 2, sy / 2, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '【':
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sy / 2, y + sy + tempY, ExRateX, ExRateY, sy / 2, sy / 2, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case ')':
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + 5*sy/3, y + sy / 2 + tempY, ExRateY, ExRateX, sy / 2, sy / 2, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case '】':
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sy/2, y + sy/2 + tempY, ExRateX, ExRateY, sy/2, sy/2, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    case ' ':
                    case '　':
                        sy /= 2;
                        DX.DrawRotaStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2 + sy, y + sy + tempY, ExRateX, ExRateY, sy, sy, 90 * Math.PI / 180, color, fh, eColor, 0, c.ToString());
                        break;
                    default:
                        DX.DrawExtendStringFToHandle(x - DX.GetDrawStringWidthToHandle(c.ToString(), 1, fh) / 2, y + tempY, ExRateX, ExRateY, c.ToString(), color, fh, eColor);
                        break;
                }
                //DX.GetDrawExtendStringSizeToHandle(out sx, out sy, out l, ExRateX, ExRateY, c.ToString(), 1, fh);
                tempY += sy;
            }
        }
        public static void DrawVStringToHandle(int x, int y, string str, uint color, int fh, uint eColor)
        {
            int bufY = 0;
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case ' ':
                        bufY += DX.GetFontSizeToHandle(fh) / 2;
                        break;
                    case '\'':
                        DX.DrawStringToHandle(x, y + bufY, str[i].ToString(), color, fh, eColor);
                        bufY += DX.GetFontSizeToHandle(fh) / 3;
                        break;
                    case '!':
                        if (i != str.Length - 1)
                        {
                            DX.DrawStringToHandle(x - 3, y + bufY, str[i].ToString(), color, fh, eColor);
                            DX.DrawStringToHandle(x + 4 - DX.GetDrawStringWidthToHandle(str[i].ToString(), str[i].ToString().Length, fh), y + bufY, str[i].ToString(), color, fh, eColor);
                            i++;
                        }
                        else
                        {
                            DX.DrawStringToHandle(x - DX.GetDrawStringWidthToHandle(str[i].ToString(), str[i].ToString().Length, fh) / 2, y + bufY, str[i].ToString(), color, fh, eColor);
                        }
                        bufY += DX.GetFontSizeToHandle(fh);
                        break;
                    case '、':
                        DX.DrawStringToHandle(x, y + bufY - 36 / 2, str[i].ToString(), color, fh, eColor);
                        bufY += DX.GetFontSizeToHandle(fh) / 2;
                        break;
                    case '･':
                        DX.DrawStringToHandle(x - DX.GetDrawStringWidthToHandle(str[i].ToString(), str[i].ToString().Length, fh) / 2, y + bufY - 36 / 3, str[i].ToString(), color, fh, eColor);
                        bufY += DX.GetFontSizeToHandle(fh) / 3;
                        break;
                    case '-':
                    case 'ー':
                        DX.DrawStringToHandle(x - DX.GetDrawStringWidthToHandle('l'.ToString(), 'l'.ToString().Length, fh) / 2, y + bufY, 'l'.ToString(), color, fh, eColor);
                        bufY += DX.GetFontSizeToHandle(fh);
                        break;
                    default:
                        DX.DrawStringToHandle(x - DX.GetDrawStringWidthToHandle(str[i].ToString(), str[i].ToString().Length, fh) / 2, y + bufY, str[i].ToString(), color, fh, eColor);
                        bufY += DX.GetFontSizeToHandle(fh);
                        break;
                }
            }
        }
        static Origin()
        {
        }
    }
}

