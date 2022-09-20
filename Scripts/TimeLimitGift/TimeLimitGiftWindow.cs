using Base;
using Base.StartUp;
using Logic.ChongZhi;
using System.Threading.Tasks;
using Common;
using FairyGUI;
using HongDong;
using HongDongTag;
using Logic.ActivityHall.TaskRank;
using Logic.Common;
using Logic.Sigin.Com;
using Message.Activity;
using ServerLink;
using System;
using System.Collections.Generic;

namespace Logic.ActivityHall.TimeLimitGift
{
    public class TimeLimitGiftWindow : BindWindow<UI_TimeLimitGiftWindow>
    {

        TimeLimitGiftData mainInfo;
        List<TimeLimitGiftCell> cellList;
        int activityID;
        long tickID;

        protected override void OnClose()
        {
            base.OnClose();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            activityID = (int)param;
            AddListeners();
            InitData();
            InitBaseView();
            InitView();
            StartTick();
        }


        private void AddListeners()
        {
            AddListener(EventID.TimeLimitGiftUpdate, UpdateView);
        }

        private void UpdateView(Base.Event evt)
        {
            if ((int)evt.Data != activityID) return;
            InitData();
            InitView();
            StartTick();
        }

        private void InitBaseView()
        {
            View.m_Help.visible = int.TryParse(mainInfo.rule, out var ruleID);
            View.m_Help.onClick.Set(() =>
            {
                var param = new Base.TwoParam<int, int> { value1 = -1, value2 = ruleID };
                WinMgr.Singleton.Open<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
            });
            if (int.TryParse(mainInfo.title, out var title) && title != 0)
                UIGloader.SetUrl(View.m_titleLoader, "ui://HongDongTag/" + title.GetItsLanaugeStr());
            else
                UIGloader.SetUrl(View.m_titleLoader, "");
            //if (int.TryParse(mainInfo.title, out var title) && title != 0)
            //    View.m_Label_1.text = title.GetItsLanaugeStr();
            //else if (mainInfo.title != "0")
            //    View.m_Label_1.text = mainInfo.title;
            //else
            //    View.m_Label_1.text = "";
            if (int.TryParse(mainInfo.desc, out var desc) && desc != 0)
                View.m_Des.text = desc.GetItsLanaugeStr();
            else if (mainInfo.desc != "0")
                View.m_Des.text = mainInfo.desc;
            else
                View.m_Des.text = "";
            if (mainInfo.activity != 0)
            {
                var other = ActivityService.Singleton.Data.GetMainInfo(mainInfo.activity);
                if (other != null)
                {
                    if (other.type == (int)ActiveEnum.Rank)
                    {
                        var Rank = UIPackage.CreateObjectFromURL(UI_RankBtn.URL) as UI_RankBtn;
                        View.AddChild(Rank);
                        //Rank.m_title.text = 1010021.GetItsLanaugeStr();
                        Rank.xy = View.m_Rank.xy;
                        Rank.onClick.Set(async () =>
                        {
                            if (await TaskRankService.Singleton.OnReqRankInfo(mainInfo.activity))
                                WinMgr.Singleton.Open<TaskRankWindow>(HongDongTag.HongDongTagPackage.packageId, UILayer.Popup, false, mainInfo.activity);
                        });
                    }

                }
            }
        }

        private void InitView()
        {
            RenderList();
        }

        private void InitData()
        {
            mainInfo = ActivityService.Singleton.Data.GetMainInfo(activityID) as TimeLimitGiftData;

            if (cellList == null)
                cellList = new List<TimeLimitGiftCell>();
            cellList.Clear();

            cellList.AddRange(mainInfo.gifts);

        }

