using Base;
using FairyGUI;
using Logic.Sigin.Com;
using Logic.Bag;
using Logic.Common;
using Message.Activity;
using Message.Bag;
using ServerLink;
using System;
using System.Collections.Generic;
using Logic.Sigin;
using UnityEngine;
using Data.Beans;
using Logic.ActivityPirateTreasure;

namespace HongDongPirateTreasure
{
    public sealed partial class UI_IconListItem
    {
        int listIndex;
        public PirateTreasureData ptData;
        bool isInit = false;
        public void Init(int index, PirateTreasureData baseData)
        {
            long tick = DateTime.Now.Ticks;
            ptData = baseData;
            listIndex = index;
            m_itemList.SetVirtual();
            m_itemList.itemRenderer = ItemIconRender;
            m_itemListBg.SetVirtual();
            m_itemListBg.itemRenderer = ItemBgRender;
            m_itemListBg1.SetVirtual();
            m_itemListBg1.itemRenderer = ItemBgRender;
        }
        public void InitBgList()
        {
            if (!isInit)
            {
                m_itemListBg.numItems = 18;
                m_itemListBg1.numItems = 18;
                m_itemListBg.numItems = 18;
                m_itemListBg1.numItems = 18;
                isInit = !isInit;
            }
        }
        private void ItemBgRender(int index,GObject obj)
        {
            var item = obj as UI_TreasureItemBg;
            if (item == null)
                return;
            string[] iconBg = ptData.itemplate.Split('+');
            if (iconBg.Length == 2)
            {
                UIGloader.SetUrl(item.m_bg1, ClientTools.GetLoaderUrl(HongDongPirateTreasurePackage.packageId, iconBg[0]));
                UIGloader.SetUrl(item.m_bg2, ClientTools.GetLoaderUrl(HongDongPirateTreasurePackage.packageId, iconBg[0]));
            }
        }
        public void RefreshList(int length)
        {
            //m_itemList.RemoveChildren(0, -1, true);
            for (int i = 0; i < length; i++)
            {
                UI_TreasureItem item = null;
                if (i < m_itemList.numItems)
                {
                    item = m_itemList.GetChildAt(i) as UI_TreasureItem;
                }
                else
                {
                    item = WinMgr.CreateInstance<UI_TreasureItem>(HongDongPirateTreasurePackage.packageId);
                }
                string[] iconBg = ptData.itemplate.Split('+');
                if (iconBg.Length == 2)
                {
                    UIGloader.SetUrl(item.m_bg1, ClientTools.GetLoaderUrl(HongDongPirateTreasurePackage.packageId, iconBg[0]));
                    UIGloader.SetUrl(item.m_bg2, ClientTools.GetLoaderUrl(HongDongPirateTreasurePackage.packageId, iconBg[0]));
                }

                if (!ptData.hasCurrentMainReward())
                {
                    if (listIndex == 2 && (i % 9) * 4 + listIndex == 18)
                    {
                        item.Init();
                    }
                    else
                    {
                        ItemInfo itemInfo = ActivityService.Singleton.Data.GetBasePTItemByIndex((i % 9) * 4 + listIndex, ptData.baseItemId);
                        if (itemInfo != null)
                        {
                            item.Init(itemInfo, false);
                        }
                        else
                            item.visible = false;
                    }
                }
                else
                {
                    PirateTreasureRewardGrid itemInfo = ActivityService.Singleton.Data.GetItemInfoByIndex(i % (int)PTListPos.Nine + 1 + (listIndex - 1) * 9);
                    if (itemInfo == null)
                    {
                        item.Init(i % (int)PTListPos.Nine + 1 + (listIndex - 1) * 9);
                        item.onClick.Set(() =>
                        {
                            if (BagService.Singleton.GetItemCount(ptData.voucherId) > 0)
                            {
                                PirateTreasureService.Singleton.OnReqPickOne(item.index, ptData.id);
                                return;
                            }
                            else if (ptData.maxDiamondNum > ptData.diamondNum)
                            {
                                if (BagService.Singleton.GetItemCount((int)CurrencyType.Diamond) >= ptData.currentPrice)
                                {
                                    ConfirmWindow.Singleton.ShowTip(string.Format(4190016.GetItsLanaugeStr(), ptData.currentPrice), () =>
                                    {
                                        PirateTreasureService.Singleton.OnReqPickOne(item.index, ptData.id);
                                    });
                                }
                                else
                                    TipWindow.Singleton.ShowTip(1090022.GetItsLanaugeStr());
                            }
                            else
                                TipWindow.Singleton.ShowTip(4190028.GetItsLanaugeStr());
                        });
                    }
                    else
                    {
                        item.Init(itemInfo.itemInfo, itemInfo.mainReward);
                    }
                }
               
                if (i >= m_itemList.numItems)
                    m_itemList.AddChild(item);
            }
        }
        private void ItemIconRender(int index, GObject obj)
        {
            var item = obj as UI_TreasureItem;
            if (item == null)
                return;
            item.onClick.Clear();
            //洗牌itemBg动效结束时，item为(0,0.98),代码还原x
            //item.m_item.scaleX = 0.98f;
            item.m_item.scaleX = 0.88f;
            string[] iconBg = ptData.itemplate.Split('+');
            if (iconBg.Length == 2)
            {
                UIGloader.SetUrl(item.m_bg1, ClientTools.GetLoaderUrl(HongDongPirateTreasurePackage.packageId, iconBg[0]));
                UIGloader.SetUrl(item.m_bg2, ClientTools.GetLoaderUrl(HongDongPirateTreasurePackage.packageId, iconBg[0]));
            }
            item.m_item.m_roundEft.visible = false;
            if (!ptData.hasCurrentMainReward())
            {
                if (listIndex == 2 && (index % 9) * 4 + listIndex == 18)
                {
                    item.Init();
                }
                else
                {
                    ItemInfo itemInfo = ActivityService.Singleton.Data.GetBasePTItemByIndex((index % 9) * 4 + listIndex, ptData.baseItemId);
                    if (itemInfo != null)
                    {
                        item.Init(itemInfo, false);
                    }
                    else
                        item.visible = false;
                }
            }
            else
            {
                PirateTreasureRewardGrid itemInfo = ActivityService.Singleton.Data.GetItemInfoByIndex(index % (int)PTListPos.Nine + 1 + (listIndex-1)*9);
                if (itemInfo == null)
                {
                    item.Init(index % (int)PTListPos.Nine + 1 + (listIndex - 1) * 9);
                    item.onClick.Set(() =>
                    {
                        if (BagService.Singleton.GetItemCount(ptData.voucherId) > 0)
                        {
                            PirateTreasureService.Singleton.OnReqPickOne(item.index, ptData.id);
                            return;
                        }
                        else if (ptData.maxDiamondNum > ptData.diamondNum)
                        {
                            if (BagService.Singleton.GetItemCount((int)CurrencyType.Diamond) >= ptData.currentPrice)
                            {
                                ConfirmWindow.Singleton.ShowTip(string.Format(4190016.GetItsLanaugeStr(), ptData.currentPrice), () =>
                                {
                                    PirateTreasureService.Singleton.OnReqPickOne(item.index, ptData.id);
                                });
                            }
                            else
                                TipWindow.Singleton.ShowTip(1090022.GetItsLanaugeStr());
                        }
                        else
                        {
                            t_itemBean itemBean = ConfigBean.GetBean<t_itemBean, int>(ptData.voucherId);
                            TipWindow.Singleton.ShowTip(string.Format(4190028.GetItsLanaugeStr(), itemBean.t_name));
                        }
                    });
                }
                else
                {
                    item.Init(itemInfo.itemInfo, itemInfo.mainReward);
                    item.m_item.m_roundEft.visible = itemInfo.mainReward;
                }
            }
           
        }
        public void PlayStarAni(bool ani)
        {
            for (int i = 0; i < m_itemList.numItems; i++)
            {
                var item = m_itemList.GetChildAt(i) as UI_TreasureItem;
                if (item != null)
                {
                    item.m_aniBg.Play(1, 0, () => { item.m_wenhao.visible = false; });
                }
            }
            m_itemList.RefreshVirtualList();
        }

