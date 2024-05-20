using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Snake : MonoBehaviour
    {
        private Vector2Int _gridPosition;
        private Vector2Int _gridMoveDirection;
        private float _gridMoveTimer;
        [SerializeField] private float _gridMoveTimerMax = 0.1f;
        private LevelGrid _levelGrid;
        private List<Transform> _snakeBodyTransformList;

        public void Setup(LevelGrid levelGrid)
        {
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
            HandleInput();
            HandleMovement();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_gridMoveDirection.y != -1)
                {
                    _gridMoveDirection.x = 0;
                    _gridMoveDirection.y = 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_gridMoveDirection.y != 1)
                {
                    _gridMoveDirection.x = 0;
                    _gridMoveDirection.y = -1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (_gridMoveDirection.x != 1)
                {
                    _gridMoveDirection.x = -1;
                    _gridMoveDirection.y = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (_gridMoveDirection.x != -1)
                {
                    _gridMoveDirection.x = 1;
                    _gridMoveDirection.y = 0;
                }
            }
        }

        private void HandleMovement()
        {
            _gridMoveTimer += Time.deltaTime;
            if (_gridMoveTimer >= _gridMoveTimerMax)
            {
                _gridMoveTimer -= _gridMoveTimerMax;
                _gridPosition += _gridMoveDirection;

                // Validate the grid position within the range of -width to width and -height to height
                _gridPosition = new Vector2Int(
                    Mathf.Clamp(_gridPosition.x, -_levelGrid.GetWidth(), _levelGrid.GetWidth()),
                    Mathf.Clamp(_gridPosition.y, -_levelGrid.GetHeight(), _levelGrid.GetHeight())
                );

                if (_levelGrid.TrySnakeEatFood(_gridPosition))
                {
                    Grow();
                }

                UpdateBodyPositions();
                transform.position = new Vector3(_gridPosition.x, _gridPosition.y);
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

        private void UpdateBodyPositions()
        {
            if (_snakeBodyTransformList.Count == 0) return;

            // Move last body part to where the head was
            for (var i = _snakeBodyTransformList.Count - 1; i > 0; i--)
            {
                _snakeBodyTransformList[i].position = _snakeBodyTransformList[i - 1].position;
            }

            // First body part moves to where the head was
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
}