using Base;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logic.ActivityHall.TaskActivity
{
    public class TaskActivitySevice : SingletonService<TaskActivitySevice>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResTaskActivityReward.MsgId, OnResTaskActivityReward);
            comp.AddListener(ResTaskActivityUpdate.MsgId, OnResTaskActivityUpdate);
            comp.AddListener(ResActivityRankInfo.MsgId, OnResActivityRankInfo);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResTaskActivityReward.MsgId, OnResTaskActivityReward);
            comp.RemoveListener(ResTaskActivityUpdate.MsgId, OnResTaskActivityUpdate);
            comp.RemoveListener(ResActivityRankInfo.MsgId, OnResActivityRankInfo);
        }
        private void OnResActivityRankInfo(Event evt)
        {
            var msg = GetCurMsg<ResActivityRankInfo>(evt.EventId);
            if (msg == null)
                return;
            ActivityService.Singleton.rankCom.Init(msg);
        }
        private void OnResTaskActivityReward(Event evt)
        {
            ResTaskActivityReward msg = GetCurMsg<ResTaskActivityReward>(evt.EventId);
        }
        private void OnResTaskActivityUpdate(Event evt)
        {
            ResTaskActivityUpdate msg = GetCurMsg<ResTaskActivityUpdate>(evt.EventId);
            var info = ActivityService.Singleton.Data.GetMainInfo(msg.id);
            var infoTask = info as TaskActivityData;
            if (info == null || infoTask == null)
            {
                return;
            }
          
            for (int i = 0; i < msg.item.Count; i++)
            {
                for (int j = 0; j < infoTask.items.Count; j++)
                {
                    if (infoTask.items[j].id == msg.item[i].id)
                    {
                        infoTask.items[j] = msg.item[i];
                        break;
                    }
                }
            }

            ActivityService.Singleton.Data.SetMainInfo(infoTask);
            ActivityService.Singleton.Data.SetActivityRed();
            GED.ED.dispatchEvent(EventID.RefreshActivityTask);
        }
        public void OnReqTaskActivityReward(int actId, int taskId)
        {
            ReqTaskActivityReward req = GetEmptyMsg<ReqTaskActivityReward>();
            req.id = actId;
            req.taskId = taskId;
            SendMsg(ref req);
        }

        public async Task<bool> OnAwardRankInfo(int id)
        {
            //yield_place_holder
            var msg = GetEmptyMsg<ReqActivityRankInfo>();
            msg.id = id;
            return await SendMsg(ref msg);
        }
    }
}
