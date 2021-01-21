using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
namespace PraTaiko
{
	class CSceneOut : Image
	{
		static COldConfig OldConfig;
		new int moveY;
		int moveYDiff = -22;
		bool drawOK = false;
		public CSceneOut(string fp, int diff) : base(fp)
		{
			OldConfig = COldConfig.Get();
			moveY = 0;
			moveYDiff = diff;
		}
		public void Reset()
		{            
			moveY = 0;
            DX.DeleteGraph(handle);
		}
		public void Start()
		{
			drawOK = true;
			Key.SetKeyAcqu(false);
		}
		public void Draw(EScene next)
		{
			if (drawOK)
			{
				if (sizeY > -moveY)
				{
					DX.DrawRotaGraph(OldConfig.DrawSize.X / 2, OldConfig.DrawSize.Y / 2 + sizeY + moveY, 1.0, 0.0, handle, 1);
					moveY += moveYDiff;
				}
				else
				{
					DX.DrawRotaGraph(OldConfig.DrawSize.X / 2, OldConfig.DrawSize.Y / 2 + sizeY + moveY, 1.0, 0.0, handle, 1);
					Key.SetKeyAcqu(true);
					drawOK = false;
					NScene.SetNext(this, next);
				}
			}
		}
	}
	class CSceneIn : Image
	{
		static COldConfig MainConfig;
		new int moveY;
		int moveYDiff;
		public CSceneIn(string fp, int diff) : base(fp)
		{
			MainConfig = COldConfig.Get();
			moveY = 0;
			moveYDiff = diff;
		}
        public void Reset()
        {
            DX.DeleteGraph(handle);
        }
        public new void Draw()
		{
			DX.DrawRotaGraph(MainConfig.DrawSize.X / 2, MainConfig.DrawSize.Y / 2 + moveY, 1.0, 0.0, handle, 1);
			moveY += moveYDiff;
		}

	}
}
