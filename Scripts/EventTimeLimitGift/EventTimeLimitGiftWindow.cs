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
using Base.StartUp;
using Logic.ChongZhi;
using System.Threading.Tasks;
using Base;
using Base.Resource;
//using UI_RankBtn = HongDong.UI_RankBtn;

namespace Logic.ActivityHall.EventTimeLimitGift
{
    public class EventTimeLimitGiftWindow : BindWindow<UI_EventTimeLimitGiftWindow>
    {
        EventTimeLimitGiftData mainInfo;
        List<EventTimeLimitGiftCell> cellList;
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
            AddListener(EventID.EventTimeLimitGiftUpdate, UpdateView);
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
            View.m_Help.visible = int.TryParse(mainInfo.rule, out var ruleID) && ruleID != 0;
            View.m_Help.onClick.Set(() =>
            {
                var param = new Base.TwoParam<int, int> { value1 = -1, value2 = ruleID };
                WinMgr.Singleton.Open<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
            });
            //if (int.TryParse(mainInfo.title, out var title) && title != 0)
            //    View.m_Title.text = title.GetItsLanaugeStr();
            //else if (mainInfo.title != "0")
            //    View.m_Title.text = mainInfo.title;
            //else
            //    View.m_Title.text = "";
            if (int.TryParse(mainInfo.title, out var title) && title != 0)
                UIGloader.SetUrl(View.m_titleLoader, "ui://HongDongTag/" + title.GetItsLanaugeStr());
            else
                UIGloader.SetUrl(View.m_titleLoader, "");

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
            mainInfo = ActivityService.Singleton.Data.GetMainInfo(activityID) as EventTimeLimitGiftData;

            if (cellList == null)
                cellList = new List<EventTimeLimitGiftCell>();
            cellList.Clear();

            cellList.AddRange(mainInfo.items);

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
                item.m_DeepCtl.SetSelectedIndex(2);

                item.m_GetBtn.visible = info.maxBuyNum > info.boughtNum;
                item.m_GetBtn.grayed = TimeUtils.currentTick >= mainInfo.overTime;
                item.m_GetBtn.m_title.text = ClientTools.GetMoneySymbol(info.price);
                var itemID = info.id;
                if (item.grayed)
                    item.m_GetBtn.onClick.Set(() => { });
                else
                    item.m_GetBtn.onClick.Set(() => BuyGift(itemID, info.price));

                item.m_AwardList.RemoveChildrenToPool();
                var awardList = info.reward.SplitTo2IntArray(';', '+');
                foreach (var award in awardList)
                {
                    var icon = item.m_AwardList.AddItemFromPool() as UI_ItemIcon;
                    icon.Init(award[0], award[1]);
                }
                item.m_Condition.text = string.Format(4120106.GetItsLanaugeStr(), info.maxBuyNum - info.boughtNum);
                item.m_BiaoQian.visible = info.maxBuyNum <= info.boughtNum;
               // item.m_BiaoQian.m_title.text = 1090035.GetItsLanaugeStr();
                item.m_Timer.text = "";
                item.m_Old.visible = info.orgPrice != 0 && (info.maxBuyNum > info.boughtNum) && Tools.GetVersionType() != 1 && PathUtil.resLanguage != "vietnamese";
                var moneySybol = ConfigBean.GetBeanList<Data.Beans.t_paiBean>()[0].t_huobi_fuhao;
                item.m_OldPrise.text = string.Format("{0}{1}{2}", 4070036.GetItsLanaugeStr(), moneySybol, info.orgPrice);

                var canBuyTime = info.nextCanBuyTime;
                if (TimeUtils.currentTick < canBuyTime)
                {
                    item.timerID = AddTimer(1f, () =>
                    {
                        var now = TimeUtils.currentTick;
                        if (now > info.nextCanBuyTime)
                        {
                            StopTimerOrCoroutine(item.timerID);
                            item.timerID = 0;
                            info.boughtNum = 0;
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
                        item.m_Timer.text = 4120204.GetItsLanaugeStr() + ClientTools.GetHDTimeStr(new TimeSpan(canBuyTime - nowT));
                }
                else
                    item.m_Timer.text = "";
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
            //付费表ID没有传，又要用新的ChongzhiT

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
                await EventTimeLimitGiftService.Singleton.OnReqEventTimeLimitGift(activityID, itemID);
            }
            else if (!choosePayment)
            {
                await EventTimeLimitGiftService.Singleton.OnReqEventTimeLimitGift(activityID, itemID);
            }
            else
            {
                //不光是充值界面用到了，其它的花钱的地方也用到了，走的协议是不一样的，不要简单的在这里就发送了
                //用回调
                FourParam<int, int, int, int> fourParam = new FourParam<int, int, int, int>();
                fourParam.value1 = payID;
                fourParam.value2 = (int)ChongZhiPayment.EventTimeLimitGift;
                fourParam.value3 = activityID;
                fourParam.value4 = itemID;
                WinMgr.Singleton.Open<ChongzhiT_ThemeBuy>(ChongZhiTC.ChongZhiTCPackage.packageId, UILayer.Popup, false, fourParam);
            }
        }
    }
}
