using BusterCall;
using Data.Beans;
using FairyGUI;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.UI_ItemIcon;

namespace GameLogic.Scripts.Logic.ActivityHall.BusterCall
{
    public class BusterCallRewardWindow:BindWindow<UI_BusterCallRewardWindow>
    {
        int activiId = 0;
        BusterCallData data = null;
        protected override void OnOpen()
        {
            base.OnOpen();

            InitView();
        }

        void InitView()
        {
            View.m_close.onClick.Set(Close);

            activiId = (int)param;
            data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(activiId);

            var rewardList = ConfigBean.GetBeanList<t_bustercall_rewardBean>();
            View.m_list.itemRenderer = RewardItemRenderer;
            View.m_list.numItems = rewardList.Count;
        }

        int[] tempArr = { };
        private void RewardItemRenderer(int index, GObject obj)
        {
            var rewardList = ConfigBean.GetBeanList<t_bustercall_rewardBean>();
            var com = obj as UI_comRewardItem;
            var cur = rewardList[index];
            if (cur.t_boss_hp.Contains("+"))
            {
                var rang = cur.t_boss_hp.SplitToIntArray('+');
                com.m_txtRang.text = string.Format("{0}%-{1}%", rang[0], rang[1]);
            }
            else
            {
                com.m_txtRang.text = 4200020.GetItsLanaugeStr();
            }
            tempArr = cur.t_reward.SplitToIntArray('+');
            com.m_list.itemRenderer = ItemRenderer;
            com.m_list.numItems = data.iconList.Count;
            com.m_list.scrollPane.touchEffect = data.iconList.Count >= 3;
        }

        private void ItemRenderer(int index, GObject item)
        {
            var com = item as UI_comItem;
            int num = 0;
            if (tempArr.Length > index)
            {
                num = tempArr[index];
            }
            com.m_txtNum.text = string.Format("X{0}", num);
            com.m_item.Init(data.iconList[index]);
        }
    }
}
