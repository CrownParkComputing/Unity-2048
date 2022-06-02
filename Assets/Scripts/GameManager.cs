using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using System.Drawing;
using Color = UnityEngine.Color;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;

    private List<BlockType> _types;
    private List<Block> _blocks;
    private List<Node> _nodes;
    private GameState _state;
    private int _round = 0;

    private BlockType GetBlockTypeByValue(int value) => _types.First(t => t.Value == value);

    public void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState)
    {
        _state = newState;
        switch(_state)
        {
            case GameState.GenerateLevel:
                GenerateTypes();
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(_round++ ==0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    public void GenerateTypes()
    {
        _types = new List<BlockType>();
        _types.Add(new BlockType() { Value = 2, Color = "#DF66BC" });
        _types.Add(new BlockType() { Value = 4, Color = "#3AB8CB" });
        _types.Add(new BlockType() { Value = 8, Color = "#3680DB" });
        _types.Add(new BlockType() { Value = 16, Color = "#E86947" });
        _types.Add(new BlockType() { Value = 32, Color = "#8B80FB" });
        _types.Add(new BlockType() { Value = 64, Color = "#968980" });
        _types.Add(new BlockType() { Value = 128, Color = "#FFAD29" });
        _types.Add(new BlockType() { Value = 256, Color = "#FE6472" });
        _types.Add(new BlockType() { Value = 512, Color = "#C42A09" });
        _types.Add(new BlockType() { Value = 1024, Color = "#60BC42" });
        _types.Add(new BlockType() { Value = 2048, Color = "#D33CEC" });

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
        
        ChangeState(GameState.SpawningBlocks);
    }

    public void SpawnBlocks(int amount)
    {
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        foreach (var node in freeNodes.Take(amount))
        {
            SpawnBlock(node, Random.value > 0.8f ? 4 : 2, false);
        }

        if (freeNodes.Count() == 1)
        {
            //lost game
            return;
        }

        ChangeState(GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value, bool Merging)
    {

        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        if (Merging) block.transform.DOPunchScale(new Vector3(1.5f, 1.5f, 1.5f), 1f,0,1f);
        block.SetBlock(node);
        _blocks.Add(block);
        
    }

    public void Update()
    {
        if (_state != GameState.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ShiftBlocks(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ShiftBlocks(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ShiftBlocks(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ShiftBlocks(Vector2.down);
        
    }

    void ShiftBlocks(Vector2 dir)
    {
        ChangeState(GameState.Moving);
        var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                if (possibleNode != null)
                {
                    //we know node is present
                    // if its possible to merge, we merge 
                    if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        block.MergeBlock(possibleNode.OccupiedBlock);
                    }
                    // otherwise can we move to spot
                    else if (possibleNode.OccupiedBlock == null) next = possibleNode;

                    // none hit end do while
                }

            } while (next != block.Node);

        }

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock !=null ? block.MergingBlock.Node.Pos : block.Node.Pos;
                sequence.Insert(0, block.transform.DOMove(movePoint, 0.2f));
        }

        sequence.OnComplete(() =>
        {
            
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null))
            {
                MergeBlocks(block.MergingBlock, block);
                             
            }

            ChangeState(GameState.SpawningBlocks);

        });


    
    }

    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        var newValue = baseBlock.Value * 2;
        SpawnBlock(baseBlock.Node, newValue, true);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
 
    }

    void RemoveBlock(Block block)
    {
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }
    Node GetNodeAtPosition(Vector2 pos)
    { return _nodes.FirstOrDefault(n => n.Pos == pos); }

}


public struct BlockType
{
    public int Value;
    public string Color;
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}

