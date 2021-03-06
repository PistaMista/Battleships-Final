﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;

using Gameplay;

public class MiscellaneousVariables : MonoBehaviour
{
    public int[] boardSizes;
    public GameObject playerPrefab;
    public GameObject[] shipPrefabs;
    public Effect[] effectPrefabs;
    public GameObject[] defaultShipLoadout;
    public float boardTileSideLength;
    public float boardUIRenderHeight;
    public float boardCameraHeightModifier;
    public float boardDistanceFromCenter;
    public float flagVoxelScale;
    public float flagRenderHeight;
    public Vector2Int flagResolution;
    public int maximumTorpedoAttacksPerTurn;
    public TitleUI.Title titleUI;
    public static MiscellaneousVariables it;

    void Start()
    {
        it = this;
        if (boardSizes.Length != 3)
        {
            Debug.LogError("Board size array is not the correct size!");
        }
    }


}
