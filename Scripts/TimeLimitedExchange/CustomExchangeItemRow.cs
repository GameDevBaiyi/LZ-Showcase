using System.Collections.Generic;
using Data.Beans;
using Logic.Bag;
using Logic.Common.BaiyiLibraries;
using Message.Activity;
using Message.Bag;
using Message.Pet;

namespace Logic.ActivityHall.TimeLimitedExchange
{
    public class CustomExchangeItemRow
    {
        public int consumedTimes;
        public int maxTimes;
        public int requestId;
        public List<ItemNeeded> itemNeededList;
        public ItemToGet itemToGet;
        public bool HasAllEnoughQuantity =>
            itemNeededList.TrueForAll(t => t.hasEnoughQuantity, ExtensionMethodsEnum.IsExtensionMethod);
        public bool HasLeftTimes => consumedTimes < maxTimes;

        /// <summary>
        /// 将从活动数据中拿到的 ExchangeItemCell 转化为更加详细的 兑换项数据 CustomExchangeItemRow.
        /// </summary>
        /// <param name="exchangeItemCell"></param>
        /// <param name="petGroups"></param>
        /// <returns></returns>
        public static CustomExchangeItemRow ExchangeItemToCustomConverter(ExchangeItemCell exchangeItemCell,
            Dictionary<int, List<PetInfo>> petGroups)
        {
            int[][] itemsNeedIdArray = exchangeItemCell.costs.SplitTo2IntArray(';', '+');
            List<ItemNeeded> itemNeededList = new List<ItemNeeded>();
            foreach (int[] itemIdAndCount in itemsNeedIdArray)
            {
                //ItemNeeded 的 Constructor 会根据三个参数: 道具 Id, 道具数量, 分组的宠物数据, 生成剩下的诸如"道具类型","玩家是否数量足够"等数据.
                itemNeededList.Add(new ItemNeeded(itemIdAndCount[0], itemIdAndCount[1], petGroups));
            }

            int[][] itemToGetArray = exchangeItemCell.items.SplitTo2IntArray(';', '+');
            return new CustomExchangeItemRow()
            {
                consumedTimes = exchangeItemCell.num,
                maxTimes = exchangeItemCell.max,
                requestId = exchangeItemCell.id,
                itemNeededList = itemNeededList,
                itemToGet = new ItemToGet()
                {
                    itemId = itemToGetArray[0][0],
                    itemCount = itemToGetArray[0][1]
                }
            };
        }

        public static List<ItemInfo> GetAllCostItems(List<ItemNeeded> itemNeededList)
        {
            List<ItemInfo> itemList = new List<ItemInfo>();
            foreach (ItemNeeded itemNeeded in itemNeededList)
            {
                if (itemNeeded.itemInfo == null) continue;

                itemList.Add(itemNeeded.itemInfo);
            }

            return itemList;
        }

        public static List<long> GetAllCostPets(List<ItemNeeded> itemNeededList)
        {
            List<long> petInsIdList = new List<long>();
            foreach (ItemNeeded itemNeeded in itemNeededList)
            {
                if (itemNeeded.petInstanceId == default(long)) continue;

                petInsIdList.Add(itemNeeded.petInstanceId);
            }

            return petInsIdList;
        }

        public static List<long> GetAllEquipInstanceIds(List<ItemNeeded> itemNeededList)
        {
            List<long> equipInsIdList = new List<long>();

            foreach (ItemNeeded itemNeeded in itemNeededList)
            {
                if (itemNeeded.equipInstanceIds == null) continue;

                equipInsIdList.AddRange(itemNeeded.equipInstanceIds);
            }

            return equipInsIdList;
        }
    }

    public class ItemNeeded
    {
        public int itemId;
        public int itemCount;
        public ItemTypeEnum itemType;
        public bool hasEnoughQuantity;

        public PetInfo petInfo; //同 Id 的最低等级的 pet 优先选取.
        public ItemInfo itemInfo; //指定被消耗的道具.
        public long petInstanceId; //指定被消耗的宠物.
        public List<long> equipInstanceIds = new List<long>(); //指定被消耗的装备. 可能有多个

        public ItemNeeded(int itemId, int itemCount, Dictionary<int, List<PetInfo>> petGroups)
        {
            this.itemId = itemId;
            this.itemCount = itemCount;

            t_itemBean itemBean = ConfigBean.GetBean<t_itemBean, int>(itemId);
            //道具为 宠物 时.
            if (itemBean.t_type == 1004)
            {
                itemType = ItemTypeEnum.Pet;
                if (petGroups.TryGetValue(itemId, out List<PetInfo> petInfos))
                {
                    petInfo = petInfos.MinByNonAlloc(t => t.level);
                    hasEnoughQuantity = true;
                    petInstanceId = petInfo.instanceId;
                }
                else
                {
                    hasEnoughQuantity = false;
                }

                return;
            }

            //道具为 装备时.
            if (itemBean.t_tab == 2)
            {
                itemType = ItemTypeEnum.Equipment;
                t_equipBean equipBean = ConfigBean.GetBean<t_equipBean, int>(itemId);
                List<EquipData> equipDataList =
                    BagService.Singleton.GetEquipByLevel(itemId, equipBean.t_type);
                hasEnoughQuantity = equipDataList.Count < itemCount;
                if (hasEnoughQuantity)
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        equipInstanceIds.Add(equipDataList[i].instanceId);
                    }
                }

                return;
            }

            //一般道具.
            itemType = ItemTypeEnum.Item;
            if (BagService.Singleton.ItemDic.TryGetValue(itemId, out ItemInfo itemInfoParameter))
            {
                hasEnoughQuantity = itemInfoParameter.num >= itemCount;
                if (hasEnoughQuantity)
                {
                    itemInfo = new ItemInfo()
                    {
                        id = itemId,
                        num = itemCount,
                    };
                }
            }
            else
            {
                hasEnoughQuantity = false;
            }
        }
    }


    public enum ItemTypeEnum
    {
        Item,
        Equipment,
        Pet
    }

    public struct ItemToGet
    {
        public int itemId;
        public int itemCount;
    }
}