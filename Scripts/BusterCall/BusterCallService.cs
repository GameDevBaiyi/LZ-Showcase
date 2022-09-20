using Base;
using Logic.Bag;
using Logic.Role;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using ServerLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Scripts.Logic.ActivityHall.BusterCall
{
    public class BusterCallService : SingletonService<BusterCallService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public ResBusterCallFight resBusterCallFight;//战斗返回数据
        public int CurFightActivityId = 0;//当前战斗的活动id
        List<int> canExchangeList = new List<int>();//可以兑换的道具
        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResBusterCallFight.MsgId, OnResBusterCallFight);
            comp.AddListener(ResBusterCallBuyNum.MsgId, OnResBusterCallBuyNum);
            GED.ED.addListener(EventID.BagMainData,OnBagData);
            GED.ED.addListener(EventID.BagDataChange, OnBagData);
        }


        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResBusterCallFight.MsgId, OnResBusterCallFight);
            comp.RemoveListener(ResBusterCallBuyNum.MsgId, OnResBusterCallBuyNum);
            GED.ED.removeListener(EventID.BagMainData, OnBagData);
            GED.ED.removeListener(EventID.BagDataChange, OnBagData);
        }

        #region ========================================请求=========================================
        /// <summary>
        /// 请求战斗
        /// </summary>
        public async Task<bool> OnReqFight(int id)
        {
            ReqBusterCallFight req = new ReqBusterCallFight();
            req.id = id;
            return await SendMsg(ref req);
        }

        /// <summary>
        /// 请求购买挑战次数
        /// </summary>
        public void OnReqBuy(int id)
        {
            ReqBusterCallBuyNum req = new ReqBusterCallBuyNum();
            req.id = id;
            SendMsg(ref req);
        }
        #endregion ========================================请求=========================================

        //请求战斗返回
        private void OnResBusterCallFight(Event obj)
        {
            ResBusterCallFight msg = GetCurMsg<ResBusterCallFight>(obj.EventId);
            CurFightActivityId = msg.id;
            resBusterCallFight = msg;
            var data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(msg.id);
            data.leftFightNum = msg.leftFightNum;
            GED.ED.dispatchEvent(EventID.BusterCallUpdateData, msg.id);
            UpdateRed(msg.id);
        }

        //请求购买挑战次数返回
        private void OnResBusterCallBuyNum(Event obj)
        {
            ResBusterCallBuyNum msg = GetCurMsg<ResBusterCallBuyNum>(obj.EventId);
            var data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(msg.id);
            data.leftFightNum = msg.leftFightNum;
            data.buyNum = msg.buyNum;
            GED.ED.dispatchEvent(EventID.BusterCallUpdateData, msg.id);
            UpdateRed(msg.id);
        }


        /// <summary>
        /// 获取购买次数
        /// </summary>
        /// <returns></returns>
        public int GetBuyNum()
        {
            var buyArr = 4200002.GetItsGlobalStr().SplitToIntArray('+');
            if (RoleService.Singleton.RoleInfo.vipLevel >= buyArr[0])
            {
                return buyArr[1];
            }
            return 0;
        }

        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="id"></param>
        public ActivityState CheckState(int id)
        {
            var data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(id);
            var curTime = TimeUtils.currentDateTime().Ticks;
            if (curTime < data.overTime)
            {
                return ActivityState.Open;
            }
            if (curTime >= data.overTime && curTime<data.closeTime)
            {
                return ActivityState.End;
            }
            if (curTime >= data.closeTime)
            {
                return ActivityState.Close;
            }
            return ActivityState.Close;
        }

        public override void ClearData()
        {
            base.ClearData();
            canExchangeList.Clear();
        }

        private void OnBagData()
        {
            var data = ActivityService.Singleton.Data.GetMainInfoByType((int)ActiveEnum.BusterCall);
            if (data != null)
            {
                UpdateRed(data.id);
            }
        }

        public void UpdateRed(int id)
        {
            var data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(id);
            var state = CheckState(id);
            RedDotManager.Singleton.SetRedDotValue(RedPath.Bustercall + $"/{id}/Fight", (data.leftFightNum > 0 && state == ActivityState.Open));
            var shopData = (CommonExchangeShopData)ActivityService.Singleton.Data.GetMainInfo(data.activity);
            RedDotManager.Singleton.SetRedDotValue(RedPath.Bustercall + $"/{id}/Shop", false);
            if (shopData != null)
            {
                bool red = false;
                for (int i = 0; i < shopData.iconList.Count; i++)
                {
                    var cur = shopData.iconList[i];
                    var has = BagService.Singleton.GetItemCount(cur);
                    if (has > 0)
                    {
                        for (int j = 0; j < shopData.itemList.Count; j++)
                        {
                            var curItem = shopData.itemList[j];
                            if (curItem.costInfo.id != cur)
                                continue;
                            if (curItem.costInfo.num <= has)
                            {
                                if(!canExchangeList.Contains(curItem.itemId))
                                {
                                    red = true;
                                    canExchangeList.Add(curItem.itemId);
                                }
                            }
                        }
                        if (red)
                        {
                            RedDotManager.Singleton.SetRedDotValue(RedPath.Bustercall + $"/{id}/Shop", true);
                            return;
                        }
                    }
                }
            }
        }
    }
}
