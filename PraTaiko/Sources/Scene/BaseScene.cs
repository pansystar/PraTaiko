using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraTaiko
{
	abstract class BaseScene
	{
		protected enum EState { Null = 0, OK }
		protected EState State { get; set; }
        protected static COldConfig OldConfig;
        protected static CMainConfig MainConfig;
		public abstract void Start();
		public abstract void Update();
		public abstract void Draw();
		public abstract void Finish();
        public BaseScene()
        {
            if (OldConfig == null) OldConfig = COldConfig.Get();
            if (MainConfig == null) MainConfig = CMainConfig.Get();
        }
	}
}
