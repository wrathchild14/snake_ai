using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamiltonSnake : MonoBehaviour
{
    private Vector2Int _gridPosition;
    private Vector2Int _gridMoveDirection;
    private float _gridMoveTimer;
    [SerializeField] private float _gridMoveTimerMax = 0.1f;
    private LevelGridHamilton _levelGrid;
    private List<Transform> _snakeBodyTransformList;
    [SerializeField] private List<Vector2> _path;
    private int _current_waypoint;
    private Vector2 _targetWaypoint;

    public void Setup(LevelGridHamilton levelGrid)
    {
        _current_waypoint = 0;
        _levelGrid = levelGrid;
        _gridPosition = new Vector2Int(Random.Range(-levelGrid.GetWidth(), levelGrid.GetWidth()), Random.Range(-levelGrid.GetHeight(), levelGrid.GetHeight()));
    }

    private void Awake()
    {


        _gridMoveTimer = _gridMoveTimerMax;
        _gridMoveDirection = new Vector2Int(1, 0);

        _snakeBodyTransformList = new List<Transform>();
    }

    private void Update()
    {
        if (_current_waypoint < this._path.Count)
        {
            if (_targetWaypoint == null)
            {
                _targetWaypoint = _path[_current_waypoint];
            }
            this.transform.position = Vector2.MoveTowards(this.transform.position, _targetWaypoint, _gridMoveTimer);

        }
        else
        {
            _current_waypoint = 0;
            if (_targetWaypoint == null)
            {
                _targetWaypoint = _path[_current_waypoint];
            }
            this.transform.position = Vector2.MoveTowards(this.transform.position, _targetWaypoint, _gridMoveTimer);
        }
        
        if (this.transform.position.x == _targetWaypoint.x && this.transform.position.y == _targetWaypoint.y)
        {
            _current_waypoint++;
            _targetWaypoint = _path[_current_waypoint];
        }
        HandleMovement();
        //HandleInput();
        //HandleMovement();
    }

    private void HandleInput()
    {

    }

    private void HandleMovement()
    {
        _gridMoveTimer += Time.deltaTime;
        if (_gridMoveTimer >= _gridMoveTimerMax)
        {
            _gridMoveTimer -= _gridMoveTimerMax;
            _gridPosition += _gridMoveDirection;

            if (_gridPosition.x < -_levelGrid.GetWidth() || _gridPosition.x > _levelGrid.GetWidth() ||
               _gridPosition.y < -_levelGrid.GetHeight() || _gridPosition.y > _levelGrid.GetHeight())
            {
                Debug.Log("Game Over: Hit the wall");
                return;
            }

            if (IsPositionInBody(_gridPosition))
            {
                Debug.Log("Game Over: Ate itself");
                return;
            }

            if (_levelGrid.TrySnakeEatFood(_gridPosition))
            {
                Grow();
            }

            UpdateBodyPositions();
        }
    }

    private void Grow()
    {
        var snakeBodyGameObject = new GameObject("SnakeBody", typeof(SpriteRenderer));
        snakeBodyGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.instance.snakeBodySprite;
        snakeBodyGameObject.transform.localScale = this.transform.localScale;
        snakeBodyGameObject.transform.position = _snakeBodyTransformList.Count > 0 ? _snakeBodyTransformList[_snakeBodyTransformList.Count - 1].position : transform.position;
        _snakeBodyTransformList.Add(snakeBodyGameObject.transform);
    }

    private bool IsPositionInBody(Vector2Int position)
    {
        foreach (var bodyPart in _snakeBodyTransformList)
        {
            if (Vector2Int.RoundToInt(new Vector2(bodyPart.position.x, bodyPart.position.y)) == position)
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateBodyPositions()
    {
        if (_snakeBodyTransformList.Count == 0) return;

        // move last body part to where the head was
        for (var i = _snakeBodyTransformList.Count - 1; i > 0; i--)
        {
            _snakeBodyTransformList[i].position = _snakeBodyTransformList[i - 1].position;
        }

        // first body part moves to where the head was
        _snakeBodyTransformList[0].position = transform.position;
    }

    public List<Vector2Int> GetFullSnakePositionList()
    {
        var fullSnakePositionList = new List<Vector2Int> { _gridPosition };

        foreach (var snakeBodyTransform in _snakeBodyTransformList)
        {
            fullSnakePositionList.Add(new Vector2Int((int)snakeBodyTransform.position.x, (int)snakeBodyTransform.position.y));
        }

        return fullSnakePositionList;
    }
}

