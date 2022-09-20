using Base;
using Logic.Service;
using Message.Activity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logic.ActivityHall.TaskRank
{
    public class RankAllInfo
    {
        public RankInfo self;
        public List<RankInfo> all = new List<RankInfo>();
        public List<RankConfigInfo> configs = new List<RankConfigInfo>();
    }

    public class TaskRankService : SingletonService<TaskRankService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public Dictionary<int, RankAllInfo> RankInfos { get; private set; } = new Dictionary<int, RankAllInfo>();

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResRankInfo.MsgId, OnResRankInfo);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResRankInfo.MsgId, OnResRankInfo);
        }

        #region req

        public Task<bool> OnReqRankInfo(int activityID)
        {
            var req = GetEmptyMsg<ReqRankInfo>();
            req.activityID = activityID;
            return SendMsg(ref req);
        }
        #endregion

        #region res
        private void OnResRankInfo(Base.Event evt)
        {
            var res = GetCurMsg<ResRankInfo>(evt.EventId);
            if (!RankInfos.ContainsKey(res.activityID))
                RankInfos.Add(res.activityID, new RankAllInfo());
            RankInfos[res.activityID].self = res.selfInfo;
            RankInfos[res.activityID].all.Clear();
            RankInfos[res.activityID].all.AddRange(res.RankList);
            RankInfos[res.activityID].configs.Clear();
            RankInfos[res.activityID].configs.AddRange(res.RankConfigList);
        }
        #endregion


        public List<RankInfo> GetRank(int activityID)
        {
            if (RankInfos.ContainsKey(activityID))
                return RankInfos[activityID].all;
            return new List<RankInfo>();
        }

        public RankInfo GetSelfRank(int activityID)
        {
            if (RankInfos.ContainsKey(activityID))
                return RankInfos[activityID].self;
            return new RankInfo()
            {
                rank = -1,
                value = 0,
            };
        }

        public List<RankConfigInfo> GetRankConfig(int activityID)
        {
            if (RankInfos.ContainsKey(activityID))
                return RankInfos[activityID].configs;
            return new List<RankConfigInfo>();
        }
    }
}
