using Base;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Scripts.Logic.ActivityHall.CommonExchangeShop
{
    public class CommonExchangeShopService : SingletonService<CommonExchangeShopService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResExchangeItem.MsgId, OnResExchangeItem);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResExchangeItem.MsgId, OnResExchangeItem);
        }

        #region ========================================请求=========================================
        /// <summary>
        /// 请求购买
        /// </summary>
        public void OnReqBuy(int id,int index,int num)
        {
            ReqExchangeItem req = new ReqExchangeItem();
            req.id = id;
            req.itemId = index;
            req.num = num;
            SendMsg(ref req);
        }
        #endregion ========================================请求=========================================


        private void OnResExchangeItem(Event obj)
        {
            ResExchangeItem msg = GetCurMsg<ResExchangeItem>(obj.EventId);
            var data = (CommonExchangeShopData)ActivityService.Singleton.Data.GetMainInfo(msg.id);
            for (int i = 0; i < data.itemList.Count; i++)
            {
                var cur = data.itemList[i];
                if (cur.itemId == msg.itemId)
                {
                    cur.buyNum = msg.buyNum;
                }
            }
            GED.ED.dispatchEvent(EventID.CommonExchangeShopUpdate, msg.id);
        }
    }
}
