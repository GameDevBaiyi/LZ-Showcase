using Base;
using Common;
using FairyGUI;
using Logic.Sigin.Com;
using HongDongPirateTreasure;
using Logic.Bag;
using Logic.ChongZhi;
using Logic.Common;
using Logic.MainCity;
using Logic.Role;
using Message.Activity;
using Message.Bag;
using ServerLink;
using System;
using System.Collections;
using UnityEngine;
using ChongZhi;
using Logic.ActivityPirateTreasure;
using System.Collections.Generic;
using Logic.Task;

namespace Logic.Sigin
{
    class PirateTreasureWindow : BindWindow<UI_PirateTreasureWindow>
    {
        long timeer = 0;
        PirateTreasureData ptData;
        public GameObject modelEff;
        protected override void OnOpen()
        {
            base.OnOpen();
            View.m_closeBtn.onClick.Add(Close);
            PlayerPrefs.SetString(RoleService.Singleton.RoleInfo.roleId.ToString() + ((int)param).ToString(), TimeUtils.currentTick.ToString());
            RedDotManager.Singleton.SetRedDotValue(RedPath.Pirate + $"/{(int)param}", false);
            //ActivityService.Singleton.Data.SetMianHongDongRed();
            RefreshPtData();
            if (ptData == null)
                Close();
            SetListDrag();
            InitTime();
            InitModelEff();
            InitText();
            RefreshRoundText();
            RefreshMainReward();
            RegisterEvent();
            RefrshList();
            RefreshDiamondNum();
            RefreshUseItemNum();
            RefreshNextBtnVisible();
            RefreshOneKeyBtnVisible();
        }
        private void InitModelEff()
        {
            modelEff = resLoader.LoadGo("eff_ui_haizeibaoxiang_burst");
            GoWrapper wrapper = new GoWrapper(modelEff);
            View.m_modelEff.SetNativeObject(wrapper);
            modelEff.SetActive(false);
        }
        private void RefreshPtData()
        {
            ptData = ActivityService.Singleton.Data.GetMainInfo((int)param) as PirateTreasureData;
            if (ptData == null || ptData.closeTime <= TimeUtils.currentTick)
            {
                TipWindow.Singleton.ShowTip(4120011);
                Close();
            }
        }
        //设置列表不可拖拽
        private void SetListDrag()
        {
            View.m_listOne.m_itemList.scrollPane.touchEffect = false;
            View.m_listTwo.m_itemList.scrollPane.touchEffect = false;
            View.m_listThree.m_itemList.scrollPane.touchEffect = false;
            View.m_listFour.m_itemList.scrollPane.touchEffect = false;
        }
        //设置奖励列表显示
        private void RefrshList()
        {
            View.m_listOne.m_itemList.numItems = 9;
            View.m_listTwo.m_itemList.numItems = 9;
            View.m_listThree.m_itemList.numItems = 9;
            View.m_listFour.m_itemList.numItems = 9;
        }
        //设置下一轮按钮的状态显示
        private void RefreshNextBtnVisible()
        {
            View.m_top.m_nextRoundBtn.visible = ptData.hasCurrentMainReward() && ptData.currentMainReward.receive;
        }
        //设置一键按钮
        private void RefreshOneKeyBtnVisible()
        {
            if (ptData.hasCurrentMainReward() && ptData.currentMainReward.receive)
                View.m_top.m_oneKeyBtn.visible = false;
            else
            {
                View.m_top.m_oneKeyBtn.visible = ptData.isShowOneKey;
            }
        }
        //设置大奖显示
        private void RefreshMainReward()
        {
            View.m_top.m_firstSelect.visible = !ptData.hasCurrentMainReward();
            if (!ptData.hasCurrentMainReward())
            {
                View.m_top.m_bigItem.Init(true);
            }
            else
            {
                View.m_top.m_bigItem.m_item.Init(ptData.currentMainReward.itemInfo.id, ptData.currentMainReward.itemInfo.num, false);
                View.m_top.m_bigItem.m_bgGroup.visible = false;
                View.m_top.m_bigItem.m_item.visible = true;
                View.m_top.m_bigItem.m_wenhao.visible = false;
                View.m_top.m_bigItem.m_jia.visible = false;
                View.m_top.m_bigItem.m_reciveGroup.visible = ptData.currentMainReward.receive;
                View.m_top.m_bigItem.m_recive.m_title.text = 1110018.GetItsLanaugeStr();
            }
        }
        //设置钻石消耗
        private void RefreshDiamondNum()
        {
            View.m_top.m_diamondNum.text = ClientTools.GetItemNumStr(BagService.Singleton.GetItemCount((int)CurrencyType.Diamond));
            View.m_top.m_remainNum.text = string.Format(4190023.GetItsLanaugeStr(), (ptData.maxDiamondNum - ptData.diamondNum).ToString());

            string useNum = BagService.Singleton.GetItemCount((int)CurrencyType.Diamond) >= ptData.currentPrice ? $"[color=#14e6ff]{ ptData.currentPrice}[/color]" : $"[color=#ff6262]{ ptData.currentPrice}[/color]";
            View.m_top.m_diamondUse.text = string.Format(4190024.GetItsLanaugeStr(), useNum);
        }
        //设置消耗道具
        private void RefreshUseItemNum()
        {
            View.m_top.m_itemNum.text = BagService.Singleton.GetItemCount(ptData.voucherId).ToString();
        }
        //设置轮次
        private void RefreshRoundText()
        {
            View.m_top.m_roundNum.text = string.Format(4190002.GetItsLanaugeStr(), ptData.currentRound, 4190002.GetItsGlobalInt());
        }
        //设置文本标题与背景版等不会变动的
        private void InitText()
        {
            View.m_preViewBtn.m_title.text = 4190008.GetItsLanaugeStr();
            View.m_top.m_nextRoundBtn.m_title.text = 4190022.GetItsLanaugeStr();
            View.m_top.m_oneKeyBtn.title = 4190032.GetItsLanaugeStr();
            View.m_giftBtn.m_title.text = 4190033.GetItsLanaugeStr();
            string baseBg = "lz_ui_hongDongPirateTreasur_topBg";
            if (!string.IsNullOrEmpty(ptData.baseplate) && ptData.baseplate != "null")
            {
                baseBg = ptData.baseplate;
            }
            UIGloader.SetUrl(View.m_top.m_itemIcon, ClientTools.GetItemIcon(ptData.voucherId));
        }
        //添加事件监听
        private void RegisterEvent()
        {
            //按钮事件
            View.m_preViewBtn.onClick.Set(OnPreViewClick);
            View.m_giftBtn.onClick.Add(() =>
            {
                TaskHelper.Singleton.OpenWindow((int)TaskJumpType.HongDongPirate, param: ptData.activity);
            });
            View.m_top.m_nextRoundBtn.onClick.Set(OnNextRoundClick);
            View.m_top.m_ruleBtn.onClick.Add(OnRuleClick);
            View.m_top.m_bigItem.onClick.Add(OnBigRewardClick);
            View.m_top.m_oneKeyBtn.onClick.Add(OnOnekeyClick);
            //列表事件
            View.m_listOne.Init(1, ptData);
            View.m_listTwo.Init(2, ptData);
            View.m_listThree.Init(3, ptData);
            View.m_listFour.Init(4, ptData);
            View.m_listOne.m_anim1.selectedIndex = 0;
            View.m_listTwo.m_anim1.selectedIndex = 1;
            View.m_listThree.m_anim1.selectedIndex = 0;
            View.m_listFour.m_anim1.selectedIndex = 1;
            //服务器消息监听事件
            //大奖消息返回，打开大奖展示界面
            AddListener(EventID.HongDongMainReward, () =>
            {
                this.OpenChild<PirateTreasureBigRewardWindow>(HongDongPirateTreasurePackage.packageId, UILayer.Popup, false, param);
            });
            //切换大奖关闭大奖显示界面刷新界面显示
            AddListener(EventID.HongDongMainSelect, () =>
            {
                RefreshPtData();
                RefreshMainReward();
                if (ActivityService.Singleton.Data.ptIsSelectMainReward)
                {
                    PlayAni();
                    ActivityService.Singleton.Data.ptIsSelectMainReward = false;
                }
                else
                    TipWindow.Singleton.ShowTip(4190027);
            });
            AddListener(EventID.HongDongPickOne, () =>
            {
                View.m_mask.visible = true;
                RefreshPtData();
                RefreshUseItemNum();
                RefreshDiamondNum();
                RefreshMainReward();
                RefreshListPTData();
                RefreshNextBtnVisible();
                RefreshOneKeyBtnVisible();
                if (ActivityService.Singleton.Data.GetPickOneId() != -1)
                {
                    switch (ActivityService.Singleton.Data.GetPickOneId())
                    {
                        case 0:
                            View.m_listOne.RefreshPickOne(View, modelEff);
                            break;
                        case 1:
                            View.m_listTwo.RefreshPickOne(View, modelEff);
                            break;
                        case 2:
                            View.m_listThree.RefreshPickOne(View, modelEff);
                            break;
                        case 3:
                            View.m_listFour.RefreshPickOne(View, modelEff);
                            break;
                        default:
                            break;
                    }
                }
                else
                    Debuger.Err("返回翻牌奖励为空");
            });
            AddListener(EventID.HongDongPickOneKey, () =>
            {
                if (View.m_mask.visible == true)
                    return;
                //WinMgr.Singleton.GetWindow<HongDongMainWindow>().View.m_pmask.visible = true;
                View.m_mask.visible = true;
                RefreshPtData();
                RefreshUseItemNum();
                RefreshDiamondNum();
                RefreshMainReward();
                RefreshListPTData();
                RefreshNextBtnVisible();
                RefreshOneKeyBtnVisible();
                targetList.Clear();
                if (ActivityService.Singleton.Data.curRewardGrids != null && ActivityService.Singleton.Data.curRewardGrids.Count > 0)
                {
                    targetList.AddRange(ActivityService.Singleton.Data.curRewardGrids);
                    curIndex = 0;
                }
                else
                    curIndex = -1;
                RefreshShowOneKey();
            });
            AddListener(EventID.HongDongPreviews, () =>
            {
                this.OpenChild<PriateTreasureShowRewardWindow>(HongDongPirateTreasurePackage.packageId, UILayer.Popup, false, param);
            });
            AddListener(EventID.HongDongNext, () =>
            {
                RefreshPtData();
                RefreshRoundText();
                RefrshList();
                RefreshMainReward();
                RefreshNextBtnVisible();
                RefreshOneKeyBtnVisible();
            });
            AddListener(EventID.HongDongUpdate, () =>
            {
                RefreshPtData();
            });
        }
        //刷新一键翻牌显示
        int curIndex = -1;
        List<PirateTreasureRewardGrid> targetList = new List<PirateTreasureRewardGrid>();
        private void RefreshShowOneKey()
        {
            if (curIndex < 0)
            {
                Debuger.Err("返回翻牌奖励为空");
            }
            else
            {
                if (ActivityService.Singleton.Data.GetPickOneId(targetList[curIndex].index) != -1)
                {
                    switch (ActivityService.Singleton.Data.GetPickOneId(targetList[curIndex].index))
                    {
                        case 0:
                            View.m_listOne.RefreshPickOneKey(View, modelEff, targetList[curIndex]);
                            break;
                        case 1:
                            View.m_listTwo.RefreshPickOneKey(View, modelEff, targetList[curIndex]);
                            break;
                        case 2:
                            View.m_listThree.RefreshPickOneKey(View, modelEff, targetList[curIndex]);
                            break;
                        case 3:
                            View.m_listFour.RefreshPickOneKey(View, modelEff, targetList[curIndex]);
                            break;
                        default:
                            break;
                    }
                    float delatTime = 1.5f / targetList.Count;
                    curIndex += 1;
                    if (curIndex < targetList.Count)
                    {
                        DelayCall(delatTime, RefreshShowOneKey);
                    }
                    else if (!PirateTreasureService.Singleton.endIsBig)
                    {
                        DelayCall(0.5f, () =>
                        {
                            curIndex = -1;
                            OpenChild<ItemGetWindow>(CommonPackage.packageId, UILayer.TopHUD, false, PirateTreasureService.Singleton.oneKeyParam);
                            View.m_mask.visible = false;
                            //WinMgr.Singleton.GetWindow<HongDongMainWindow>().View.m_pmask.visible = false;
                        });
                    }
                    else
                        curIndex = -1;
                }
                else
                    Debuger.Err("返回翻牌奖励为空");
            }
        }
        //按钮事件
        #region
        //预览按钮点击事件
        private void OnPreViewClick()
        {
            PirateTreasureService.Singleton.OnReqPreview(ptData.id);
        }
        //进入下一轮按钮点击
        private void OnNextRoundClick()
        {
            if (ptData.currentRound >= 4190002.GetItsGlobalInt())
            {
                TipWindow.Singleton.ShowTip(4190020);
                return;
            }
            if (ptData.hasCurrentMainReward() && !ptData.currentMainReward.receive)
            {
                View.m_top.m_nextRoundBtn.visible = false;
                Debuger.Err("进入下一轮的按钮在为抽中大奖的情况下显示出来了");
                return;
            }
            if (!ActivityService.Singleton.Data.GetIsAllRevice())
            {
                ConfirmWindow.Singleton.ShowTip(4190029.GetItsLanaugeStr(), () => { PirateTreasureService.Singleton.OnReqNextRound(ptData.id); });
            }
            else
                PirateTreasureService.Singleton.OnReqNextRound(ptData.id);
        }
        //钻石获取按钮点击
        private async void OnDiamondClickAsync()
        {
            if (FuncComp.Singleton.TipFuncNotOpen(ServerLink.FuncEnum.ChongZhi))
                return;
            if (await ChongZhiServise.Singleton.OnReqChongZhiInfo(ChongZhiType.ChongZhi))
            {
                MainCityStateHelper.Singleton.OpenChild<ChongZhiMainWindow>(ChongZhiPackage.packageId, UILayer.Popup, true);
                //if (!Tools.channelUnderMark((int)UnderMarkType.ChongZhiJump))
                //{
                //}
                //else
                //{
                //    MainCityStateHelper.Singleton.OpenChild<Logic.ChongZhiGuoShen.ChongZhiMainWindow1>(ChongZhiShenHe.ChongZhiShenHePackage.packageId, UILayer.Popup, true, null, refPkgs: new string[] { ChongZhiPackage.packageId });
                //}
                Close();
            }
        }
        //规则按钮
        private void OnRuleClick()
        {
            int ruleId;
            Int32.TryParse(ptData.rule, out ruleId);
            TwoParam<int, int> param = new TwoParam<int, int> { value1 = -1, value2 = ruleId };
            OpenChild<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
        }
        //点击大奖
        private void OnBigRewardClick()
        {
            if (!ptData.hasCurrentMainReward() || !ptData.currentMainReward.receive)
            {
                PirateTreasureService.Singleton.OnReqMainRewardData(ptData.id);
                if (!ptData.hasCurrentMainReward())
                {
                    View.m_listOne.InitBgList();
                    View.m_listTwo.InitBgList();
                    View.m_listThree.InitBgList();
                    View.m_listFour.InitBgList();
                }
            }
            else
                TipWindow.Singleton.ShowTip(4190021.GetItsLanaugeStr());
        }
        //一键翻牌
        private async void OnOnekeyClick()
        {
            if(!ptData.hasCurrentMainReward())
            {
                TipWindow.Singleton.ShowTip(4190006);
                return;
            }
            if (ptData.isShowOneKey)
            {
                if (BagService.Singleton.GetItemCount(ptData.voucherId) <= 0)
                {
                    TipWindow.Singleton.ShowTip(4190031);
                }
                else
                {
                    await PirateTreasureService.Singleton.OnReqOneKey(ptData.id);
                }
            }
            else
            {
                Debuger.Err("未开启一键");
            }
        }
        #endregion
        //设置倒计时
        private void InitTime()
        {
            timeer = ptData.overTime;
            if (timeer > 0)
            {
                long timerId = 0;
                var now = TimeUtils.currentDateTime();
                if (timeer > now.Ticks)
                {
                    View.m_top.m_timeText.text = ClientTools.GetHDTimeStr(new DateTime(timeer) - now);
                    timerId = AddTimer(1.0f, () =>
                    {
                        now = TimeUtils.currentDateTime();
                        if (timeer > now.Ticks)
                        {
                            var span = new DateTime(timeer) - now;
                            View.m_top.m_timeText.text = ClientTools.GetHDTimeStr(span);
                        }
                        else
                        {
                            if (timerId != 0)
                                StopTimerOrCoroutine(timerId);
                            View.m_top.m_timeText.text = 4120011.GetItsLanaugeStr();
                        }
                    });
                }
                else
                {
                    View.m_top.m_timeText.text = 4120011.GetItsLanaugeStr();
                }
            }
        }

