using Base;
using Logic.Service;
using Base.StartUp;
using Logic.ChongZhi;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Threading.Tasks;

namespace Logic.ActivityHall.EventTimeLimitGift
{
    public class EventTimeLimitGiftService : SingletonService<EventTimeLimitGiftService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResEventTimeLimitGift.MsgId, OnResEventTimeLimitGift);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResEventTimeLimitGift.MsgId, OnResEventTimeLimitGift);
        }

        #region req
        public Task<bool> OnReqEventTimeLimitGift(int activityID, int itemID, bool choosePayment = false)
        {
            var req = GetEmptyMsg<ReqEventTimeLimitGift>();
            req.id = activityID;
            req.itemID = itemID;
            req.thirdData = ChongZhiServise.Singleton.GetDaSaFaEventData(choosePayment);
            return SendMsg(ref req);
        }
        #endregion

        #region res
        public void OnResEventTimeLimitGift(Base.Event evt)
        {
            var res = GetCurMsg<ResEventTimeLimitGift>(evt.EventId);
            var data = ActivityService.Singleton.Data.GetMainInfo(res.id) as EventTimeLimitGiftData;
            for (int i = 0; i < data.items.Count; i++)
                if (data.items[i].id == res.item.id)
                {
                    data.items[i] = res.item;
                    break;
                }
            GED.ED.dispatchEvent(EventID.EventTimeLimitGiftUpdate, res.id);
        }



        #endregion
    }
}
