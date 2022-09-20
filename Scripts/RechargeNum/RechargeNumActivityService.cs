using Base;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logic.ActivityHall.RechargeNum
{
    public class RechargeNumService : SingletonService<RechargeNumService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResRechargeNum.MsgId, OnResRechargeNum);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResRechargeNum.MsgId, OnResRechargeNum);
        }

        private void OnResRechargeNum(Event evt)
        {
            var msg = GetCurMsg<ResRechargeNum>(evt.EventId);
            if (msg == null)
                return;

            var info = ActivityService.Singleton.Data.GetMainInfo(msg.id);
            var infoTask = info as RechargeNumData;
            if (info == null || infoTask == null)
            {
                return;
            }

            for (int j = 0; j < infoTask.tasks.Count; j++)
            {
                if (infoTask.tasks[j].id == msg.item.id)
                {
                    infoTask.tasks[j] = msg.item;
                    break;
                }
            }
            ActivityService.Singleton.Data.SetMainInfo(infoTask);
            ActivityService.Singleton.Data.SetActivityRed();
            GED.ED.dispatchEvent(EventID.HongDongRechargeNumRefresh);
        }

        public void OnReqRechargeNum(int actId, int itemId)
        {
            var req = GetEmptyMsg<ReqRechargeNum>();
            req.id = actId;
            req.itemID = itemId;
            SendMsg(ref req);
        }
    }
}
