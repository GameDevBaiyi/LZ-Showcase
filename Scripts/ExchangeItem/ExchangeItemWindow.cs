using Common;
using Data.Beans;
using HongDongDrop;
using Logic.Common;
using Logic.Role;
using Logic.ShoppingMall;
using Logic.Sigin.Com;
using Message.Activity;
using ServerLink;
using ShoppingMall;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.ActivityHall.ExchangeItem
{
    public class ExchangeItemWindow : BindWindow<UI_ExchangeItemWindow>
    {
        int activityID;
        long tickID;
        ExchangeItemData mainInfo;
        List<ExchangeItemCell> cellList;

        protected override void OnClose()
        {
            base.OnClose();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            activityID = (int)param;
            InitData();
            InitView();
            InitList();
            StartTick();
            AddListener(EventID.ExchangeItemUpdate, UpdateView);
        }

        private void InitData()
        {
            mainInfo = ActivityService.Singleton.Data.GetMainInfo(activityID) as ExchangeItemData;
            if (cellList == null)
                cellList = new List<ExchangeItemCell>();
            cellList.Clear();
            cellList.AddRange(mainInfo.items);
        }

        private void InitView()
        {
            View.m_Help.visible = int.TryParse(mainInfo.rule, out var ruleID);
            View.m_Help.onClick.Set(() =>
            {
                var param = new Base.TwoParam<int, int> { value1 = -1, value2 = ruleID };
                //WinMgr.Singleton.Open<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
                //活动结束，子窗口一起自动关闭
                OpenChild<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
            });
            if (int.TryParse(mainInfo.title, out var title) && title != 0)
                UIGloader.SetUrl(View.m_titleLoader, "ui://HongDongDrop/" + title.GetItsLanaugeStr());
            else
                UIGloader.SetUrl(View.m_titleLoader, "");

            if (int.TryParse(mainInfo.desc, out var desc) && desc != 0)
                View.m_Des.text = desc.GetItsLanaugeStr();
            else if (mainInfo.desc != "0")
                View.m_Des.text = mainInfo.desc;
            else
                View.m_Des.text = "";

            ExchangeItemCell cell = cellList[0];
            int[][] costArrs = cell.costs.SplitTo2IntArray(';', '+');
            UIGloader.SetUrl(View.m_Token, ClientTools.GetItemIcon(costArrs[0][0]));
            View.m_Count.text = ClientTools.GetItemNumStr(Bag.BagService.Singleton.GetItemCount(costArrs[0][0]));
            var bean = ConfigBean.GetBean<Data.Beans.t_itemBean, int>(costArrs[0][0]);
            View.m_Add.visible = false;
            if (bean != null)
            {
                View.m_Add.visible = string.IsNullOrEmpty(bean.t_use_jump_ly);
                View.m_Add.onClick.Add(() =>
                {
                    ItemSourceParam param = new ItemSourceParam();
                    param.itemId = bean.t_id;
                    //WinMgr.Singleton.Open<ItemSourceWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
                    //活动结束，子窗口一起自动关闭
                    OpenChild<ItemSourceWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
                });
            }
        }

        private void InitList()
        {
            //View.m_DropList.itemRenderer = RenderList;
            //View.m_DropList.numItems = cellList.Count;
            //View.m_DropList.SetVirtual();
            RenderList();
        }

        private void RenderList()
        {
            View.m_DropList.RemoveChildrenToPool();
            foreach (var cell in cellList)
            {
                var item = View.m_DropList.AddItemFromPool() as UI_ActiveShopItem;
                var info = cell;

                //int改为string，是为了保证TimeLimitConvertWnd能用。这个掉落兑换活动，还是保持只有1对1道具兑换
                int[][] costArrs = info.costs.SplitTo2IntArray(';', '+');
                item.m_Money.text = costArrs[0][1].ToString();
                UIGloader.SetUrl(item.m_Icon, ClientTools.GetItemIcon(costArrs[0][0]));

                int[][] itemArrs = info.items.SplitTo2IntArray(';', '+');
                var bean = ConfigBean.GetBean<t_itemBean, int>(itemArrs[0][0]);
                if (bean != null)
                {
                    item.m_Name.text = string.Format(ClientTools.GetTextColorrByQuality((ItemQuality)bean.t_quality),
                        bean.t_name);
                    item.m_Item.Init(bean.t_id, itemArrs[0][1], false);
                }

                item.m_LimtCount.visible = false;
                item.m_VipMask.visible = false;
                if (string.IsNullOrEmpty(info.see))
                {
                    var lvLimit = Tools.SplitStringToIntArray(info.see);
                    if (lvLimit.Length >= 2)
                    {
                        var role = RoleService.Singleton.RoleInfo;
                        if (lvLimit[0] == 1)
                        {
                            if (lvLimit[1] > role.level)
                            {
                                item.m_VipMask.visible = true;
                                item.m_Vip.text = $"Lv{lvLimit[1]}";
                            }
                        }
                        else if (lvLimit[0] == 2)
                        {
                            if (lvLimit[1] > role.vipLevel)
                            {
                                item.m_VipMask.visible = true;
                                item.m_Vip.text = $"VIP{lvLimit[1]}";
                            }
                        }
                    }
                }

                if (info.max != 0)
                {
                    item.m_LimtCount.visible = true;
                    item.m_LimtCount.text = string.Format(1090012.GetItsLanaugeStr(), info.max - info.num);
                }

                item.m_Over.visible = info.max != 0 && info.max <= info.num;
                item.m_BiaoQian.visible = info.describe != 0;
                item.m_rate.text = info.describe != 0 ? info.describe.GetItsLanaugeStr() : "";
                item.onClick.Set(() =>
                {
                    switch ((ItemType)bean.t_tab)
                    {
                        case ItemType.Equip:
                        {
                            if (!WinMgr.Singleton.HasOpen<BuyShopWindow2>())
                            {
                                //WinMgr.Singleton.Open<BuyShopWindow2>(ShoppingMallPackage.packageId, UILayer.Popup, false, bean.t_id);
                                //活动结束，子窗口一起自动关闭
                                OpenChild<BuyShopWindow2>(ShoppingMallPackage.packageId, UILayer.Popup, false,
                                    bean.t_id);
                            }

                            BuyShopWindow2.Singleton.ActiveShop(new int[] { costArrs[0][0], costArrs[0][1] },
                                new int[] { itemArrs[0][0], itemArrs[0][1] }, info.num, info.max,
                                (int num) =>
                                {
                                    ExchangeItemService.Singleton.OnReqExchangeItem(activityID, info.id, num);
                                },
                                !(info.max != 0 && info.max <= info.num));
                        }
                            break;
                        default:
                        {
                            if (!WinMgr.Singleton.HasOpen<BuyShopWindow1>())
                            {
                                //WinMgr.Singleton.Open<BuyShopWindow1>(ShoppingMallPackage.packageId, UILayer.Popup, false);
                                //活动结束，子窗口一起自动关闭
                                OpenChild<BuyShopWindow1>(ShoppingMallPackage.packageId, UILayer.Popup, false);
                            }

                            BuyShopWindow1.Singleton.ActiveShop(new int[] { costArrs[0][0], costArrs[0][1] },
                                new int[] { itemArrs[0][0], itemArrs[0][1] }, info.num, info.max,
                                (int num) =>
                                {
                                    ExchangeItemService.Singleton.OnReqExchangeItem(activityID, info.id, num);
                                },
                                !(info.max != 0 && info.max <= info.num));
                        }
                            break;
                    }
                });
            }
        }

        private void UpdateView(Base.Event evt)
        {
            InitData();
            InitView();
            InitList();
            StartTick();
        }

        private void StartTick()
        {
            if (tickID != 0)
            {
                StopTimerOrCoroutine(tickID);
                tickID = 0;
            }

            tickID = AddTimer(1f, Tick);
            Tick();
        }

        private void Tick()
        {
            var now = TimeUtils.currentTick;
            if (now > mainInfo.overTime)
            {
                StopTimerOrCoroutine(tickID);
                tickID = 0;
                InitList();
                View.m_Timer.text = 4120011.GetItsLanaugeStr();
            }
            else
            {
                View.m_Timer.text = ClientTools.GetHDTimeStr(new TimeSpan(mainInfo.overTime - now));
            }
        }
    }
}