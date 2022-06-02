using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;


    [SerializeField]
    private TextMeshPro _text;
    [SerializeField] 
    private SpriteRenderer _renderer;

    public Vector2 Pos => transform.position;
    public void Init(BlockType type)
    {
        Value = type.Value;
        Color newCol;
        if (ColorUtility.TryParseHtmlString(type.Color, out newCol))
            _renderer.color = newCol;
        _text.text = type.Value.ToString();

    }

    public void SetBlock(Node node)
    {
        if (Node != null) Node.OccupiedBlock = null;
        Node = node;
        Node.OccupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        // set block we are merging with
        MergingBlock = blockToMergeWith;

        //Set current node as unoccupied to allow blocks to use it
        Node.OccupiedBlock = null;
        // set base block to merging as not used twice
        blockToMergeWith.Merging = true;

    }

    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null; 

}
