using Battle.Entry;
using BusterCall;
using Data.Beans;
using FairyGUI;
using Logic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Scripts.Logic.ActivityHall.BusterCall
{
    public class BusterCallPropertyWindow :BindWindow<UI_BusterCallPropertyWindow>
    {
        private int endIndex = 0;
        private List<t_propertyBean> beanList;
        BattleMonsterEntity bossEntity;

        protected override void OnOpen()
        {
            base.OnOpen();
            View.onClick.Add(Close);
            bossEntity = (BattleMonsterEntity)param;
            InitData();
            SetBaseProperty();
            SetSpecialProperty();
            ClientTools.PlayWindowOpenAnimationWithRebound(View, View.m_bg);
        }

        private void InitData()
        {
            beanList = ConfigBean.GetBeanList<Data.Beans.t_propertyBean>();
        }

        private void SetBaseProperty()
        {
            endIndex = 0;
            int index = 0;
            GTextField[] baseValues = { View.m_p2, View.m_p1, View.m_p3, View.m_p4 };
            for (int i = 0; i <= beanList.Count; i++)
            {
                if (beanList[i].t_dis <= 0)
                    continue;
                if (index < baseValues.Length)
                {
                    baseValues[index].text = GetPropertyStr(beanList[i]);
                    index++;
                }
                else
                {
                    endIndex = i;
                    break;
                }
            }
        }

        private void SetSpecialProperty()
        {
            View.m_pList.RemoveChildrenToPool();
            while (endIndex < beanList.Count)
            {
                if (beanList[endIndex].t_dis <= 0)
                {
                    endIndex++;
                    continue;
                }
                var item = (UI_PetPropertyCell)View.m_pList.AddItemFromPool();
                item.m_txt1.text = GetPropertyStr(beanList[endIndex]);
                endIndex++;
                if (endIndex < beanList.Count)
                {
                    if (beanList[endIndex].t_dis <= 0)
                    {
                        endIndex++;
                        continue;
                    }
                    item.m_txt2.text = GetPropertyStr(beanList[endIndex]);
                    endIndex++;
                }
            }
        }


        private string GetPropertyStr(t_propertyBean data)
        {
            string str = "";
            str = data.t_name;
            if (data.t_value_type == 1)
                str = str + ": " + (long)GetValue(data.t_id);
            else
            {
                var value = GetValue(data.t_id);
                if (value % 100 > 0)
                    str = str + ": " + ((float)(value / 100)).ToString("0.0") + "%";
                else
                    str = str + ": " + ((float)(value / 100)).ToString("F0") + "%";
            }
            return str;
        }

        LNumber GetValue(int id)
        {
            var valueMap = bossEntity.UpdateProperty();
            if (valueMap.ContainsKey(id))
            {
                return valueMap[id];
            }
            return 0;
        }
    }
}
