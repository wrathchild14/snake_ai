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
            _gridMoveDirection = new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
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
                    _gridMoveDirection = new Vector2Int(0, 1);
                    break;
                case 1: // move down
                    _gridMoveDirection = new Vector2Int(0, -1);
                    break;
                case 2: // move left
                    _gridMoveDirection = new Vector2Int(-1, 0);
                    break;
                case 3: // move right
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
            sensor.AddObservation(_levelGrid.GetFoodPosition());
            sensor.AddObservation(_gridPosition);
            sensor.AddObservation(_gridMoveDirection);

            for (var i = 0; i < 10; i++)
            {
                if (i < _snakeBodyTransformList.Count)
                {
                    sensor.AddObservation(_snakeBodyTransformList[i].position);
                }
                else
                {
                    sensor.AddObservation(Vector3.zero); // add a default value for the remaining body parts
                }
            }
        }

        private void Update()
        {
            // HandleInput();
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

                if (_gridPosition.x < -_levelGrid.GetWidth() || _gridPosition.x > _levelGrid.GetWidth() ||
                   _gridPosition.y < -_levelGrid.GetHeight() || _gridPosition.y > _levelGrid.GetHeight())
                {
                    EndEpisode();
                    return;
                }

                if (IsPositionInBody(_gridPosition))
                {
                    EndEpisode();
                    return;
                }

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