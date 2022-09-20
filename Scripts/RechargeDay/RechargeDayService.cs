using Base;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.ActivityHall.RechargeDay
{
    public class RechargeDayService : SingletonService<RechargeDayService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResRechargeDay.MsgId, OnResRechargeDay);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResRechargeDay.MsgId, OnResRechargeDay);
        }

        private void OnResRechargeDay(Event evt)
        {
            var msg = GetCurMsg<ResRechargeDay>(evt.EventId);
            if (msg == null)
                return;

            var info = ActivityService.Singleton.Data.GetMainInfo(msg.id);
            var infoTask = info as RechargeDayData;
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
            GED.ED.dispatchEvent(EventID.HongDongRechargeDayRefresh);
        }

        public void OnReqRechargeDay(int actId,int itemId)
        {
            var req = GetEmptyMsg<ReqRechargeDay>();
            req.id = actId;
            req.itemID = itemId;
            SendMsg(ref req);
        }
    }
}
