using FairyGUI;
using Logic.Sigin.Com;
using HongDongPirateTreasure;
using Message.Activity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Sigin
{
    class PriateTreasureShowRewardWindow : BindWindow<UI_PriateTreasureShowRewardWindow>
    {
        List<PirateTreasurePreview> previews = new List<PirateTreasurePreview>();
        protected override void OnOpen()
        {
            base.OnOpen();
            View.m_closeBtn.onClick.Add(Close);
            previews = ActivityService.Singleton.Data.GetPreviewList((int)param);
            View.m_itemList.SetVirtual();
            View.m_itemList.itemRenderer = ListRender;
            View.m_itemList.numItems = previews.Count;
        }
        private void ListRender(int index,GObject obj)
        {
            var item = obj as UI_ShowRewardItem;
            if (item == null)
                return;
            item.m_item.Init(previews[index].itemInfo.id, previews[index].itemInfo.num);
            item.m_item.m_roundEft.visible = false;
            string remain = previews[index].remain > 0 ? $"[color=#0a9200]{previews[index].remain}[/color]": $"[color=#cd2e14]{previews[index].remain}[/color]";
            item.m_numText.text = string.Format(4190002.GetItsLanaugeStr(), remain, previews[index].max);
            item.m_recived.visible = previews[index].remain == 0;
        }
    }
}
