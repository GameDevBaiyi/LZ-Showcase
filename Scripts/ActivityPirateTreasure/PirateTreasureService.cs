using Base;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using Message.Bag;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logic.ActivityPirateTreasure
{
    public class PirateTreasureService : SingletonService<PirateTreasureService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public TwoParam<List<ItemInfo>, List<long>> oneKeyParam = new TwoParam<List<ItemInfo>, List<long>>();
        public bool endIsBig = false;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResPirateTreasureMainRewardData.MsgId, OnResMainRewardData);
            comp.AddListener(ResPirateTreasureChangeMainReward.MsgId, OnResChangeMainReward);
            comp.AddListener(ResPirateTreasurePickOne.MsgId, OnResPickOne);
            comp.AddListener(ResPirateTreasurePreview.MsgId, OnResPreview);
            comp.AddListener(ResPirateTreasureNextRound.MsgId, OnResNextRound);
            comp.AddListener(ResPirateTreasurePickOneKey.MsgId, OnResOneKey);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResPirateTreasureMainRewardData.MsgId, OnResMainRewardData);
            comp.RemoveListener(ResPirateTreasureChangeMainReward.MsgId, OnResChangeMainReward);
            comp.RemoveListener(ResPirateTreasurePickOne.MsgId, OnResPickOne);
            comp.RemoveListener(ResPirateTreasurePreview.MsgId, OnResPreview);
            comp.RemoveListener(ResPirateTreasureNextRound.MsgId, OnResNextRound);
            comp.RemoveListener(ResPirateTreasurePickOneKey.MsgId, OnResOneKey);
        }

        //返回
        #region
        private void OnResMainRewardData(Event evt)
        {
            var msg = GetCurMsg<ResPirateTreasureMainRewardData>(evt.EventId);
            ActivityService.Singleton.Data.SetPTMianReward(msg);
        }
        private void OnResChangeMainReward(Event evt)
        {
            var msg = GetCurMsg<ResPirateTreasureChangeMainReward>(evt.EventId);
            ActivityService.Singleton.Data.RefreshPTMianRewardId(msg.mainReward, msg.id);
        }
        private void OnResPickOne(Event evt)
        {
            var msg = GetCurMsg<ResPirateTreasurePickOne>(evt.EventId);
            ActivityService.Singleton.Data.PTPickReturn(msg);
        }
        private void OnResPreview(Event evt)
        {
            var msg = GetCurMsg<ResPirateTreasurePreview>(evt.EventId);
            ActivityService.Singleton.Data.PTPreviewData(msg.previews);
        }
        private void OnResNextRound(Event evt)
        {
            var msg = GetCurMsg<ResPirateTreasureNextRound>(evt.EventId);
            ActivityService.Singleton.Data.PTNextRound(msg.currentRound, msg.id);
        }
        private void OnResOneKey(Event evt)
        {
            var msg = GetCurMsg<ResPirateTreasurePickOneKey>(evt.EventId);
            oneKeyParam.value1 = new List<ItemInfo>();
            oneKeyParam.value2 = new List<long>();
            endIsBig = msg.grid != null && msg.grid.Count > 0 && msg.grid[msg.grid.Count - 1].mainReward;
            ActivityService.Singleton.Data.PTPickReturn(msg);
        }
        #endregion

        //请求
        #region 
        //请求大奖信息
        public void OnReqMainRewardData(int activityId)
        {
            var msg = GetEmptyMsg<ReqPirateTreasureMainRewardData>();
            msg.id = activityId;
            SendMsg(ref msg);
        }
        //请求选择大奖
        public void OnReqChangeMainReward(int Id, int activityId)
        {
            var msg = GetEmptyMsg<ReqPirateTreasureChangeMainReward>();
            msg.mainRewardId = Id;
            msg.id = activityId;
            SendMsg(ref msg);
        }
        //请求翻牌
        public void OnReqPickOne(int index, int activityId)
        {
            var msg = GetEmptyMsg<ReqPirateTreasurePickOne>();
            msg.index = index;
            msg.id = activityId;
            SendMsg(ref msg);
        }
        //请求预览信息
        public void OnReqPreview(int activityId)
        {
            var msg = GetEmptyMsg<ReqPirateTreasurePreview>();
            msg.id = activityId;
            SendMsg(ref msg);
        }
        //请求进入下一轮
        public void OnReqNextRound(int activityId)
        {
            var msg = GetEmptyMsg<ReqPirateTreasureNextRound>();
            msg.id = activityId;
            SendMsg(ref msg);
        }
        //请求一键
        public Task<bool> OnReqOneKey(int activityId)
        {
            var msg = GetEmptyMsg<ReqPirateTreasurePickOneKey>();
            msg.id = activityId;
            return SendMsg(ref msg);
        }
        #endregion 
    }
}
