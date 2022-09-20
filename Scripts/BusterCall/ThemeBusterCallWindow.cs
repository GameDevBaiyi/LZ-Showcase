using Base;
using Battle.Entry;
using BusterCall;
using BuZhen;
using Common;
using Data.Beans;
using FairyGUI;
using GameLogic.Scripts.Logic.ActivityHall.CommonExchangeShop;
using Logic.Bag;
using Logic.BattleLogic;
using Logic.BuZhen;
using Logic.Common;
using Logic.Role;
using Logic.Sigin.Com;
using Message.Activity;
using Message.Bag;
using Message.Fight;
using ServerLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Common.UI_ItemIcon;

namespace GameLogic.Scripts.Logic.ActivityHall.BusterCall
{
    class ThemeBusterCallWindow : BindWindow<UI_ThemeBusterCallWindow>
    {
        int themeId;
        int activiId = 0;
        BusterCallData data = null;
        long corId = 0;//倒计时
        protected override void OnOpen()
        {
            base.OnOpen();
            if (null != param)
            {
                TwoParam<int, int> twoParam = param as TwoParam<int, int>;
                if (twoParam == null)
                    return;
                themeId = twoParam.value1;
                activiId = twoParam.value2;
                data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(activiId);

                AddListener(EventID.BusterCallUpdateData, RefreshView);
                AddListener(EventID.BusterCallStartBattle, PetStationChange);
                AddListener(EventID.HongDongUpdate, RefreshView);

                InitBtn();
                InitView();
            }
            else
            {
                View.m_mijiGroup.visible = true;
                View.m_mijiBtn.onClick.Add(() =>
                {
                    string tempStr = View.m_mijiText.inputTextField.text;
                    if (string.IsNullOrEmpty(tempStr))
                        return;

                    var bossBeanList = ConfigBean.GetBeanList<t_monsterBean>();
                    foreach (var bean in bossBeanList)
                    {
                        if (bean.t_monster_id == int.Parse(tempStr))
                        {
                            SetModel(View.m_model, bean.t_battle_prefab);
                            break;
                        }
                    }
                });
            }
        }


        private void RefreshView(Base.Event obj)
        {
            data = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(activiId);
            if (data == null)
            {
                Close();
                return;
            }
            InitView(true);
        }

        void InitBtn()
        {
            RedDotManager.Singleton.RegisterRedDot(RedPath.mainHuoDong + $"/{activiId}/Shop", View.m_btnShop.m_red);
            View.m_challengeBtn.title = 4200008.GetItsLanaugeStr();// "进入挑战";
            View.m_challengeBtn.onClick.Set(OnClickFight);
            View.m_btnReward.onClick.Set(OnClickReward);
            View.m_btnQ.onClick.Set(OnClickHelp);
            View.m_btnBuy.onClick.Set(OnClickBuy);
            //跳过
            View.m_comJump.m_btngou.onChanged.Set(OnJumpClick);
            View.m_btnShop.onClick.Set(OnClickShop);
        }

