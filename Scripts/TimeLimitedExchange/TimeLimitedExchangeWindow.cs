using System;
using System.Collections.Generic;
using Base;
using FairyGUI;
using HongDongTag;
using Logic.Common;
using Message.Activity;

namespace Logic.ActivityHall.TimeLimitedExchange
{
    public class TimeLimitedExchangeWindow : BindWindow<UI_TimeLimitedExchangeWindow>
    {
        private int _activityID;
        private ExchangeItemData _activityData;
        private List<CustomExchangeItemRow> _rowDataArray;
        private TwoParam<List<ItemNeeded>, Action> _twoParam = new TwoParam<List<ItemNeeded>, Action>();

        //Debug 完手动切换为 false.
        public static bool debugMode = false;

        protected override void OnOpen()
        {
            base.OnOpen();
            _activityID = (int)param;

            _activityData = TimeLimitedExchangeService.Singleton.activityData;
            _rowDataArray = TimeLimitedExchangeService.Singleton.rowDataList;
            InitializeHeader();
            InitializeList();
            AddListener(EventID.TimeLimitedExchangeUpdate, RefreshWindow);
        }


        private void InitializeHeader()
        {
            View.m_header.m_title.text = 6530121.GetItsLanaugeStr();
            View.m_header.m_timePrompt.text = 6530122.GetItsLanaugeStr();
            View.m_header.m_timer.visible = false;
            if (_activityData.closeTime > 0)
            {
                View.m_header.m_timer.visible = true;
                var now = TimeUtils.currentDateTime();
                if (_activityData.closeTime > now.Ticks)
                {
                    View.m_header.m_timer.text = ClientTools.GetHDTimeStr(new DateTime(_activityData.closeTime) - now);
                    long timerId = 0;
                    timerId = AddTimer(1.0f, () =>
                    {
                        now = TimeUtils.currentDateTime();
                        var time = new DateTime(_activityData.closeTime);
                        if (_activityData.closeTime > now.Ticks)
                        {
                            var span = new DateTime(_activityData.closeTime) - now;
                            View.m_header.m_timer.text = ClientTools.GetHDTimeStr(span);
                        }
                        else
                        {
                            if (timerId != 0)
                                StopTimerOrCoroutine(timerId);
                            View.m_header.m_timer.text = 4120011.GetItsLanaugeStr();
                        }
                    });
                }
                else
                {
                    View.m_header.m_timer.text = 4120011.GetItsLanaugeStr();
                }
            }
        }

        private void InitializeList()
        {
            View.m_exchangeList.RemoveChildrenToPool();
            foreach (CustomExchangeItemRow rowData in _rowDataArray)
            {
                UI_RowOfExchangeList row = View.m_exchangeList.AddItemFromPool() as UI_RowOfExchangeList;

                //标题.
                row.m_title.text = 6530120.GetItsLanaugeStr();
                //次数.
                row.m_timesRecord.text = $"({rowData.consumedTimes}/{rowData.maxTimes})";
                //左边的 需要拥有的道具.
                InitializeItemsNeeded(row.m_itemsNeededList, rowData.itemNeededList);
                //右边的被兑换的道具.
                InitializeItemToGet(row, rowData.itemToGet);
                //兑换按钮.
                row.m_exchangeButton.enabled = rowData.HasAllEnoughQuantity && rowData.HasLeftTimes;
                row.m_exchangeButton.m_redDot.visible = rowData.HasAllEnoughQuantity && rowData.HasLeftTimes;
                row.m_exchangeButton.m_buttonName.text = 6530126.GetItsLanaugeStr();
                row.m_exchangeButton.onClick.Set((context =>
                {
                    _twoParam.value1 = rowData.itemNeededList;
                    _twoParam.value2 = () =>
                    {
                        TimeLimitedExchangeService.Singleton.OnReqTimeLimitedExchange(_activityID,
                            rowData.requestId,
                            CustomExchangeItemRow.GetAllEquipInstanceIds(rowData.itemNeededList),
                            CustomExchangeItemRow.GetAllCostPets(rowData.itemNeededList),
                            CustomExchangeItemRow.GetAllCostItems(rowData.itemNeededList));
                    };
                    WinMgr.Singleton.Open<ConfirmWindow>(HongDongTagPackage.packageId, UILayer.Popup, false,
                        _twoParam);
                }));
            }
        }

        private void InitializeItemsNeeded(GList itemsNeededList, List<ItemNeeded> itemNeededList)
        {
            itemsNeededList.RemoveChildrenToPool();

            foreach (ItemNeeded itemNeeded in itemNeededList)
            {
                UI_ItemWithPlusIcon uiItemWithPlusIcon = itemsNeededList.AddItemFromPool() as UI_ItemWithPlusIcon;

                switch (itemNeeded.itemType)
                {
                    case ItemTypeEnum.Item:
                        uiItemWithPlusIcon.m_item.Init((itemNeeded.itemId), itemNeeded.itemCount);
                        uiItemWithPlusIcon.grayed = !itemNeeded.hasEnoughQuantity;
                        break;

                    case ItemTypeEnum.Pet:
                        if (itemNeeded.hasEnoughQuantity)
                        {
                            uiItemWithPlusIcon.m_item.InitPet(itemNeeded.petInfo);
                        }
                        else
                        {
                            uiItemWithPlusIcon.m_item.Init(itemNeeded.itemId, 1);
                            uiItemWithPlusIcon.grayed = true;
                        }

                        break;

                    case ItemTypeEnum.Equipment:
                        uiItemWithPlusIcon.m_item.Init(itemNeeded.itemId, itemNeeded.itemCount);
                        uiItemWithPlusIcon.grayed = !itemNeeded.hasEnoughQuantity;
                        break;
                }
            }

            //隐藏最后一个 plusIcon.
            UI_ItemWithPlusIcon lastOne =
                itemsNeededList.GetChildAt(itemNeededList.Count - 1) as UI_ItemWithPlusIcon;
            lastOne.m_plusIcon.visible = false;
        }

        private void InitializeItemToGet(UI_RowOfExchangeList row, ItemToGet itemToGet)
        {
            row.m_itemToGet.Init(itemToGet.itemId, itemToGet.itemCount);
        }

        private void RefreshWindow()
        {
            _activityData = TimeLimitedExchangeService.Singleton.activityData;
            _rowDataArray = TimeLimitedExchangeService.Singleton.rowDataList;
            InitializeHeader();
            InitializeList();
        }
    }
}