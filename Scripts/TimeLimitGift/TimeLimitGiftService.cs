using Base;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Threading.Tasks;

namespace Logic.ActivityHall.TimeLimitGift
{
    public class TimeLimitGiftService : SingletonService<TimeLimitGiftService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResBuyTimeLimitGift.MsgId, OnResBuyTimeLimitGift);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResBuyTimeLimitGift.MsgId, OnResBuyTimeLimitGift);
        }

        #region req
        public Task<bool> OnReqBuyTimeLimitGift(int activityID, int itemID)
        {
            var req = GetEmptyMsg<ReqBuyTimeLimitGift>();
            req.id = activityID;
            req.itemID = itemID;
            return SendMsg(ref req);
        }
        #endregion

        #region res
        private void OnResBuyTimeLimitGift(Base.Event evt)
        {
            var res = GetCurMsg<ResBuyTimeLimitGift>(evt.EventId);
            var data = ActivityService.Singleton.Data.GetMainInfo(res.id) as TimeLimitGiftData;
            for (int i = 0; i < data.gifts.Count; i++)
                if (data.gifts[i].id == res.item.id)
                {
                    data.gifts[i] = res.item;
                    break;
                }
            GED.ED.dispatchEvent(EventID.TimeLimitGiftUpdate, res.id);
        }
        #endregion
    }
}
