using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;
    public Vector3 Size => transform.localScale;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshPro _text;
    
    public void Init(BlockType type)
    {
        Value = type.Value;
        _spriteRenderer.color = type.Color;
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
        //set the block we're merging with
        MergingBlock = blockToMergeWith;
        
        //set current node as null for others block to use it
        Node.OccupiedBlock = null;

        //set the base block as merging so it doesnt get used twice
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;
}
