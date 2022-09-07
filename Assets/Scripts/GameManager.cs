using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region Variables
    
    [BoxGroup("Grid")] [SerializeField] private int _width = 4;
    [BoxGroup("Grid")] [SerializeField] private int _height = 4;
    [BoxGroup("Grid")] [EnumToggleButtons] [SerializeField] private WinCondition _winConditionType;
    private bool _isWinTypeShieldValue => _winConditionType == WinCondition.ShieldValue;
    private bool _isWinTypePoints => _winConditionType == WinCondition.PointsValue;
    [ShowIf("_isWinTypeShieldValue")] [BoxGroup("Grid")] [SerializeField] private int _winShieldCondition = 2048;
    [ShowIf("_isWinTypePoints")] [BoxGroup("Grid")] [SerializeField] private int _winPointsCondition = 10000;
    [BoxGroup("Grid")] [SerializeField] private int _pointsToActivateDoubleBonus = 1000;

    [BoxGroup("Blocks")] [SerializeField] private List<BlockType> _types;
    [BoxGroup("Blocks")] [SerializeField] [Range(0,1)] private float _travelTime = .5f;

    [Title("Main Game")]
    [BoxGroup("References")] [SerializeField] private Node _nodePrefab;
    [BoxGroup("References")] [SerializeField] private Block _blockPrefab;
    [BoxGroup("References")] [SerializeField] private SpriteRenderer _boardPrefabSpriteRenderer;
    [BoxGroup("References")] [SerializeField] private SpriteRenderer _selectorPrefabSpriteRenderer;
    [BoxGroup("References")] [SerializeField] private EndGameUIController _endGameUIController;
    [Title("Points & Combo")]
    [BoxGroup("References")] [SerializeField] private ComboController _comboController;
    [BoxGroup("References")] [SerializeField] private PointsController _pointsController;
    [BoxGroup("References")] [SerializeField] private Transform _pointControllerTransform;
    [BoxGroup("References")] [SerializeField] private PointEffectController _pointsEffectPrefab;
    [Title("Bonus")]
    [BoxGroup("References")] [SerializeField] private Image _bonusPrefabImage;
    [BoxGroup("References")] [SerializeField] private Image _bonusPrefabKeyImage;
    [BoxGroup("References")] [SerializeField] private GameObject _bonusEscapeUI;

    [BoxGroup("Debug")] [SerializeField] [ReadOnly] private GameState _state;
    [BoxGroup("Debug")] [SerializeField] [ReadOnly] private int _round;
    [BoxGroup("Debug")] [SerializeField] [ReadOnly] private int _freeNodesCount = 0;

    private List<Node> _nodeList;
    private List<Block> _blockList;

    private Transform _selectorTransform = null;
    
    private int _currentPointToActivateDoubleBonus = 0;

    private BlockType _getBlockTypeByValue(int value) => _types.First(t => t.Value == value);

    private UnityEvent<int,int> _onRoundEnded = new UnityEvent<int,int>();
    private UnityEvent<int> _onPointsScored = new UnityEvent<int>();
    
    #endregion

    private void Start()
    {
        ChangeState(GameState.GenerateGrid);
        
        _onRoundEnded.AddListener(ComboControl);
        _onRoundEnded.AddListener(PointsControl);
        
        _onPointsScored.AddListener(AddPointsToDoubleBonus);
        ResetDoubleBonusToZero();
    }

    private void OnDestroy()
    {
        _onRoundEnded.RemoveListener(ComboControl);
        _onRoundEnded.RemoveListener(PointsControl);
        
        _onPointsScored.RemoveListener(AddPointsToDoubleBonus);
    }

    #region GameLoop
    
    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.GenerateGrid:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(_round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Selecting:
                DoubleBonusSelectorActivation();
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                _endGameUIController.LaunchEndGameUI(true);
                break;
            case GameState.Lose:
                _endGameUIController.LaunchEndGameUI(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void Update()
    {
        InputCheck();
    }

    private void InputCheck()
    {
        if (_state != GameState.WaitingInput) return;

        if (_selectorTransform == null)
        {
            //bonus selector activation
            if (Input.GetKeyDown(KeyCode.A)) ChangeState(GameState.Selecting);

            //grid move
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
            if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        }
        else //if there is a selector bonus activated
        {
            //selector move
            if (Input.GetKeyDown(KeyCode.LeftArrow)) DoubleBonusSelectorMove(Vector2.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) DoubleBonusSelectorMove(Vector2.right);
            if (Input.GetKeyDown(KeyCode.DownArrow)) DoubleBonusSelectorMove(Vector2.down);
            if (Input.GetKeyDown(KeyCode.UpArrow)) DoubleBonusSelectorMove(Vector2.up);

            //selector choice
            var selectorPositionNode = GetNodeAtPosition(_selectorTransform.position);
            if (Input.GetKeyDown(KeyCode.A)) DoubleBonusSelectorSelection(selectorPositionNode);

            //quit selector
            if (Input.GetKeyDown(KeyCode.E)) DoubleBonusSelectorDeactivation();
        }
    }

    private void GenerateGrid()
    {
        _round = 0;
        
        //initiate nodes and blocks lists
        _nodeList = new List<Node>();
        _blockList = new List<Block>();
        
        //tiles creation
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodeList.Add(node);
            }
        }

        //board creation
        var center = new Vector2((float)_width / 2 - 0.5f,(float)_height / 2 - 0.5f);
        var board = Instantiate(_boardPrefabSpriteRenderer, center, Quaternion.identity);
        board.size = new Vector2(_width, _height);
        
        //camera centering
        if (Camera.main != null) Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        //first spawn
        ChangeState(GameState.SpawningBlocks);
    }

    private void SpawnBlocks(int amount)
    {
        //get a random list of free nodes
        var freeNodes = GetFreeNodesList(_nodeList);
            
        //spawn blocks
        foreach (var node in freeNodes.Take(amount))
        {
            SpawnBlock(node,Random.value > 0.8f ? 4 : 2, 0);
        }

        //check if the game's lost
        _freeNodesCount = freeNodes.Count();
        if (_freeNodesCount==0)
        {
            ChangeState(GameState.Lose);
            return;
        }
        
        // enum that depend on the win condition type
        switch (_winConditionType)
        {
            case WinCondition.ShieldValue:
                ChangeState(_blockList.Any(b=>b.Value>=_winShieldCondition) ? GameState.Win : GameState.WaitingInput);
                break;
            case WinCondition.PointsValue:
                ChangeState(_pointsController.ValueNumber >= _winPointsCondition ? GameState.Win : GameState.WaitingInput);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    IOrderedEnumerable<Node> GetFreeNodesList(List<Node> nodeList)
    {
        return nodeList
            .Where(n => n.OccupiedBlock == null)
            .OrderBy(i => Random.value);
    }

    private void SpawnBlock(Node node, int value, int AnimationNumber)
    {
        var block = Instantiate(_blockPrefab,node.Pos, Quaternion.identity);
        block.Init(_getBlockTypeByValue(value));
        block.SetBlock(node);
        _blockList.Add(block);

        //anim
        switch (AnimationNumber)
        {
            case 0 : //nothing
                const float blockSpawnAnimationSpeed = .25f;
                var endScale = block.Size;
                block.transform.localScale = Vector3.zero;
                block.transform.DOScale(endScale, blockSpawnAnimationSpeed);
                break;
            case 1 : //merge
                var blockPunchEffect1Scale = new Vector3(0.5f,0.5f,0.5f);
                const float blockPunchEffect1Speed = .25f;
                block.transform.DOPunchScale(blockPunchEffect1Scale, blockPunchEffect1Speed);
                block.transform.DORotate(new Vector3(0, 0, 360), blockPunchEffect1Speed);
                break;
            case 2 : //double bonus
                var blockPunchEffect2Scale = new Vector3(1f,1f,1f);
                const float blockPunchEffect2Speed = .2f;
                block.transform.DOPunchScale(blockPunchEffect2Scale, blockPunchEffect2Speed);
                block.transform.DORotate(new Vector3(0, 0, 360), blockPunchEffect2Speed);
                break;
        }
    }

    private void Shift(Vector2 direction)
    {
        ChangeState(GameState.Moving);
        
        var orderedBlocks = _blockList.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (direction == Vector2.right || direction == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var nextNode = block.Node;
            do {
               block.SetBlock(nextNode);

               var possibleNode = GetNodeAtPosition(nextNode.Pos + direction);
               if (possibleNode != null) {
                   //we know a node is present
                   //if it's possible to merge, set merge
                   if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                   {
                       block.MergingBlock = possibleNode.OccupiedBlock;
                   }
                   //otherwise can we move to this spot
                   else if (possibleNode.OccupiedBlock == null) nextNode = possibleNode;
                   
                   //None hit ? End do while loop 
               }
                
            } while (nextNode != block.Node); //while we set a next block different than the actual one
        }

        var sequence = DOTween.Sequence();
        
        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;
            sequence.Insert(0,block.transform.DOMove(movePoint, _travelTime));
        }

        int numberOfBlockMerged = 0, totalValueofBlockMerged = 0;
        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b=>b.MergingBlock!=null))
            {
                MergeBlocks(block.MergingBlock, block);
                //combo managing if a block has been merged
                numberOfBlockMerged++;
                totalValueofBlockMerged += block.Value + block.MergingBlock.Value;
            }
            _onRoundEnded.Invoke(numberOfBlockMerged,totalValueofBlockMerged);
            ChangeState(GameState.SpawningBlocks);
        });
    }

    private void MergeBlocks(Block baseBlock, Block mergingBlock)
    {

        SpawnBlock(baseBlock.Node, baseBlock.Value * 2, 1);
        MakePointEffect(baseBlock.Pos);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    private void RemoveBlock(Block block)
    {
        _blockList.Remove(block);
        Destroy(block.gameObject);
    }

    private Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodeList.FirstOrDefault(n => n.Pos == pos);
    }

    #endregion

    #region Bonuses

    // X2 SELECTOR
    private void AddPointsToDoubleBonus(int points)
    {
        bool isBonusAlreadyAvailable = _bonusPrefabImage.fillAmount >= 1;
            
        //points controller
        _currentPointToActivateDoubleBonus += points;
        if (_currentPointToActivateDoubleBonus > _pointsToActivateDoubleBonus)
            _currentPointToActivateDoubleBonus = _pointsToActivateDoubleBonus;
        
        //sprite and effects
        var fillPercentage = (float)_currentPointToActivateDoubleBonus / (float)_pointsToActivateDoubleBonus;
        _bonusPrefabImage.fillAmount = fillPercentage;
        
        //effect when complete
        if (!isBonusAlreadyAvailable && fillPercentage >= 1)
        {
            _bonusPrefabImage.transform.DOComplete();
            float effectSpeed = .2f;
            Vector3 punchEffectForce = new Vector3(1.5f,1.5f,1.5f);
            _bonusPrefabImage.transform.DOPunchScale(punchEffectForce, effectSpeed);
            
            _bonusPrefabKeyImage.gameObject.SetActive(true);
        }
    }

    private void ResetDoubleBonusToZero()
    {
        const float effectSpeed = .5f;
        _currentPointToActivateDoubleBonus = 0;
        _bonusPrefabImage.DOFillAmount(0,effectSpeed);
        _bonusPrefabKeyImage.gameObject.SetActive(false);
        _bonusEscapeUI.SetActive(false);
    }
    
    private void DoubleBonusSelectorActivation()
    {
        //guard if there isn't enough point to activate bonus
        if (_currentPointToActivateDoubleBonus < _pointsToActivateDoubleBonus)
        {
            //effect
            
            ChangeState(GameState.WaitingInput);
            return;
        }
        
        _bonusEscapeUI.SetActive(true);
        
        var maxValueBlockPos = _blockList.OrderByDescending(n => n.Value).FirstOrDefault()!.Pos;
        var selector = Instantiate(_selectorPrefabSpriteRenderer, maxValueBlockPos, Quaternion.identity);
        _selectorTransform = selector.transform;
        _selectorTransform.localScale = Vector2.zero;
        const float selectorScaleSpeed = .2f;
        _selectorTransform.DOScale(new Vector2(.5f,.5f), selectorScaleSpeed);
        
        ChangeState(GameState.WaitingInput);
    }

    private void DoubleBonusSelectorDeactivation()
    {
        _bonusEscapeUI.SetActive(false);

        _selectorTransform.DOComplete();
        const float selectorScaleSpeed = .2f;
        _selectorTransform.DOScale(0, selectorScaleSpeed).OnComplete(ResetSelectorTransformVariable);

        ChangeState(GameState.WaitingInput);
    }

    private void ResetSelectorTransformVariable()
    {
        //reset selectorTransform to null
        Destroy(_selectorTransform.gameObject);
        _selectorTransform = null;
    }

    private void DoubleBonusSelectorMove(Vector2 direction)
    {
        const float selectorScaleMoveSpeed = .2f;
        _selectorTransform.DOComplete();
        var nextSelectorPos = (Vector2)_selectorTransform.position + direction;
        var nextSelectorPosNode = GetNodeAtPosition(nextSelectorPos);
        if (nextSelectorPosNode != null)
            _selectorTransform.DOMove(nextSelectorPos, selectorScaleMoveSpeed);
        
        ChangeState(GameState.WaitingInput);
    }

    private void DoubleBonusSelectorSelection(Node node)
    {
        //guard if node null
        if (node == null) return;
        //guard if node is not occupied
        if (node.OccupiedBlock == null) return;
        
        //do the bonus
        var block = node.OccupiedBlock;
        SpawnBlock(node,block.Value*2, 2);
        RemoveBlock(block);

        ResetSelectorTransformVariable();

        ResetDoubleBonusToZero();
        
        _bonusEscapeUI.SetActive(false);

        ChangeState(GameState.SpawningBlocks);
    }

    #endregion

    #region Combo

    private void ComboControl(int numberOfMerges, int valueOfMerges)
    {
        switch (numberOfMerges)
        {
            //if there is no merge = combo stop
            case 0:
                _comboController.ControllerResetToZero();
                break;
            //if there there is merges = combo
            case var value when numberOfMerges > 0:
                _comboController.ControllerUpdate(numberOfMerges);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Points

    private void PointsControl(int numberOfMerges, int valueOfMerges)
    {
        int valueOfPointsWithCombo = valueOfMerges * _comboController.ValueNumber;
        _pointsController.ControllerUpdate(valueOfPointsWithCombo);
        _onPointsScored.Invoke(valueOfPointsWithCombo);
    }

    private void MakePointEffect(Vector2 position)
    {
        var pointEffect = Instantiate(_pointsEffectPrefab, position, Quaternion.identity);
        pointEffect._pointControllerPosition = _pointControllerTransform.position;
        pointEffect.StartPointAnimation();
    }

    [BoxGroup("Debug")]
    [Button]
    private void AddOneHundredPoints()
    {
        PointsControl(1, 100);
    }

    #endregion
}

[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}

public enum GameState
{
    GenerateGrid,
    SpawningBlocks,
    WaitingInput,
    Selecting,
    Moving,
    Win,
    Lose
}

public enum WinCondition
{
    ShieldValue,
    PointsValue
}