using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private List<BlockType> _types;

    private List<Block> _blocks;
    private List<Node> _nodes;

    private BlockType GetBlockTypeByValue(int value) => _types.First(t => t.Value == value);

    public void Start()
    {
        GenerateGrid();
    }
    public void GenerateGrid()
    {
        _blocks = new List<Block>();
        _nodes = new List<Node>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node);
            }
        }
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        var background = Instantiate(_boardPrefab, center, Quaternion.identity);
        background.size = new Vector2(_width, _height);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        SpawnBlocks(2);
    }

    public void SpawnBlocks(int amount)
    {
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        foreach (var node in freeNodes.Take(amount))
        {
            var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(amount));
            _blocks.Add(block);
        }

        if (freeNodes.Count() == 1)
        {
            //lost game
            return;
        }
    }

}

[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}
