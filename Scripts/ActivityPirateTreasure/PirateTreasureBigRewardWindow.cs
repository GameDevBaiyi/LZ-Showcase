using FairyGUI;
using Logic.Sigin.Com;
using HongDongPirateTreasure;
using Message.Activity;
using System;
using System.Collections.Generic;
using Logic.Common;
using Logic.ActivityPirateTreasure;

namespace Logic.Sigin
{
    class PirateTreasureBigRewardWindow : BindWindow<UI_PirateTreasureBigRewardWindow>
    {
        PirateTreasureData ptData;
        Dictionary<int, List<PirateTreasureMainReward>> mainList;
        PirateTreasureMainReward curReward;
        UI_BigRewardItem curItem;
        protected override void OnOpen()
        {
            base.OnOpen();
            ptData = ActivityService.Singleton.Data.GetMainInfo((int)param) as PirateTreasureData;
            mainList = ActivityService.Singleton.Data.GetPTMainRewardList();
            if (ptData == null || mainList == null || mainList.Count == 0)
            {
                Close();
            }

            curReward = ptData.hasCurrentMainReward() ?  ptData.currentMainReward : null;
            View.m_closeBtn.onClick.Add(Close);
            View.m_cancelBtn.onClick.Add(Close);
            View.m_confirmBtn.onClick.Add(() =>
            {
                if (curReward == null || (ptData.hasCurrentMainReward() && curReward.rewardId == ptData.currentMainReward.rewardId))
                {
                    Close();
                }
                else
                {
                    PirateTreasureService.Singleton.OnReqChangeMainReward(curReward.rewardId, ptData.id);
                    Close();
                }
            });
            //AddListener(EventID.HongDongMainSelect, () => { Close(); });

            View.m_confirmBtn.m_title.text = 4190019.GetItsLanaugeStr();
            View.m_cancelBtn.m_title.text = 1020016.GetItsLanaugeStr();

            View.m_list.SetVirtual();
            View.m_list.itemRenderer = ListRender;
            View.m_list.numItems = mainList.Count;
            if (ptData.currentRound > mainList.Count)
            {
                View.m_list.ScrollToView(mainList.Count - 1);
            }
            else
                View.m_list.ScrollToView(ptData.currentRound - 1);
        }
        private string GetRoundText(int index)
        {
            string txt = "";
            List<int> numList = new List<int>();
            if (index > 10)
            {
                numList.Add(index / 10);
                numList.Add(index % 10);
            }
            else
                numList.Add(index);
            for (int i = 0; i < numList.Count; i++)
            {
                switch (index)
                {
                    case 1:
                        txt += 1000001.GetItsLanaugeStr();
                        break;
                    case 2:
                        txt += 1000002.GetItsLanaugeStr();
                        break;
                    case 3:
                        txt += 1000003.GetItsLanaugeStr();
                        break;
                    case 4:
                        txt += 1000004.GetItsLanaugeStr();
                        break;
                    case 5:
                        txt += 1000005.GetItsLanaugeStr();
                        break;
                    case 6:
                        txt += 1000006.GetItsLanaugeStr();
                        break;
                    case 7:
                        txt += 1000007.GetItsLanaugeStr();
                        break;
                    case 8:
                        txt += 1000008.GetItsLanaugeStr();
                        break;
                    case 9:
                        txt += 1000009.GetItsLanaugeStr();
                        break;
                    case 0:
                    case 10:
                        txt += 1000010.GetItsLanaugeStr();
                        break;
                    default:
                        break;
                }
            }
            return txt;
        }
        private void ListRender(int index,GObject obj)
        {
            UI_BigRewardListItem item = obj as UI_BigRewardListItem;
            if (item == null)
                return;
            item.m_roundText.text = string.Format(4190007.GetItsLanaugeStr(), GetRoundText(index + 1));
            item.m_itemList.RemoveChildren(0, -1, true);
            for (int i = 0; i < mainList[index+1].Count; i++)
            {
                UI_BigRewardItem selectItem = WinMgr.CreateInstance<UI_BigRewardItem>(HongDongPirateTreasurePackage.packageId);
                selectItem.Refresh(mainList[index + 1][i], ptData.currentRound);
                selectItem.m_select.m_slectCtr.selectedIndex = 0;
                if (curReward != null && mainList[index + 1][i].rewardId == curReward.rewardId)
                {
                    selectItem.m_select.m_slectCtr.selectedIndex = 1;
                    curItem = selectItem;
                }
                if (!mainList[index + 1][i].receive && mainList[index + 1][i].openRound <= ptData.currentRound)
                {
                    selectItem.m_select.onClick.Add(() =>
                    {
                        curReward = selectItem.mainData;
                        if (curItem != null)
                        {
                            curItem.m_select.m_slectCtr.selectedIndex = 0;
                        }
                        selectItem.m_select.m_slectCtr.selectedIndex = 1;
                        curItem = selectItem;
                    });
                }
                else if (mainList[index + 1][i].openRound > ptData.currentRound)
                {
                    selectItem.m_select.onClick.Add(() =>
                    {
                        TipWindow.Singleton.ShowTip(4190030.GetItsLanaugeStr());
                    });
                }
                item.m_itemList.AddChild(selectItem);
            }
            item.m_itemList.ResizeToFit();
        }
    }
}