        //播放洗牌动画
        private void PlayAni()
        {
            View.m_mask.visible = true;
            View.m_listOne.PlayStarAni(true);
            View.m_listTwo.PlayStarAni(true);
            View.m_listThree.PlayStarAni(true);
            View.m_listFour.PlayStarAni(true);
            DelayCall(1f, () =>
            {
                View.m_listOne.m_itemList.visible = false;
                View.m_listTwo.m_itemList.visible = false;
                View.m_listThree.m_itemList.visible = false;
                View.m_listFour.m_itemList.visible = false;
                View.m_listOne.m_itemListBg.visible = true;
                View.m_listTwo.m_itemListBg1.visible = true;
                View.m_listThree.m_itemListBg.visible = true;
                View.m_listFour.m_itemListBg1.visible = true;
                View.m_listOne.m_rightAni.Play(1, 0, () => { View.m_listOne.m_itemListBg.visible = false; View.m_listOne.m_itemList.visible = true; });
                View.m_listTwo.m_leftAni.Play(1, 0, () => { View.m_listTwo.m_itemListBg1.visible = false; View.m_listTwo.m_itemList.visible = true; });
                View.m_listThree.m_rightAni.Play(1, 0, () => { View.m_listThree.m_itemListBg.visible = false; View.m_listThree.m_itemList.visible = true; });
                View.m_listFour.m_leftAni.Play(1, 0, () => { View.m_listFour.m_itemListBg1.visible = false; View.m_mask.visible = false; View.m_listFour.m_itemList.visible = true; });
            });
        }

        //列表点击事件
        #region
        private void RefreshListPTData()
        {
            View.m_listOne.ptData = ptData;
            View.m_listTwo.ptData = ptData;
            View.m_listThree.ptData = ptData;
            View.m_listFour.ptData = ptData;
        }
        #endregion

        protected override void OnClose()
        {
            base.OnClose();
            if (modelEff != null)
            {
                GameObject.Destroy(modelEff);
            }
        }
    }
}
