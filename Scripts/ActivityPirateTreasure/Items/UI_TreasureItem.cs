using Message.Bag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HongDongPirateTreasure
{
    public sealed partial class UI_TreasureItem
    {
        public int index;
        public void Init(int index)
        {
            this.index = index;
            m_item.visible = false;
            m_bgGroup.visible = true;
            m_reciveGroup.visible = false;
            m_wenhao.visible = false;
            m_jia.visible = false;
        }
        public void Init(ItemInfo itemInfo,bool isMain)
        {
            m_item.Init(itemInfo.id, itemInfo.num);
            m_item.m_getIcon.visible = false;
            m_item.visible = true;
            m_bgGroup.visible = false;
            m_reciveGroup.visible = false;
            m_jia.visible = false;
            m_wenhao.visible = false;
        }
        public void Init(bool isTitle = false)
        {
            m_item.visible = false;
            m_jia.visible = isTitle;
            m_reciveGroup.visible = false;
            m_wenhao.visible = !isTitle;
            m_wenhao1.SetScale(1, 1);
            m_wenhao2.SetScale(1, 1);
            m_bgGroup.visible = !isTitle;
        }
    }
}
