using Base;
using BusterCall;
using Common;
using Data.Beans;
using FairyGUI;
using Logic.Bag;
using Logic.Common;
using Logic.ShoppingMall;
using Logic.Sigin.Com;
using Message.Activity;
using ServerLink;
using ShoppingMall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Scripts.Logic.ActivityHall.CommonExchangeShop
{
    public class CommonExchangeShopWindow: BindWindow<UI_CommonExchangeShopWindow>
    {
        int activiId = 0;
        CommonExchangeShopData data = null;
        protected override void OnOpen()
        {
            base.OnOpen();
            activiId = (int)param;
            data = (CommonExchangeShopData)ActivityService.Singleton.Data.GetMainInfo(activiId);

            AddListener(EventID.CommonExchangeShopUpdate, RefreshView);

            View.m_close.onClick.Set(Close);

            InitView();
        }

        private void RefreshView(Event obj)
        {
            var id = (int)obj.Data;
            if (id == activiId)
                InitView();
        }

        void InitView()
        {
            View.m_AllContent.itemRenderer = ItemRenderer;
            View.m_AllContent.numItems = data.itemList.Count;

            View.m_listCost.itemRenderer = CostRenderer;
            View.m_listCost.numItems = data.iconList.Count;
        }

        private void ItemRenderer(int index, GObject obj)
        {
            UI_ShopItem item = obj as UI_ShopItem;
            var cur = data.itemList[index];
            t_itemBean iBean = ConfigBean.GetBean<t_itemBean, int>(cur.itemInfo.id);

            item.m_BiaoQian.visible = false;
            if (!cur.etraTitle.Equals("0"))
            {
                item.m_BiaoQian.visible = true;
                item.m_rate.text = int.Parse(cur.etraTitle).GetItsLanaugeStr();
            }
            item.m_VipMask.visible = false;
            item.m_Name.text = string.Format(ClientTools.GetTextColorrByQuality((ItemQuality)iBean.t_quality), iBean.t_name);
            UI_ItemIcon itemIcon = item.m_Item as UI_ItemIcon;
            if (itemIcon != null)
                itemIcon.Init(cur.itemInfo.id, cur.itemInfo.num, false);

            var leftCount = cur.limitNum - cur.buyNum;
            leftCount = leftCount >= 0 ? leftCount : 0;
            item.m_Over.visible = leftCount > 0;
            item.m_LimtCount.text = string.Format(1090012.GetItsLanaugeStr(), leftCount);

            if (item.m_LimtCount.visible)
            {
                item.m_Over.visible = cur.buyNum >= cur.limitNum;
            }
            else
            {
                item.m_Over.visible = false;
            }
            //消耗
            t_itemBean bean = ConfigBean.GetBean<t_itemBean, int>(cur.costInfo.id);
            UIGloader.SetUrl(item.m_Icon, bean.t_icon);
            var has = BagService.Singleton.GetItemCount(cur.costInfo.id);
            if (has >= cur.costInfo.num)
            {
                item.m_Money.text = string.Format("[color=#ffffff]{0}[/color]", cur.costInfo.num);
            }
            else
            {
                item.m_Money.text = string.Format("[color=#ff0000]{0}[/color]", cur.costInfo.num);
            }
            item.onClick.Set(() =>
            {
                if (!WinMgr.Singleton.HasOpen<BuyShopWindow1>())
                    WinMgr.Singleton.Open<BuyShopWindow1>(ShoppingMallPackage.packageId, UILayer.TopHUD,
                        false, cur.itemInfo.id);
                BuyShopWindow1.Singleton.CommonExchangeShopBuy(activiId, cur.itemId, cur.itemInfo, cur.costInfo, leftCount);
            });
        }


        private void CostRenderer(int index, GObject item)
        {
            var com = item as UI_comCostItem;
            var cur = data.iconList[index];
            t_itemBean bean = ConfigBean.GetBean<t_itemBean, int>(cur);
            UIGloader.SetUrl(com.m_Icon, bean.t_icon);
            com.m_Money.text = BagService.Singleton.GetItemCount(cur).ToString();
        }
    }
}