        public void RefreshPickOne(GComponent parent,GameObject parentObj)
        {
            for (int i = 0; i < m_itemList.numItems; i++)
            {
                var item = m_itemList.GetChildAt(i) as UI_TreasureItem;
                if (item != null && ActivityService.Singleton.Data.curRewardGrid.index == item.index)
                {
                    item.m_item.Init(ActivityService.Singleton.Data.curRewardGrid.itemInfo.id, ActivityService.Singleton.Data.curRewardGrid.itemInfo.num);
                    //播放显示donghua,动画播放完成打开奖励界面
                    item.m_item.m_roundEft.visible = ActivityService.Singleton.Data.curRewardGrid.mainReward;
                    item.m_item.m_getIcon.visible = false;
                    item.m_item.visible = true;
                    //item.m_bgGroup.visible = false;
                    if (ActivityService.Singleton.Data.curRewardGrid.mainReward)
                    {
                        if (parentObj != null)
                        {
                            ResPack resPack = new ResPack(this);
                            GameObject modelEff = resPack.LoadGo("eff_ui_haizeibaoxiang_burst");
                            GoWrapper wrapper = new GoWrapper(modelEff);
                            item.m_bigModel.SetNativeObject(wrapper);
                            modelEff.transform.localPosition = new Vector3(-15, -40, 0);
                            parentObj.transform.position = modelEff.transform.position;
                            parentObj.SetActive(true);
                        }
                      
                        item.m_aniSpecial.Play(1, 0, () =>
                        {
                            TwoParam<List<ItemInfo>, List<long>> param = new TwoParam<List<ItemInfo>, List<long>>();
                            List<ItemInfo> items = new List<ItemInfo>();
                            items.Add(ActivityService.Singleton.Data.curRewardGrid.itemInfo);
                            param.value1 = items;
                            param.value2 = new List<long>();
                            WinMgr.Singleton.Open<ItemGetWindow>(Common.CommonPackage.packageId, UILayer.TopHUD, false, param);
                            var winodow = parent as UI_PirateTreasureWindow;
                            if (winodow != null)
                                winodow.m_mask.visible = false;
                            //parent.touchable = true;
                            parentObj.SetActive(false);
                        });
                    }
                    else
                    {
                        item.m_aniShow.Play(1, 0, () =>
                        {
                            TwoParam<List<ItemInfo>, List<long>> param = new TwoParam<List<ItemInfo>, List<long>>();
                            List<ItemInfo> items = new List<ItemInfo>();
                            items.Add(ActivityService.Singleton.Data.curRewardGrid.itemInfo);
                            param.value1 = items;
                            param.value2 = new List<long>();
                            WinMgr.Singleton.Open<ItemGetWindow>(Common.CommonPackage.packageId, UILayer.TopHUD, false, param);
                            var winodow = parent as UI_PirateTreasureWindow;
                            if (winodow != null)
                                winodow.m_mask.visible = false;
                            //parent.touchable = true;
                        });
                    }
                    break;
                }
            }
        }
        public void RefreshPickOneKey(GComponent parent, GameObject parentObj, PirateTreasureRewardGrid targetGrid)
        {
            for (int i = 0; i < m_itemList.numItems; i++)
            {
                var item = m_itemList.GetChildAt(i) as UI_TreasureItem;
                if (item != null && targetGrid.index == item.index)
                {
                    item.m_item.Init(targetGrid.itemInfo.id, targetGrid.itemInfo.num);
                    //播放显示donghua,动画播放完成打开奖励界面
                    item.m_item.m_roundEft.visible = targetGrid.mainReward;
                    item.m_item.m_getIcon.visible = false;
                    item.m_item.visible = true;
                    //item.m_bgGroup.visible = false;
                    if (targetGrid.mainReward)
                    {
                        if (parentObj != null)
                        {
                            ResPack resPack = new ResPack(this);
                            GameObject modelEff = resPack.LoadGo("eff_ui_haizeibaoxiang_burst");
                            GoWrapper wrapper = new GoWrapper(modelEff);
                            item.m_bigModel.SetNativeObject(wrapper);
                            modelEff.transform.localPosition = new Vector3(-15, -40, 0);
                            parentObj.transform.position = modelEff.transform.position;
                            parentObj.SetActive(true);
                        }

                        item.m_aniSpecial.Play(1, 0, () =>
                        {
                            //PirateTreasureService.Singleton.oneKeyParam.value1.Add(targetGrid.itemInfo);
                            WinMgr.Singleton.Open<ItemGetWindow>(Common.CommonPackage.packageId, UILayer.TopHUD, false, PirateTreasureService.Singleton.oneKeyParam);

                            var winodow = parent as UI_PirateTreasureWindow;
                            if (winodow != null)
                                winodow.m_mask.visible = false; //parent.touchable = true;
                            WinMgr.Singleton.GetWindow<HongDongMainWindow>().View.m_pmask.visible = false;
                            parentObj.SetActive(false);
                        });
                    }
                    else
                    {
                        item.m_aniShow.Play(1, 0, () =>
                        {
                            //PirateTreasureService.Singleton.oneKeyParam.value1.Add(targetGrid.itemInfo);
                            //WinMgr.Singleton.Open<ItemGetWindow>(Common.CommonPackage.packageId, UILayer.TopHUD, false, param);
                        });
                    }
                    break;
                }
            }
        }
    }
}
