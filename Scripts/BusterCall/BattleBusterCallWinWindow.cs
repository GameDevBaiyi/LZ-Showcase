using Common;
using FakeBattle;
using Logic.Bag;
using Logic.BattleLogic;
using Logic.Chapter;
using Logic.Common;
using Logic.Role;
using Logic.Sigin.Com;
using Message.Activity;
using Message.Bag;
using Message.Fight;
using Message.Pet;
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
    public class BattleBusterCallWinWindow : BindWindow<UI_BattleBusterCallWinWindow>
    {
        ReplayInfo replayInfo;//回访
        UIPetLoader uiPetLoader;

        protected override void OnOpen()
        {
            base.OnOpen();

            GED.ED.dispatchEvent(EventID.HideModel, new Base.TwoParam<long, bool> { value1 = winId, value2 = true });

            AudioManager.Singleton.PlayEffect("music_bgm_fight_win");

            InitView();
        }

        void InitView()
        {
            View.m_FightDataBut.onClick.Add(onTongJi);
            View.m_mask.onClick.Add(Close);
            View.m_modelHolder.touchable = false;
            View.m_ModelRoot.touchable = false;
            View.m_txtBattle.text = string.Format(2020134.GetItsLanaugeStr(), 4200001.GetItsLanaugeStr());

            var rewardList = new List<ItemInfo>();
            var type = param.GetType();
            if (type == typeof(Message.Activity.ResBusterCallFight))
            {
                var msg = (Message.Activity.ResBusterCallFight)param;
                replayInfo = msg.replay;
                rewardList.AddRange(msg.rewardList);
            }

            //设置数据
            var role = RoleService.Singleton.RoleInfo;
            if (role != null)
            {
                View.m_HeadIcon.Init(role.headIcon, role.headFrame, role.vipLevel);
                View.m_HeadIcon.touchable = false;
                View.m_LevelText.text = $"Lv.{role.level}";
                var bean = ConfigBean.GetBean<Data.Beans.t_role_level_upBean, int>(role.level);//升级经验
                var value = BagService.Singleton.GetItemCount((int)CurrencyType.TeamExp);
                View.m_ProgressBar.GetChild("title").text = $"{value}/{bean.t_exp}";
                float cur = (float)value;
                View.m_ProgressBar.max = bean.t_exp;
                View.m_ProgressBar.value = cur;
            }
            //展示boss血量
            ShowBossHp();
            //奖励
            foreach (var item in rewardList)
            {
                var cell = (UI_ItemIcon)View.m_AwardList.AddItemFromPool();
                cell.Init(item.id, item.num);
            }
            //展示MVP 形象
            ShowMvp();
            //动效完成后 加载特效
            DelayCall(1.5f, () =>
            {
                uiPetLoader.SetParticleMoudle(View.m_lightGrap, "eff_ui_zhandoushengli_shanguang", "zhandoushengli");
            });
        }

        void onTongJi()
        {
            OpenChild<BattleHurtInfoWindow>(FakeBattlePackage.packageId, UILayer.Popup, false, replayInfo);
        }

        protected override void OnClose()
        {
            base.OnClose();
            FightTools.Singleton.EndFight((FightType)replayInfo.fightType);
            GED.ED.dispatchEvent(EventID.RoleInfoChange);
            GED.ED.dispatchEvent(EventID.HideModel, new Base.TwoParam<long, bool> { value1 = winId, value2 = false });
            RoleService.Singleton.ShowLvlUpWindow();
        }

        void ShowMvp()
        {
            var fightStatistics = new List<FightStatistics>();
            fightStatistics = replayInfo.fightList[0].statistics;
            var replayEntityInfolist = new List<ReplayEntityInfo>();
            replayEntityInfolist = replayInfo.fightList[0].player1.entityList;
            ReplayEntityInfo item = GetMvpFightStatistics(fightStatistics);
            if (item.type == 3)
                return;
            if (item == null)
            {
                return;
            }
            uiPetLoader = new UIPetLoader(this.resLoader, View.m_ModelRoot);
            PetInfo petInfo = new PetInfo()
            {
                petId = item.templateId,
                star = item.star,
                level = item.level,
                skinId = item.model,
            };

            //结算模型偏移
            var tpbean = ConfigBean.GetBean<Data.Beans.t_petBean, int>(petInfo.petId);
            var pbean = ConfigBean.GetBean<Data.Beans.t_prefabBean, int>(tpbean.t_prefab);
            if (tpbean == null || pbean == null)
                return;
            GameObject to = uiPetLoader.SetPetModel(View.m_modelHolder, petInfo, false, false);
            int[] scales = pbean.t_scale_settlement.SplitToIntArray(';');
            int[] positions = pbean.t_position_settlement.SplitToIntArray(';');
            to.transform.localScale = new Vector3(scales[0], scales[1], scales[2]);
            to.transform.setLocalRotationY(pbean.t_rotate_settlement);
            to.transform.localPosition = new Vector3(positions[0], positions[1], positions[2]);

            uiPetLoader.PlayAnimation("normal_idle", 0);
        }

        ReplayEntityInfo GetMvpFightStatistics(List<FightStatistics> endStatisList)
        {
            List<ReplayEntityInfo> etList = replayInfo.fightList[0].player1.entityList;
            if (endStatisList.Count < 1)
            {
                if (etList.Count > 0)
                    return etList[0];
                return null;
            }

            FightStatistics fs = null;
            ReplayEntityInfo tem = null;
            foreach (var sta in endStatisList)
            {
                foreach (var ent in etList)
                {
                    if (sta.entityId == ent.entityId)
                    {
                        if (fs == null || sta.exportHurt > fs.exportHurt)
                        {
                            if (ent.type == 3)
                                continue;
                            tem = ent;
                            fs = sta;
                        }
                        break;
                    }
                }
            }
            return tem;
        }

        /// <summary>
        /// 显示boss血量
        /// </summary>
        void ShowBossHp()
        {
            var activityData = (BusterCallData)ActivityService.Singleton.Data.GetMainInfo(BusterCallService.Singleton.CurFightActivityId);
            var bossId = activityData.bossId;
            View.m_bosshead.InitMonster(bossId);
            View.m_bosshead.touchable = false;
            View.m_bosshead.m_level.text = activityData.bossLevel.ToString();

            int leftHpRatio = 0;//剩余血量万分比
            if (replayInfo.fightList.Count > 0)
            {
                var fightInfo = replayInfo.fightList[0];
                foreach (var st in fightInfo.statistics)
                {
                    foreach (var et in fightInfo.player2.entityList)
                    {
                        if (st.entityId == et.entityId)
                        {
                            var hp = LNumber.Create_Row(st.hp);
                            var maxHp = LNumber.Create_Row(st.maxHp);
                            if (et.type != 3)
                            {
                                leftHpRatio = (int)(((hp * 1d) / maxHp) * 10000);
                            }
                        }
                    }
                }
            }

            //溢出临界值处理
            if (leftHpRatio < 0)
                leftHpRatio = 0;
            if (leftHpRatio > 10000)
                leftHpRatio = 10000;

            var percent = leftHpRatio / 100f;
            View.m_bar.value = percent;
            View.m_bar.m_qipao.width = View.m_bar.m_bar.actualWidth * leftHpRatio / 10000f;
            View.m_bar.m_txt.text = string.Format("{0}%", (int)percent);

            View.m_bar.m_p1.text = "20%";
            View.m_bar.m_p2.text = "40%";
            View.m_bar.m_p3.text = "60%";
            View.m_bar.m_p4.text = "80%";
            View.m_bar.m_p5.text = "100%";
        }
    }
}
