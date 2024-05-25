using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets.Scripts.RL
{
    public class SmartSnake : Agent
    {
        private Vector2Int _gridPosition;
        private Vector2Int _gridMoveDirection;
        private float _gridMoveTimer;
        [SerializeField] private float _gridMoveTimerMax = 0.1f;
        private SmartLevelGrid _levelGrid;
        private List<Transform> _snakeBodyTransformList;
        [SerializeField] private bool _useRL = true;

        public void Setup(SmartLevelGrid levelGrid)
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

        public override void OnEpisodeBegin()
        {
            ClearSnakeBody();
            _gridPosition = new Vector2Int(Random.Range(-_levelGrid.GetWidth(), _levelGrid.GetWidth()), Random.Range(-_levelGrid.GetHeight(), _levelGrid.GetHeight()));
            int direction = Random.Range(0, 2);
            if (direction == 0)
            {
                _gridMoveDirection = new Vector2Int(Random.Range(-1, 2), 0);
            }
            else
            {
                _gridMoveDirection = new Vector2Int(0, Random.Range(-1, 2));
            }
            _levelGrid.SpawnFood();
        }

        private void ClearSnakeBody()
        {
            foreach (var bodyPart in _snakeBodyTransformList)
            {
                Destroy(bodyPart.gameObject);
            }
            _snakeBodyTransformList.Clear();
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int action = actionBuffers.DiscreteActions[0];

            switch (action)
            {
                case 0: // move up
                    if (_gridMoveDirection.y != -1)
                        _gridMoveDirection = new Vector2Int(0, 1);
                    break;
                case 1: // move down
                    if (_gridMoveDirection.y != 1)
                        _gridMoveDirection = new Vector2Int(0, -1);
                    break;
                case 2: // move left
                    if (_gridMoveDirection.x != 1)
                        _gridMoveDirection = new Vector2Int(-1, 0);
                    break;
                case 3: // move right
                    if (_gridMoveDirection.x != -1)
                        _gridMoveDirection = new Vector2Int(1, 0);
                    break;
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut.Clear();

            if (Input.GetKey(KeyCode.UpArrow))
                discreteActionsOut[0] = 0;
            else if (Input.GetKey(KeyCode.DownArrow))
                discreteActionsOut[0] = 1;
            else if (Input.GetKey(KeyCode.LeftArrow))
                discreteActionsOut[0] = 2;
            else if (Input.GetKey(KeyCode.RightArrow))
                discreteActionsOut[0] = 3;
            else
                discreteActionsOut[0] = -1; // default value
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            Vector2 normalizedFoodPosition = (_levelGrid.GetFoodPosition() + new Vector2(_levelGrid.GetWidth(), _levelGrid.GetHeight())) / new Vector2(_levelGrid.GetWidth() * 2, _levelGrid.GetHeight() * 2);
            sensor.AddObservation(normalizedFoodPosition);

            Vector2 normalizedHeadPosition = (_gridPosition + new Vector2(_levelGrid.GetWidth(), _levelGrid.GetHeight())) / new Vector2(_levelGrid.GetWidth() * 2, _levelGrid.GetHeight() * 2);
            sensor.AddObservation(normalizedHeadPosition);

            float distanceToFood = Vector2.Distance(normalizedHeadPosition, normalizedFoodPosition);
            sensor.AddObservation(distanceToFood);

            sensor.AddObservation(IsObstacleInDirection(new Vector2Int(0, 1)));  // Up
            sensor.AddObservation(IsObstacleInDirection(new Vector2Int(0, -1))); // Down
            sensor.AddObservation(IsObstacleInDirection(new Vector2Int(-1, 0))); // Left
            sensor.AddObservation(IsObstacleInDirection(new Vector2Int(1, 0)));  // Right

            float distanceToNearestWall = CalculateDistanceToNearestWall();
            float normalizedDistanceToNearestWall = distanceToNearestWall / (_levelGrid.GetWidth() + _levelGrid.GetHeight());
            sensor.AddObservation(normalizedDistanceToNearestWall);
        }

        private bool IsObstacleInDirection(Vector2Int direction)
        {
            Vector2Int nextPosition = _gridPosition + direction;

            // wall
            if (nextPosition.x < -_levelGrid.GetWidth() || nextPosition.y < -_levelGrid.GetHeight() || nextPosition.x > _levelGrid.GetWidth() || nextPosition.y > _levelGrid.GetHeight())
            {
                return true;
            }

            // snake body
            foreach (Transform bodyPart in _snakeBodyTransformList)
            {
                if (Vector2Int.RoundToInt(bodyPart.position) == nextPosition)
                {
                    return true;
                }
            }

            return false;
        }

        private float CalculateDistanceToNearestWall()
        {
            float distanceToLeftWall = _gridPosition.x + _levelGrid.GetWidth();
            float distanceToRightWall = _levelGrid.GetWidth() - _gridPosition.x;
            float distanceToTopWall = _levelGrid.GetHeight() - _gridPosition.y;
            float distanceToBottomWall = _gridPosition.y + _levelGrid.GetHeight();

            return Mathf.Min(distanceToLeftWall, distanceToRightWall, distanceToTopWall, distanceToBottomWall);
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        // private void Update()
        // {
        //     HandleMovement();
        // }

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

        private void UpdateMovement() {
            _gridPosition += _gridMoveDirection;

            if (_gridPosition.x < -_levelGrid.GetWidth() || _gridPosition.x > _levelGrid.GetWidth() ||
                _gridPosition.y < -_levelGrid.GetHeight() || _gridPosition.y > _levelGrid.GetHeight())
            {
                AddReward(-1f);
                EndEpisode();
                return;
            }

            if (IsPositionInBody(_gridPosition))
            {
                AddReward(-1f);
                EndEpisode();
                return;
            }

            if (_levelGrid.TrySnakeEatFood(_gridPosition))
            {
                AddReward(1f);
                Grow();
            } 
            // else {
            //     AddReward(-0.1f);
            // }

            UpdateBodyPositions();
            transform.position = new Vector3(_gridPosition.x, _gridPosition.y);
        }

        private void HandleMovement()
        {
            if (_useRL)
            {
                UpdateMovement();
            }
            else
            {
                _gridMoveTimer += Time.deltaTime;
                if (_gridMoveTimer >= _gridMoveTimerMax) {
                    _gridMoveTimer -= _gridMoveTimerMax;
                    UpdateMovement();
                }
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