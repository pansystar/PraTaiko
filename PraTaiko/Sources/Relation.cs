using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pansystar.Extensions;

namespace PraTaiko
{
    /// <summary>
    /// 各クラスに関連を付けるインターフェース
    /// </summary>
    interface IRelation
    {
        void Link();
    }

    /// <summary>
    /// 関連付けクラス
    /// </summary>
    class CRelation
    {
        bool state = false;
        List<Action> Links = new List<Action>();
        public void Add(IRelation relation)
        {
            Links.Add(relation.Link);
        }
        public void Start()
        {
            if (!state)
            {
                Links.ForEach(l => l());
            }
            else
            {
                PrintMessage("Relational.Startは複数回呼び出し禁止。");
            }
            Links.Clear();
            state = true;
        }
        public void Clear()
        {
            Links.Clear();
            state = false;
        }
        public static CRelation Construct()
        {
            return new CRelation();
        }
    }
}
