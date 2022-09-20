using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Event = Base.Event;

namespace Logic.ActivityHall.ExchangeItem
{
    public class ExchangeItemService : SingletonService<ExchangeItemService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResOldExchangeItem.MsgId, OnResExchangeItem);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResOldExchangeItem.MsgId, OnResExchangeItem);
        }

        #region req

        public Task<bool> OnReqExchangeItem(int activityID, int itemID, int num)
        {
            var req = GetEmptyMsg<ReqOldExchangeItem>();
            req.id = activityID;
            req.itemID = itemID;
            req.num = num;
            return SendMsg(ref req);
        }

        #endregion

        #region res

        private void OnResExchangeItem(Base.Event evt)
        {
            var res = GetCurMsg<ResOldExchangeItem>(evt.EventId);
            var data = ActivityService.Singleton.Data.GetMainInfo(res.id) as ExchangeItemData;
            for (int i = 0; i < data.items.Count; i++)
                if (data.items[i].id == res.item.id)
                {
                    data.items[i] = res.item;
                    break;
                }

            GED.ED.dispatchEvent(EventID.ExchangeItemUpdate, res.id);
        }

        #endregion
    }
}