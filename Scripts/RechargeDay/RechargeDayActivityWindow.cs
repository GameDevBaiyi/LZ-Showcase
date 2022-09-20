using Base;
using Common;
using FairyGUI;
using Logic.Common;
using ServerLink;
using SignIn;
using System;
using System.Collections.Generic;
using HongDong;
// using UI_RankBtn = HongDong.UI_RankBtn;
using Logic.ActivityHall.TaskActivity;
using Message.Activity;
using Logic.Sigin.Com;

namespace Logic.Sigin
{
    class RechargeDayActivityWindow : BindWindow<HongDongTag.UI_RechargeDayActivityWindow>
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        RechargeDayData ptData;

        bool over = false;
        protected override void OnOpen()
        {
            base.OnOpen();
            int activiId = (int)param;
            RefreshPtData();
            if (ptData == null)
            {
                LOGGER.Error($"没有找到此活动");
                return;
            }

            if (ptData.type != (int)ActiveEnum.RechargeDay)
            {
                LOGGER.Error($"id 与类型对应不上，当前id:{activiId},当前类型：{ptData.type}");
                return;
            }
            InitWindow();

            View.m_DayList.itemRenderer = (int index, FairyGUI.GObject go) =>
            {
                UI_DeepItem item = go as UI_DeepItem;
                if (ptData != null && ptData.tasks.Count > 0)
                {
                    item.Init(0, ptData.tasks[index], ptData.id, ptData.day, over, () =>
                    {
                        if (WinMgr.Singleton.HasOpen<HongDongMainWindow>())
                            WinMgr.Singleton.Close<HongDongMainWindow>();
                    });
                }
            };
            View.m_DayList.SetVirtual();
            UpdateRender();
            AddListener(EventID.HongDongRechargeDayRefresh, UpdateData);           
        }
        private void RefreshPtData()
        {
            ptData = ActivityService.Singleton.Data.GetMainInfo((int)param) as RechargeDayData;
        }
        private void UpdateData(Event evt)
        {
            RefreshPtData();
            UpdateRender();
        }
        private void UpdateRender()
        {
            ptData.tasks.Sort(SortDayList);
            View.m_DayList.numItems = ptData.tasks.Count;
        }
        private int SortDayList(RechargeDayCell A, RechargeDayCell B)
        {
            int aNum = 1;
            int bNum = 1;
            if (A.got)
            {
                aNum = 2;
            }
            else if (ptData.day >= A.need)
            {
                aNum = 0;
            }
            if (B.got)
            {
                bNum = 2;
            }
            else if(ptData.day >= B.need)
            {
                bNum = 0;
            }
            if (aNum == bNum)
            {
                return A.id.CompareTo(B.id);
            }
            else
                return aNum.CompareTo(bNum);
        }

        private void InitWindow()
        {
            over = false;
            //int title;
            //Int32.TryParse(ptData.title, out title);
            //View.m_Title.text = title.GetItsLanaugeStr();
            if (int.TryParse(ptData.title, out var title) && title != 0)
                UIGloader.SetUrl(View.m_titleLoader, "ui://HongDongTag/" + title.GetItsLanaugeStr());
            else
                UIGloader.SetUrl(View.m_titleLoader, "");
            int descId;
            if (Int32.TryParse(ptData.desc, out descId) && descId != 0)
            {
                View.m_Des.text = descId.GetItsLanaugeStr();
            }
            else
                View.m_Des.text = ptData.desc;

            View.m_Time.visible = false;
            if (ptData.closeTime > 0)
            {
                View.m_Time.visible = true;
                var now = TimeUtils.currentDateTime();
                if (ptData.closeTime > now.Ticks)
                {
                    View.m_Timer.text = ClientTools.GetHDTimeStr(new DateTime(ptData.closeTime) - now);
                    long timerId = 0;
                    timerId = AddTimer(1.0f, () =>
                    {
                        now = TimeUtils.currentDateTime();
                        var time = new DateTime(ptData.closeTime);
                        if (ptData.closeTime > now.Ticks)
                        {
                            var span = new DateTime(ptData.closeTime) - now;
                            View.m_Timer.text = ClientTools.GetHDTimeStr(span);
                        }
                        else
                        {
                            if (timerId != 0)
                                StopTimerOrCoroutine(timerId);
                            View.m_Timer.text = 4120011.GetItsLanaugeStr();
                            over = true;
                        }
                    });
                }
                else
                {
                    View.m_Timer.text = 4120011.GetItsLanaugeStr();
                    over = true;
                }
            }
            if (ptData.activity != 0)
            {
                var other = ActivityService.Singleton.Data.GetMainInfo(ptData.activity);
                if (other != null)
                {
                    if (other.type == (int)ActiveEnum.Rank)
                    {
                        var Rank = UIPackage.CreateObjectFromURL(UI_RankBtn.URL) as UI_RankBtn;
                        View.AddChild(Rank);
                        Rank.text = 1010021.GetItsLanaugeStr();
                        Rank.xy = View.m_rankPos.xy;
                        Rank.onClick.Set( async() =>
                        {
                            //yield_place_holder
                            if (await TaskActivitySevice.Singleton.OnAwardRankInfo(ptData.activity))
                                //WinMgr.Singleton.Open<ActivityRankRewardWindow>(HongDongTag.HongDongTagPackage.packageId, UILayer.Popup, false, ptData.activity);                           
                                OpenChild<ActivityRankRewardWindow>(HongDongTag.HongDongTagPackage.packageId, UILayer.Popup, false, ptData.activity);
                        });
                    }                   
                }
            }
            int ruleId;
            Int32.TryParse(ptData.rule, out ruleId);
            View.m_Help.visible = ruleId != 0;
            View.m_Help.onClick.Add(() =>
            {
                TwoParam<int, int> param = new TwoParam<int, int> { value1 = -1, value2 = ruleId };
                WinMgr.Singleton.Open<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
            });
        }
    }
}