        void InitView(bool isRefresh = false)
        {
            var state = BusterCallService.Singleton.CheckState(activiId);
            View.m_groupFight.visible = (state == ActivityState.Open);
            View.m_txtEnd.visible = (state == ActivityState.End);
            //购买次数
            //View.m_comBuyTimes.m_txt.text = string.Format(4200005.GetItsLanaugeStr(), 4200002.GetItsGlobalStr().SplitToIntArray('+')[1]);
            if (data.leftFightNum > 0)
            {
                View.m_txtLeftNum.text = string.Format(4200006.GetItsLanaugeStr(), string.Format("[color=#239630]{0}[/color]", data.leftFightNum), 4200001.GetItsGlobalInt());
            }
            else
            {
                View.m_txtLeftNum.text = string.Format(4200006.GetItsLanaugeStr(), string.Format("[color=#ff0000]{0}[/color]", data.leftFightNum), 4200001.GetItsGlobalInt());
            }
            //boss信息
            var bossBean = ConfigBean.GetBean<t_monsterBean, int>(data.bossId);
            Debuger.Log($"data.bossId:{data.bossId}");
            View.m_nameTxt.text = bossBean.t_name;
            Debuger.Log($"bossBean.t_name:{bossBean.t_name}");
            View.m_txtLevel.text = 4200002.GetItsLanaugeStr() + data.bossLevel;
            //UIGloader.SetUrl(View.m_bossTypeLoader, ClientTools.GetRaceIcon((PetRace)bossBean.t_camp));

            UIGloader.SetUrl(View.m_camp.m_icon, ClientTools.GetRaceIconExt02((PetRace)bossBean.t_camp));
            UIGloader.SetUrl(View.m_camp.m_frame, ClientTools.GetRaceFrame((PetRace)bossBean.t_camp));
            var skills = bossBean.t_skill.SplitToIntArray('+');
            for (var i = 0; i < skills.Length; i++)
            {
                var skillbean = ConfigBean.GetBean<t_skillBean, int>(skills[i]);
                if (skillbean != null)
                {
                    var icon = (UI_SkillIcon)View.m_skillList.GetChildAt(i);
                    if (icon != null)
                    {
                        if (skillbean.t_icon.Length > 0)
                        {
                            icon.visible = true;
                            icon.Init(skillbean.t_id, 1, showLevel: false, showBg: false);
                        }
                    }
                }
            }
            var bossEntity = BattleMonsterEntity.Build(data.bossId, data.bossLevel);
            View.m_txtPower.text = bossEntity != null ? bossEntity.FightPower.ToString() : "0";

            View.m_btnHelp.onClick.Set(() => { OnClickDetail(bossEntity); });


            //基础属性
            SetBaseProperty(bossEntity);

            Debuger.Log($"bossBean.t_battle_prefab:{bossBean.t_battle_prefab}");
            //模型
            if (!isRefresh)
                SetModel(View.m_model, bossBean.t_battle_prefab);

            //奖励预览
            View.m_comReward.m_listReward.itemRenderer = RewardItemRenderer;
            View.m_comReward.m_listReward.numItems = data.iconList.Count;
            View.m_comReward.m_listReward.scrollPane.touchEffect = data.iconList.Count >= 3;

            //倒计时
            if (corId != 0)
                StopTimerOrCoroutine(corId);
            corId = AddTimer(0, 1, () =>
            {
                var curTime = TimeUtils.currentDateTime().Ticks;
                var endTime = curTime >= data.overTime ? data.closeTime : data.overTime;
                var countDownStr = ClientTools.CountTime(endTime);
                if (countDownStr.Equals("00:00:00"))
                {
                    if (curTime > data.closeTime)
                    {
                        StopTimerOrCoroutine(corId);
                    }
                    else
                    {
                        InitView(true);
                    }
                }

                if (curTime <= data.overTime)
                {
                    View.m_txtTime.text = 4200011.GetItsLanaugeStr() + countDownStr;
                }
                else if (curTime > data.overTime && curTime < data.closeTime)
                {
                    View.m_txtTime.text = 4200015.GetItsLanaugeStr() + countDownStr;
                }
                else
                {
                    View.m_txtTime.text = 4200016.GetItsLanaugeStr();
                }
            });
        }

        //点击详细数据
        private void OnClickDetail(BattleMonsterEntity bossEntity)
        {
            OpenChild<BusterCallPropertyWindow>(BusterCallPackage.packageId, UILayer.TopHUD, false, bossEntity);
        }

        //点击帮助信息
        void OnClickHelp()
        {
            var param = new TwoParam<int, int>() { value1 = 4120458, value2 = 4200028 };
            WinMgr.Singleton.Open<SimpleRuleWindow>(CommonPackage.packageId, UILayer.Popup, false, param);
        }

