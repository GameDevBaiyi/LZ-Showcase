using Base;
using Common;
using FairyGUI;
using HongDong;
using HongDongTag;
using Logic.ActivityHall.TaskActivity;
using Logic.Common;
using Logic.Sigin.Com;
using Message.Activity;
using ServerLink;
using System;

namespace Logic.Sigin
{
    class RechargeCountActivityWindow : BindWindow<UI_RechargeCountActivityWindow>
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        RechargeNumData ptData;
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
            if (ptData.type != (int)ActiveEnum.RechargeMoney)
            {
                LOGGER.Error($"id 与类型对应不上，当前id:{activiId},当前类型：{ptData.type}");
                return;
            }
            InitWindow();
            var num = ptData.diamondNum * 10;
            //if(SDKMgr.SDKName == SDKMgr.SDKNAME_GoSuSdk || SDKMgr.SDKName == SDKMgr.SDKNAME_GoSuIOSSdk)
            //{
            //    num = ptData.diamondNum * 10;
            //}
            //else
            //{
            //    num = Tools.GetVersionType() == 0 ? ptData.num : ptData.diamondNum * 10;
            //}
            //num = Tools.GetVersionType() == 0 ? ptData.num : ptData.diamondNum * 10;
            View.m_MoneyList.itemRenderer = (int index, FairyGUI.GObject go) =>
            {
                UI_DeepItem item = go as UI_DeepItem;
                if (ptData != null && ptData.tasks.Count > 0)
                {
                    item.Init(0, ptData.tasks[index], ptData.id, num, over, () =>
                      {
                          if (WinMgr.Singleton.HasOpen<HongDongMainWindow>())
                              WinMgr.Singleton.Close<HongDongMainWindow>();
                      });
                }
            };
            View.m_MoneyList.SetVirtual();
            AddListener(EventID.HongDongRechargeNumRefresh, UpdateData);
            UpdateRender();
        }
        private void RefreshPtData()
        {
            ptData = ActivityService.Singleton.Data.GetMainInfo((int)param) as RechargeNumData;
        }
        private void UpdateData(Event evt)
        {
            RefreshPtData();
            UpdateRender();
        }
        private void UpdateRender()
        {
            ptData.tasks.Sort(SortNumist);
            View.m_MoneyList.numItems = ptData.tasks.Count;
        }
        private int SortNumist(RechargeNumCell A, RechargeNumCell B)
        {
            int aNum = 1;
            int bNum = 1;
            if (A.got)
            {
                aNum = 2;
            }
            else if (ptData.num / 10 >= A.need)
            {
                aNum = 0;
            }
            if (B.got)
            {
                bNum = 2;
            }
            else if (ptData.num / 10 >= B.need)
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
            int title;
            //Int32.TryParse(ptData.title, out title);
            //View.m_Label_1.text = title.GetItsLanaugeStr();
            if (Int32.TryParse(ptData.title, out title) && title != 0)
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
            //View.m_Des.text = Convert.ToInt32(ptData.desc).GetItsLanaugeStr();
            View.m_Time.visible = false;
            if (ptData.closeTime > 0)
            {
                View.m_Time.visible = true;
                long timerId = 0;
                var now = TimeUtils.currentDateTime();
                if (ptData.closeTime > now.Ticks)
                {
                    View.m_Timer.text = ClientTools.GetHDTimeStr(new DateTime(ptData.closeTime) - now);
                    timerId = AddTimer(1.0f, () =>
                    {
                        now = TimeUtils.currentDateTime();
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
                        //Rank.m_title.text = 1010021.GetItsLanaugeStr();
                        Rank.xy = View.m_RankPos.xy;
                        Rank.onClick.Set(async () =>
                        {
                            //yield_place_holder
                            if (await TaskActivitySevice.Singleton.OnAwardRankInfo(ptData.activity))
                                OpenChild<ActivityRankRewardWindow>(HongDongTag.HongDongTagPackage.packageId, UILayer.Popup, false, ptData.activity);
                            //WinMgr.Singleton.Open<ActivityRankRewardWindow>(HongDongTag.HongDongTagPackage.packageId, UILayer.Popup, false, ptData.activity);
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
