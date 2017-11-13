﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board : MonoBehaviour
{
    [Serializable]
    public struct BoardData
    {
        public bool ownedByAttacker;
        public Tile.TileData[,] tiles;
        public static implicit operator BoardData(Board board)
        {
            BoardData result = new BoardData();
            result.ownedByAttacker = board.owner == Battle.main.attacker;
            result.tiles = new Tile.TileData[board.tiles.GetLength(0), board.tiles.GetLength(1)];
            for (int x = 0; x < result.tiles.GetLength(0); x++)
            {
                for (int y = 0; y < result.tiles.GetLength(1); y++)
                {
                    result.tiles[x, y] = board.tiles[x, y];
                }
            }
            return result;
        }
    }
    public Player owner;
    public Tile[,] tiles;

    public void Initialize(BoardData data)
    {
        //owner - REF
        tiles = new Tile[data.tiles.GetLength(0), data.tiles.GetLength(1)];

        Vector3 startingPosition = new Vector3(-tiles.GetLength(0) / 2.0f + 0.5f, 0, -tiles.GetLength(1) / 2.0f + 0.5f);
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y] = new GameObject("Tile X:" + x + " Y:" + y).AddComponent<Tile>();
                tiles[x, y].transform.SetParent(transform);
                tiles[x, y].transform.localPosition = startingPosition + new Vector3(x, 0, y);
                tiles[x, y].Initialize(data.tiles[x, y]);
            }
        }
    }

    public void AssignReferences(BoardData data)
    {
        owner = data.ownedByAttacker ? Battle.main.attacker : Battle.main.defender;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y].AssignReferences(data.tiles[x, y]);
            }
        }
    }
}
