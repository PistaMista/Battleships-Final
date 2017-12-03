﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleOverviewUserInterface : InputEnabledUserInterface
{
    public Waypoint cameraWaypoint;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                cameraWaypoint.transform.position = Vector3.up * CameraControl.CalculateCameraWaypointHeight(new Vector2(2 * MiscellaneousVariables.it.boardDistanceFromCenter + 20 * MiscellaneousVariables.it.flagVoxelScale, 0));
                cameraWaypoint.transform.LookAt(Vector3.zero);
                CameraControl.GoToWaypoint(cameraWaypoint, 1.2f);
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (tap)
        {
            Vector3 tapPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
            bool selectedAttacker = Mathf.Sign(tapPosition.x) == Mathf.Sign(Battle.main.attacker.transform.position.x);
            State = UIState.DISABLING;
            if (selectedAttacker)
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.ATTACKER_INFO);
            }
            else
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.ATTACK_VIEW);
            }
            Debug.Break();
        }
    }
}