        //购买
        private void OnClickBuy()
        {
            var state = BusterCallService.Singleton.CheckState(activiId);
            if (state != ActivityState.Open)
            {
                TipWindow.Singleton.ShowTip(4200016);
                return;
            }
            var buyArr = 4200002.GetItsGlobalStr().SplitToIntArray('+');
            var canBuyNum = buyArr[1];
            if (RoleService.Singleton.RoleInfo.vipLevel >= buyArr[0])
            {
                if (data.buyNum >= canBuyNum)
                {
                    TipWindow.Singleton.ShowTip(4200027);
                }
                else
                {
                    var leftBuyNum = canBuyNum - data.buyNum;
                    var buyCostArr = 4200003.GetItsGlobalStr().SplitToIntArray('+');
                    var cost = 0;
                    if (buyCostArr.Length > data.buyNum)
                    {
                        cost = buyCostArr[data.buyNum];
                    }
                    else
                    {
                        cost = buyCostArr[buyCostArr.Length - 1];
                    }
                    ConfirmWindow.Singleton.ShowTip(string.Format(4200026.GetItsLanaugeStr(), cost, leftBuyNum), () =>
                    {
                        if (BagService.Singleton.Diamond >= cost)
                        {
                            BusterCallService.Singleton.OnReqBuy(data.id);
                        }
                        else
                        {
                            TipWindow.Singleton.ShowTip(1090022);
                        }
                    });
                }
            }
            else
            {
                var leftBuyNum = canBuyNum - data.buyNum;
                var msg = string.Format(4200005.GetItsLanaugeStr(), leftBuyNum);
                TipWindow.Singleton.ShowTip(msg);
            }
        }

        private void OnJumpClick(EventContext context)
        {
            var clicked = View.m_comJump.m_btngou.selected;
            if (clicked && (data.leftFightNum == (4200001.GetItsGlobalInt() + data.buyNum)))
            {
                TipWindow.Singleton.ShowTip(4200030);
                View.m_comJump.m_btngou.selected = false;
            }
        }

        //奖励
        private void RewardItemRenderer(int index, GObject item)
        {
            var cur = data.iconList[index];
            UI_ItemIcon obj = item as UI_ItemIcon;
            obj.Init(cur, ShowModel.None, -1, -1, true);
        }

        /// <summary>
        /// 请求挑战
        /// </summary>
        void OnClickFight()
        {
            var state = BusterCallService.Singleton.CheckState(activiId);
            if (state != ActivityState.Open)
            {
                TipWindow.Singleton.ShowTip(4200016);
                return;
            }

            if (data.leftFightNum <= 0)
            {
                if (data.buyNum < BusterCallService.Singleton.GetBuyNum())
                {
                    OnClickBuy();
                    return;
                }
                else
                {
                    TipWindow.Singleton.ShowTip(4200017);
                    return;
                }
            }

            if (View.m_comJump.m_btngou.selected)
            {
                PetStationChange(Base.Event.NULL);
            }
            else
            {
                ThreeParam<int, int, bool> param = new ThreeParam<int, int, bool>
                {
                    value1 = (int)FightType.ThemeBusterCall,
                    value2 = (int)StationType.BusterCall,
                    value3 = false,
                };
                WinMgr.Singleton.Open<BuZhenWindow>(BuZhenPackage.packageId, UILayer.Popup, false, param);
            }
        }


        /// <summary>
        /// 显示奖励
        /// </summary>
        void OnClickReward()
        {
            OpenChild<BusterCallRewardWindow>(BusterCallPackage.packageId, UILayer.TopHUD, false, activiId);
        }

        //点击商店
        void OnClickShop()
        {
            if (data.activity != 0)
            {
                //进入界面关闭红点
                RedDotManager.Singleton.SetRedDotValue(RedPath.mainHuoDong + $"/{activiId}/Shop", false);
                OpenChild<CommonExchangeShopWindow>(BusterCallPackage.packageId, UILayer.TopHUD, false, data.activity);
            }
        }

