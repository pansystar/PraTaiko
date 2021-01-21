using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pansystar.Extensions;

namespace PraTaiko
{
    /// <summary>
    /// シーン内の更新・描画のインターフェース
    /// </summary>
    /// <remarks>
    /// ImageManagementクラス以外のクラスが用いる場合、
    /// 明示的に実装し、アクセス修飾子を「public」にしないこと
    /// </remarks>
    interface IGameAction
    {
        void Update();
        void Draw();
        void Finish();
    }
    /// <summary>
    /// シーン内のコントロール
    /// </summary>
    class CGameActionControl:IDisposable
    {
        List<Action> Updates;
        List<Action> Draws;
        List<Action> Finishs;

        public void AddAction(IGameAction gameAction)
        {
            Updates.Add(gameAction.Update);
            Draws.Add(gameAction.Draw);
            Finishs.Add(gameAction.Finish);
        }

        public void Update() { Updates.ForEach(u => u()); }
        public void Draw() { Draws.ForEach(d => d()); }
        public void Finish() { Finishs.ForEach(f => f()); }

        public static CGameActionControl Construct(BaseScene s)
        {            
            return new CGameActionControl();
        }

        public void Dispose()
        {
            Updates.Clear();
            Draws.Clear();

            Updates = null;
            Draws = null;
        }

        CGameActionControl()
        {
            Updates = new List<Action>();
            Draws = new List<Action>();
            Finishs = new List<Action>();
        }
    }
}
