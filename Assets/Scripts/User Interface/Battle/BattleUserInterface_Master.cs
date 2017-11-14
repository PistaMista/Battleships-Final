﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleUIType
{
    TURN_NOTIFIER,
    FLEET_PLACEMENT,
    BATTLE_OVERVIEW,
    ATTACK_VIEW,
    ATTACKER_INFO,
    DAMAGE_REPORT
}
public class BattleUserInterface_Master : InputEnabledUserInterface
{
    static BattleUserInterface_Master it;
    public PrimaryBattleUserInterface[] primaryBUIs;
    public SecondaryBattleUserInterface[] secondaryBUIs;

    void Awake()
    {
        it = this;
    }

    public static void ForceResetAllBUIs()
    {
        for (int i = 0; i < it.primaryBUIs.Length; i++)
        {
            PrimaryBattleUserInterface x = it.primaryBUIs[i];
            if (x.State != UIState.DISABLED)
            {
                x.State = UIState.DISABLED;
            }
        }

        for (int i = 0; i < it.secondaryBUIs.Length; i++)
        {
            SecondaryBattleUserInterface x = it.secondaryBUIs[i];
            if (x.State != UIState.DISABLED)
            {
                x.State = UIState.DISABLED;
            }
        }
    }

    public static void EnablePrimaryBUI(BattleUIType type)
    {
        Battle.main.lastOpenUserInterface = type;
        it.primaryBUIs[(int)type].State = UIState.ENABLING;
    }

    public static void SetWorldRendering(bool enabled)
    {
        for (int i = 0; i < it.primaryBUIs.Length; i++)
        {
            it.primaryBUIs[i].SetWorldRendering(enabled);
        }

        for (int i = 0; i < it.secondaryBUIs.Length; i++)
        {
            it.secondaryBUIs[i].SetWorldRendering(enabled);
        }
    }
}
