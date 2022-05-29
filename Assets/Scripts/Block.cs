using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int Value;
    [SerializeField]
    private TextMeshPro _text;
    [SerializeField] 
    private SpriteRenderer _renderer;

    public void Init(BlockType type)
    {
        Value = type.Value;
        _renderer.color = type.Color;
        _text.text = type.Value.ToString();
    }
}
