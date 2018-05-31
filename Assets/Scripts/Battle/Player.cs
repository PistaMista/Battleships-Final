﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BattleUIAgents.Base;

using Gameplay.Effects;

namespace Gameplay
{
    public class Player : BattleBehaviour
    {
        [Serializable]
        public struct PlayerData
        {
            public int index;
            public Board.BoardData board;
            public bool aiEnabled;
            public Heatmap heatmap_recon;
            public float[,,] flag;
            public static implicit operator PlayerData(Player player)
            {
                PlayerData result = new PlayerData();
                result.index = player.index;
                result.board = player.board;
                result.aiEnabled = player.aiEnabled;
                result.heatmap_recon = player.heatmap_recon;
                result.flag = new float[player.flag.GetLength(0), player.flag.GetLength(1), 3];
                for (int x = 0; x < player.flag.GetLength(0); x++)
                {
                    for (int y = 0; y < player.flag.GetLength(1); y++)
                    {
                        Color color = player.flag[x, y];
                        result.flag[x, y, 0] = color.r;
                        result.flag[x, y, 1] = color.g;
                        result.flag[x, y, 2] = color.b;
                    }
                }



                return result;
            }
        }
        public int index;
        public Board board;
        public bool aiEnabled;
        public Heatmap heatmap_recon;
        public Color[,] flag;
        public AmmoRegistry arsenal
        {
            get
            {
                return Effect.GetEffectsInQueue(x => { return x.targetedPlayer == this; }, typeof(AmmoRegistry), 1)[0] as AmmoRegistry;
            }
        }

        public void Initialize(PlayerData data)
        {
            index = data.index;
            board = new GameObject("Board").AddComponent<Board>();
            board.transform.SetParent(transform);
            board.Initialize(data.board);

            aiEnabled = data.aiEnabled;
            heatmap_recon = data.heatmap_recon;

            flag = new Color[data.flag.GetLength(0), data.flag.GetLength(1)];
            for (int x = 0; x < flag.GetLength(0); x++)
            {
                for (int y = 0; y < flag.GetLength(1); y++)
                {
                    flag[x, y] = new Color(data.flag[x, y, 0], data.flag[x, y, 1], data.flag[x, y, 2]);
                }
            }

            //hitTiles - REF
        }



        public void AssignReferences(PlayerData data)
        {
            board.AssignReferences(data.board);
        }

        /// <summary>
        /// Executes every time a new turn starts.
        /// </summary>
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            AircraftRecon[] reconEffects = Array.ConvertAll(Effect.GetEffectsInQueue(x => x.targetedPlayer != this, typeof(AircraftRecon), int.MaxValue), x => x as AircraftRecon);

            for (int i = 0; i < reconEffects.Length; i++)
            {
                AircraftRecon line = reconEffects[i];

                int linePosition = (line.target % (Battle.main.defender.board.tiles.GetLength(0) - 1));
                bool lineVertical = line.target == linePosition;

                for (int x = lineVertical ? (line.result == 1 ? linePosition + 1 : 0) : 0; x < (lineVertical ? (line.result != 1 ? linePosition + 1 : heatmap_recon.tiles.GetLength(0)) : heatmap_recon.tiles.GetLength(0)); x++)
                {
                    for (int y = !lineVertical ? (line.result == 1 ? linePosition + 1 : 0) : 0; y < (lineVertical ? (line.result != 1 ? linePosition + 1 : heatmap_recon.tiles.GetLength(1)) : heatmap_recon.tiles.GetLength(1)); y++)
                    {
                        heatmap_recon.Heat(new Vector2Int(x, y), AI.reconChangeRate, 1);
                    }
                }
            }

            heatmap_recon = heatmap_recon.normalized;

            if (board.ships != null)
            {
                for (int i = 0; i < board.ships.Length; i++)
                {
                    board.ships[i].OnTurnStart();
                }
            }
        }

        /// <summary>
        /// Executes every time a game is loaded and the current turn is therefore resumed.
        /// </summary>
        public override void OnTurnResume()
        {
            base.OnTurnResume();
            if (board.ships != null)
            {
                for (int i = 0; i < board.ships.Length; i++)
                {
                    board.ships[i].OnTurnResume();
                }
            }
        }

        /// <summary>
        /// Executes every time a turn ends.
        /// </summary>
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (board.ships != null)
            {
                for (int i = 0; i < board.ships.Length; i++)
                {
                    board.ships[i].OnTurnEnd();
                }
            }
        }
    }
}