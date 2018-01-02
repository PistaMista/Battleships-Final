﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryTTUI : TTUI
{
    protected override void DropHeldToken()
    {
        base.DropHeldToken();
        if (heldToken.value == null && placedTokens.Count == 0)
        {
            attackViewUserInterface.activePrimaryTargeter = null;
        }
        else
        {
            attackViewUserInterface.activePrimaryTargeter = this;
        }
    }

    public override bool IsSelectable()
    {
        return base.IsSelectable() && (attackViewUserInterface.activePrimaryTargeter == null || attackViewUserInterface.activePrimaryTargeter == this);
    }

    public void ConfirmTargeting()
    {
        attackViewUserInterface.activePrimaryTargeter = null;
        attackViewUserInterface.State = UIState.DISABLING;
        ConfirmAttack();
        BattleUIMaster.EnablePrimaryBUI(BattleUIType.CINEMATIC_VIEW);
    }

    protected virtual void ConfirmAttack()
    {

    }
}