        void SetModel(GGraph ggrah, int modelId)
        {
            UIPetLoader loader = new UIPetLoader(this.resLoader);
            var model = loader.SetModelByModelId(ggrah, modelId, false, false);
            Vector3 position = new Vector3(-40f, -360f, 400f);
            Vector3 scale = Vector3.one * 250f;
            float rotationY = -190f;
            var pbean = ConfigBean.GetBean<Data.Beans.t_prefabBean, int>(modelId);
            if (pbean != null)
            {
                if (pbean.t_scale_wanted > 0)
                {
                    scale = Vector3.one * pbean.t_scale_wanted;
                }
                if (!string.IsNullOrEmpty(pbean.t_position_wanted))
                {
                    var pos = pbean.t_position_wanted.SplitToIntArray(';');
                    if (pos.Length == 3)
                    {
                        position = new Vector3(pos[0], pos[1], pos[2]);
                    }
                }
                if (pbean.t_rotate_wanted != 0)
                {
                    rotationY = pbean.t_rotate_wanted;
                }
            }

            if (model != null)
            {
                //设置位置
                model.transform.localPosition = position;
                model.transform.localScale = scale;
                model.transform.setLocalRotationY(rotationY);
                loader.SetDefaultAni("normal_idle");
            }
        }

        private void SetBaseProperty(BattleMonsterEntity bossEntity)
        {
            var beanList = ConfigBean.GetBeanList<Data.Beans.t_propertyBean>();
            View.m_p1.text = GetPropertyBean(2).t_name + ":" + (int)bossEntity.UpdateProperty()[(int)PropertyType.Atk];
            View.m_p2.text = GetPropertyBean(1).t_name + ":" + (int)bossEntity.UpdateProperty()[(int)PropertyType.MaxHP];
            View.m_p3.text = GetPropertyBean(3).t_name + ":" + (int)bossEntity.UpdateProperty()[(int)PropertyType.Def];
            View.m_p4.text = GetPropertyBean(4).t_name + ":" + (int)bossEntity.UpdateProperty()[(int)PropertyType.Speed];
        }

        /// <summary>
        /// 获取属性配置
        /// </summary>
        /// <param name="id"></param>
        t_propertyBean GetPropertyBean(int id)
        {
            return ConfigBean.GetBean<Data.Beans.t_propertyBean, int>(id);
        }


        private async void PetStationChange(Base.Event obj)
        {
            if (await BusterCallService.Singleton.OnReqFight(activiId))
            {
                if (BusterCallService.Singleton.resBusterCallFight != null && BusterCallService.Singleton.resBusterCallFight.replay != null)
                {
                    if (View.m_comJump.m_btngou.selected)
                    {
                        //显示奖励
                        TwoParam<List<ItemInfo>, List<long>> param = new TwoParam<List<ItemInfo>, List<long>>();
                        param.value1 = BusterCallService.Singleton.resBusterCallFight.rewardList;
                        WinMgr.Singleton.Open<ItemGetWindow>(CommonPackage.packageId, UILayer.TopHUD, false, param);
                    }
                    else
                    {
                        TwoParam<int, int> twoParam = new TwoParam<int, int> { value1 = themeId, value2 = activiId };

                        if (BusterCallService.Singleton.resBusterCallFight.id == activiId)
                        {
                            //修改战斗类型
                            BusterCallService.Singleton.resBusterCallFight.replay.fightType = (short)FightType.ThemeBusterCall;
                            FightTools.Singleton.StartBattle(BusterCallService.Singleton.resBusterCallFight.replay, BusterCallService.Singleton.resBusterCallFight, SceneState.BusterCallFight, "lvl_gq_jingjichang", true, twoParam);
                        }
                        
                    }
                }
            }
        }

    }
}
