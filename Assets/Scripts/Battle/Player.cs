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
            public float[,,] flag;
            public static implicit operator PlayerData(Player player)
            {
                PlayerData result = new PlayerData();
                result.index = player.index;
                result.board = player.board;
                result.aiEnabled = player.aiEnabled;
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
        public Color[,] flag;
        public AmmoRegistry arsenal
        {
            get
            {
                return Battle.main.effects.Find(x => x is AmmoRegistry && x.targetedPlayer == this) as AmmoRegistry;
            }
        }

        public void Initialize(PlayerData data)
        {
            index = data.index;
            board = new GameObject("Board").AddComponent<Board>();
            board.transform.SetParent(transform);
            board.Initialize(data.board);

            aiEnabled = data.aiEnabled;

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