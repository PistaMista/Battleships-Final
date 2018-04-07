﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;

using Gameplay;

namespace BattleUIAgents.Tokens
{
    public class Token : WorldBattleUIAgent
    {
        public static Token heldToken;
        public Effect effectType;
        public Effect effect;
        public float pickupRadius;
        public float occlusionRadius;
        public float height;
        bool stacked;
        public struct Stacking
        {
            public Vector3 stackStart;
            public Vector3 stackStep;
            public Stacking(Vector3 stackStart, Vector3 stackStep)
            {
                this.stackStart = stackStart;
                this.stackStep = stackStep;
            }
        }
        Stacking stackingMethod;
        public Stacking stacking
        {
            set
            {
                stackingMethod = value;
                stackingMethod.stackStep.y = height;
                if (effect == null)
                {
                    MoveToStack();
                }
            }
            get
            {
                return stackingMethod;
            }
        }

        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            Delinker += () => { if (effect != null) { Effect.RemoveFromQueue(effect); effect = null; }; stacked = false; };

            MoveToStack();
        }

        protected Vector3 GetPositionWhenEffectless()
        {
            float blockers = FindAgents(x =>
            {
                if (x.linked)
                {
                    Token token = (Token)x;
                    return token != this && token.effect == null && token.effectType == effectType && token.stacked;
                }
                return false;
            }, typeof(Token), int.MaxValue
            ).Length;

            return stacking.stackStart + stacking.stackStep * blockers;
        }

        public bool TryPickup(Vector3 position)
        {
            if (!interactable) return false;

            Vector2 planarInput = new Vector2(position.x, position.z);

            Utilities.PerspectiveProjection scaleInfo = Utilities.GetPositionOnElevationFromPerspective(hookedPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight);
            float planarDistance = Vector2.Distance(scaleInfo.planarPosition, planarInput);

            if (heldToken == null && planarDistance < pickupRadius * scaleInfo.scalar)
            {
                if (FindAgent(x =>
                {
                    Token c = (Token)x;
                    Utilities.PerspectiveProjection candidateScaleInfo = Utilities.GetPositionOnElevationFromPerspective(c.hookedPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight);
                    float planarCandidateDistance = Vector2.Distance(planarInput, candidateScaleInfo.planarPosition);
                    return planarCandidateDistance < c.occlusionRadius * scaleInfo.scalar && ((c.transform.position.y > transform.position.y) || (Mathf.Approximately(c.transform.position.y, transform.position.y) && planarCandidateDistance < planarDistance));
                }, typeof(Token)) == null)
                {
                    Pickup();
                    return true;
                }
            }

            return false;
        }

        protected virtual void Pickup()
        {
            stacked = false;
            heldToken = this;
            if (effect != null)
            {
                Effect.RemoveFromQueue(effect);
                effect = null;
            }
        }

        public virtual void ProcessExternalInputWhileHeld(Vector3 inputPosition)
        {

        }

        public virtual void Drop()
        {
            heldToken = null;

            if (effect != null)
            {
                transform.SetAsLastSibling();
                if (!Battle.main.effects.Contains(effect))
                {
                    Effect.AddToQueue(effect);
                }
            }
            else
            {
                transform.SetAsFirstSibling();
                MoveToStack();
            }
        }

        public void MoveToStack()
        {
            hookedPosition = GetPositionWhenEffectless();
            stacked = true;
        }

        protected virtual Effect CalculateEffect()
        {
            return null;
        }

        public static Token[] FindTokens(bool linked, bool used, Type effectType, int limit)
        {
            return Array.ConvertAll(FindAgents(x =>
            {
                Token token = x as Token;
                return token.linked == linked && (token.effect != null) == used && token.effectType.GetType() == effectType;
            }, typeof(Token), limit), x => { return x as Token; });
        }

        public static void SetTypeStacking(Type tokenEffectType, Vector3 stackStart, Vector3 stackStep)
        {
            Token[] tokens = Array.ConvertAll(FindAgents(x =>
            {
                Token token = x as Token;
                return token.effectType.GetType() == tokenEffectType;
            }, typeof(Token), int.MaxValue), x => { return x as Token; });

            Array.ForEach(tokens, x => { x.stacked = false; });
            Array.ForEach(tokens, x => { x.stacking = new Stacking(stackStart, stackStep); });
        }
    }
}