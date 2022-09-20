using System;
using System.Collections.Generic;
using Logic.Service;
using Logic.Sigin.Com;
using Message.Activity;
using System.Threading.Tasks;
using Data.Beans;
using Logic.Common.BaiyiLibraries;
using Logic.Pet;
using Message.Bag;
using Message.Pet;
using ServerLink;
using UnityEngine;
using Event = Base.Event;

namespace Logic.ActivityHall.TimeLimitedExchange
{
    public class TimeLimitedExchangeService : SingletonService<TimeLimitedExchangeService>
    {
        protected override int MainReqMsgId => 0;

        protected override int MainResMsgId => 0;

        protected override Action<Event> MainResDecoder => null;

        //"限时兑换活动" 的红点检测需要 检测背包宠物装备等, 所以提前手动输入活动Id. 
        public ExchangeItemData activityData;
        public int activityId;
        public List<CustomExchangeItemRow> rowDataList;
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public void RefreshServiceData()
        {
            if (!TryGetActivityData()) return;
            activityId = activityData.id;
            //将 ExchangeItemCell 转成 CustomExchangeItemRow.
            rowDataList = GetRowDataArray();
            rowDataList.Sort(CustomExchangeItemComparision);
            rowDataList.Reverse();
            RedDotManager.Singleton.SetRedDotValue(RedPath.mainHuoDong + $"/{activityId}",
                rowDataList.Exists(t => t.HasAllEnoughQuantity && t.HasLeftTimes,
                    ExtensionMethodsEnum.IsExtensionMethod));
            GED.ED.dispatchEvent(EventID.TimeLimitedExchangeUpdate);
        }

        private bool TryGetActivityData()
        {
            activityData =
                ActivityService.Singleton.Data.GetMainInfoByType(ActiveEnum.TimeLimitedExchange)[0] as ExchangeItemData;

            if (activityData == null)
            {
                _logger.Error(
                    "未能通过 ActivityService.Singleton.Data.GetMainInfo(_activityID) as ExchangeItemData 找到 TimeLimitedExchange,即限时兑换 的活动数据. ");
                return false;
            }

            if (activityData.type != (int)ActiveEnum.TimeLimitedExchange)
            {
                _logger.Error($"id 与类型对应不上，当前id:{activityId}," +
                              $"当前类型：{activityData.type}");
                return false;
            }

            return true;
        }

        public int CustomExchangeItemComparision(CustomExchangeItemRow rowA, CustomExchangeItemRow rowB)
        {
            int comparisionValue = rowA.HasLeftTimes.CompareTo(rowB.HasLeftTimes);

            if (comparisionValue != 0)
            {
                return comparisionValue;
            }

            comparisionValue = rowA.HasAllEnoughQuantity.CompareTo(rowB.HasAllEnoughQuantity);
            if (comparisionValue != 0)
            {
                return comparisionValue;
            }

            return 0;
        }

        /// <summary>
        ///  根据活动的 List<ExchangeItemCell> 数据, 生成更详细的每一行的兑换项数据.
        /// </summary>
        /// <typeparam name="ExchangeItemCell"></typeparam>
        /// <returns></returns>
        private List<CustomExchangeItemRow> GetRowDataArray()
        {
            List<ExchangeItemCell> exchangeItemCells = activityData.items;
            int exchangeItemCount = exchangeItemCells.Count;
            List<CustomExchangeItemRow> itemRows = new List<CustomExchangeItemRow>(exchangeItemCount);

            //将 现有的宠物 按 整卡Id 分组, 再根据 同整卡Id 选择该组等级最低的优先标记为消耗.
            Dictionary<int, List<PetInfo>> petGroups =
                PetService.Singleton.petMap.GroupBy(t => ConfigBean.GetBean<t_petBean, int>(t.petId).t_complete_card);

            for (int i = 0; i < exchangeItemCount; i++)
            {
                itemRows.Add(CustomExchangeItemRow.ExchangeItemToCustomConverter(exchangeItemCells[i], petGroups));
            }

            return itemRows;
        }

        public void SelfRegisterEventListener(NetEventComp comp)
        {
            comp.AddListener(ResOldExchangeItem.MsgId, OnResTimeLimitedExchange);
        }

        public void SelfUnRegisterEventListener(NetEventComp comp)
        {
            comp.RemoveListener(ResOldExchangeItem.MsgId, OnResTimeLimitedExchange);
        }


        #region req

        public Task<bool> OnReqTimeLimitedExchange(int activityID, int itemId, List<long> equipInsIdList,
            List<long> petInsIdList, List<ItemInfo> itemList)
        {
            var req = GetEmptyMsg<ReqTimeLimitedExchange>();
            req.id = activityID;
            req.itemId = itemId;
            req.equipInsIdList.AddRange(equipInsIdList);
            req.petInsIdList.AddRange(petInsIdList);
            req.itemList.AddRange(itemList);
            return SendMsg(ref req);
        }

        #endregion

        #region res

        private void OnResTimeLimitedExchange(Base.Event evt)
        {
            ResOldExchangeItem changedData = GetCurMsg<ResOldExchangeItem>(evt.EventId);
            int cellToChangeIndex =
                activityData.items.FindIndex(t => t.id == changedData.item.id,
                    ExtensionMethodsEnum.IsExtensionMethod);
            activityData.items[cellToChangeIndex] = changedData.item;
            ActivityService.Singleton.Data.SetMainInfo(activityData);
            RefreshServiceData();
        }

        #endregion
    }
}