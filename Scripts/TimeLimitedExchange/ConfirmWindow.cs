using System;
using System.Collections.Generic;
using Base;
using Common;
using Message.Bag;
using UI_ConfirmWindow = HongDongTag.UI_ConfirmWindow;

namespace Logic.ActivityHall.TimeLimitedExchange
{
    public class ConfirmWindow : BindWindow<UI_ConfirmWindow>
    {
        private TwoParam<List<ItemNeeded>, Action> _twoParam =
            new TwoParam<List<ItemNeeded>, Action>();
        private List<ItemNeeded> itemNeededList;

        protected override void OnOpen()
        {
            base.OnOpen();
            _twoParam = (TwoParam<List<ItemNeeded>, Action>)param;
            itemNeededList = _twoParam.value1;
            //标题.
            View.m_title.text = 6530124.GetItsLanaugeStr();
            //提示语.
            View.m_promptWords.text = 6530125.GetItsLanaugeStr();
            //取消按钮.
            View.m_cancelButton.m_buttonName.text = 6530127.GetItsLanaugeStr();
            View.m_cancelButton.onClick.Set(Close);
            //右上角关闭按钮.
            View.m_closeButton.onClick.Set(Close);
            //物品列表.
            InitializeCostItemList();
            //确认按钮.
            View.m_confirmButotn.m_buttonName.text = 6530126.GetItsLanaugeStr();
            View.m_confirmButotn.onClick.Set((() =>
            {
                if (_twoParam.value2 != null)
                {
                    _twoParam.value2.Invoke();
                }

                Close();
            }));
        }

        private void InitializeCostItemList()
        {
            View.m_itemList.RemoveChildrenToPool();

            foreach (ItemNeeded itemNeeded in itemNeededList)
            {
                UI_ItemIcon itemIcon = View.m_itemList.AddItemFromPool() as UI_ItemIcon;

                switch (itemNeeded.itemType)
                {
                    case ItemTypeEnum.Item:
                        itemIcon.Init((itemNeeded.itemId), itemNeeded.itemCount);
                        break;
                    case ItemTypeEnum.Pet:
                        itemIcon.InitPet(itemNeeded.petInfo);
                        break;
                    case ItemTypeEnum.Equipment:
                        itemIcon.Init(itemNeeded.itemId, itemNeeded.itemCount);
                        break;
                }
            }
        }
    }
}