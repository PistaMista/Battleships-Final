﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using BattleUIAgents.Agents;
using BattleUIAgents.Base;

namespace BattleUIAgents.UI
{
    public class Overview : ScreenBattleUIAgent
    {
        Flag[] flags;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            flags = Array.ConvertAll(LinkAgents(FindAgents(x => { return x is Flag && x.player != null; }, 2)), item => { return (Flag)item; });
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                for (int i = 0; i < flags.Length; i++)
                {
                    if (flags[i].IsPositionOnFlag(currentInputPosition.world))
                    {
                        gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + MiscellaneousVariables.it.boardDistanceFromCenter * Vector2.right * 2.0f;
        }



        protected override void Reset()
        {
            flags = null;
        }
    }
}