        private void RenderList()
        {
            View.m_BuyList.RemoveChildrenToPool();
            foreach (var info in cellList)
            {
                var item = View.m_BuyList.AddItemFromPool() as UI_CellItem;
                if (item.timerID != 0)
                    StopTimerOrCoroutine(item.timerID);

                item.m_Label.text = info.describe.GetItsLanaugeStr();
                item.m_reddot.visible = false;
                item.m_Go.visible = false;
                item.visible = true;
                item.m_AllServerLimit.visible = false;
                item.m_DeepCtl.SetSelectedIndex(3);

                item.m_GetBtn.visible = info.max > info.num && TimeUtils.currentTick > info.canBuyTime;
                item.m_GetBtn.grayed =
                    TimeUtils.currentTick >= mainInfo.overTime ||
                    TimeUtils.currentTick < info.canBuyTime ||
                    IsInColdingTime(info);

                item.m_GetBtn.m_title.text = ClientTools.GetMoneySymbol(info.cost);
                var itemID = info.id;
                if (item.grayed)
                    item.m_GetBtn.onClick.Set(() => { });
                else
                    item.m_GetBtn.onClick.Set(() => BuyGift(itemID, info.cost));

                item.m_AwardList.RemoveChildrenToPool();
                var awardList = info.rewards.SplitTo2IntArray(';', '+');
                foreach (var award in awardList)
                {
                    var icon = item.m_AwardList.AddItemFromPool() as UI_ItemIcon;
                    icon.Init(award[0], award[1]);
                }
                item.m_Condition.text = string.Format(4120106.GetItsLanaugeStr(), info.max - info.num);
                item.m_BiaoQian.visible = info.max <= info.num;
                //item.m_BiaoQian.m_title.text = 1090035.GetItsLanaugeStr();
                //item.m_Old.visible = info.orgPrice != 0 && (info.max > info.num);
                //var moneySybol = ConfigBean.GetBeanList<Data.Beans.t_paiBean>()[0].t_huobi_fuhao;
                //item.m_OldPrise.text = string.Format("{0}{1}{2}", 4070036.GetItsLanaugeStr(), moneySybol, info.orgPrice);
                //buy over refresh
                if (info.holdDays == 0 && info.loopDays == 0)
                {
                    var canBuyTime = info.canBuyTime;
                    if (TimeUtils.currentTick < canBuyTime)
                    {
                        item.m_Condition.text = string.Format(4120106.GetItsLanaugeStr(), 0);
                        item.m_BiaoQian.visible = true;
                        item.timerID = AddTimer(1f, () =>
                        {
                            var now = TimeUtils.currentTick;
                            if (now > mainInfo.overTime)
                            {
                                StopTimerOrCoroutine(item.timerID);
                                item.timerID = 0;
                                item.m_Timer.text = 4120011.GetItsLanaugeStr();
                            }
                            if (now > info.canBuyTime)
                            {
                                StopTimerOrCoroutine(item.timerID);
                                item.timerID = 0;
                                info.num = 0;
                                InitData();
                                RenderList();
                            }
                            else
                            {
                                item.m_Timer.text = 4120204.GetItsLanaugeStr() + ClientTools.GetHDTimeStr(new TimeSpan(canBuyTime - now));
                            }

                        });
                        var nowT = TimeUtils.currentTick;
                        if (nowT > mainInfo.overTime)
                            item.m_Timer.text = 4120011.GetItsLanaugeStr();
                        else
                        {
                            if (canBuyTime < nowT)
                            {
                                UpdateView(new Base.Event() { Data = activityID });
                                return;
                            }
                            item.m_Timer.text = 4120204.GetItsLanaugeStr() + ClientTools.GetHDTimeStr(new TimeSpan(canBuyTime - nowT));
                        }
                    }
                    else
                        item.m_Timer.text = "";
                }
                else
                {
                    var nowT = TimeUtils.currentDateTime();
                    var title = 4120203;
                    var endTimeT = new DateTime(info.canBuyTime).Date.AddDays(info.holdDays);
                    if (IsInColdingTime(info))
                    {
                        endTimeT = endTimeT.AddDays(info.loopDays);
                        title = 4120204;
                    }
                    item.m_Timer.text = title.GetItsLanaugeStr() + ClientTools.GetHDTimeStr(new TimeSpan(endTimeT.Ticks - nowT.Ticks));

                    //time refresh
                    item.timerID = AddTimer(1f, () =>
                    {
                        var now = TimeUtils.currentDateTime();
                        var endTime = new DateTime(info.canBuyTime).Date.AddDays(info.holdDays);
                        var titleNum = 4120203;
                        if (IsInColdingTime(info))
                        {
                            endTime = endTime.AddDays(info.loopDays);
                            titleNum = 4120204;
                        }
                        if (endTime.Ticks < now.Ticks)
                        {
                            UpdateView(new Base.Event() { Data = activityID });
                            return;
                        }
                        item.m_Timer.text = titleNum.GetItsLanaugeStr() + ClientTools.GetHDTimeStr(new TimeSpan(endTime.Ticks - now.Ticks));
                    });
                }
            }
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
                RenderList();
                View.m_Timer.text = 4120011.GetItsLanaugeStr();
            }
            else
            {
                View.m_Timer.text = ClientTools.GetHDTimeStr(new TimeSpan(mainInfo.overTime - now));
            }
        }

        private async void BuyGift(int itemID, int payID)
        {
            //可以接着用ChongzhiT_ThemeBuy

            //先判断第三方开关是否开放，如果开放，就只走规则二
            //再判断所在服务器是否在配置中，如果不在，走规则一
            //最后，规则三
            bool thirdOpen = false;
            string[] channels = 50000001.GetItsGlobalStr().Split(';');
            for (int j = 0; j < channels.Length; j++)
            {
                if (channels[j] == ChannelManager.Channel)
                {
                    thirdOpen = true;
                    break;
                }
            }

            bool choosePayment = false;
            string[] arrServer = 50000003.GetItsGlobalStr().Split(';');
            for (int j = 0; j < arrServer.Length; j++)
            {
                if (arrServer[j] == ChannelManager.Channel)
                {
                    choosePayment = true;
                    break;
                }
            }
            if (thirdOpen)
            {
                await TimeLimitGiftService.Singleton.OnReqBuyTimeLimitGift(activityID, itemID);
            }
            else if (!choosePayment)
            {
                await TimeLimitGiftService.Singleton.OnReqBuyTimeLimitGift(activityID, itemID);
            }
            else
            {
                //不光是充值界面用到了，其它的花钱的地方也用到了，走的协议是不一样的，不要简单的在这里就发送了
                //用回调
                FourParam<int, int, int, int> fourParam = new FourParam<int, int, int, int>();
                fourParam.value1 = payID;
                fourParam.value2 = (int)ChongZhiPayment.TimeLimitGift;
                fourParam.value3 = activityID;
                fourParam.value4 = itemID;
                WinMgr.Singleton.Open<ChongzhiT_ThemeBuy>(ChongZhiTC.ChongZhiTCPackage.packageId, UILayer.Popup, false, fourParam);
            }
        }

        private bool IsInColdingTime(TimeLimitGiftCell gift)
        {
            if (gift.loopDays == 0 && gift.holdDays == 0)
                return false;
            var now = TimeUtils.currentDateTime();
            var crossDay = (now.Date - new DateTime(gift.canBuyTime).Date).TotalDays % (gift.loopDays + gift.holdDays);
            return crossDay >= gift.holdDays;
        }
    }
}
