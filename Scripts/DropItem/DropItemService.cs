using Base;
using Logic.Service;
using Message.Activity;
using Message.Bag;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logic.ActivityHall.DropItem
{
    public class DropItemService : SingletonService<DropItemService>
    {
        protected override int MainReqMsgId => ReqDropList.MsgId;

        protected override int MainResMsgId => ResDropList.MsgId;

        protected override Action<Event> MainResDecoder => null;

        public DropItemService()
        {
            AddListener(ResDropList.MsgId, OnResDropList);
        }

        public List<DropItemInfo> DropList { get; private set; } = new List<DropItemInfo>();


        public Task<bool> OnReqDropList()
        {
            var req = GetEmptyMsg<ReqDropList>();
            return SendMsg(ref req);
        }

        public void OnResDropList(Base.Event evt)
        {
            var res = GetCurMsg<ResDropList>(evt.EventId);
            DropList.Clear();
            DropList.AddRange(res.DropList);
        }

        public List<ItemInfo> GetDropItemList()
        {
            var list = new List<ItemInfo>();

            foreach (var drop in DropList)
                list.Add(new ItemInfo()
                {
                    id = drop.itemID,
                    num = drop.num,
                });

            return list;
        }

    }
}
