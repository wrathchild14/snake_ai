using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets.Scripts.RL.Snakes
{
    public class Snake : Agent
    {
        private Vector2Int _gridPosition;
        private Vector2Int _gridMoveDirection;
        private float _gridMoveTimer;
        [SerializeField] private float _gridMoveTimerMax = 0.1f;
        private Grid _levelGrid;
        private List<Transform> _snakeBodyTransformList;
        [SerializeField] private bool _useRL = true;

        public void Setup(Grid levelGrid)
        {
            _levelGrid = levelGrid;
            // _gridPosition = new Vector2Int(Random.Range(-_levelGrid.GetWidth() / 2, _levelGrid.GetWidth() / 2), Random.Range(-_levelGrid.GetHeight() / 2, _levelGrid.GetHeight() / 2));
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
            
            _gridPosition = new Vector2Int(Random.Range(-_levelGrid.GetWidth() / 2, _levelGrid.GetWidth() / 2), Random.Range(-_levelGrid.GetHeight() / 2, _levelGrid.GetHeight() / 2));

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
            Vector2 normalizedHeadPosition = (_gridPosition + new Vector2(_levelGrid.GetWidth(), _levelGrid.GetHeight())) / new Vector2((float)_levelGrid.GetWidth() * 2, (float)_levelGrid.GetHeight() * 2);
            sensor.AddObservation(normalizedHeadPosition);

            Vector2 normalizedFoodPosition = (_levelGrid.GetFoodPosition() + new Vector2(_levelGrid.GetWidth(), _levelGrid.GetHeight())) / new Vector2((float)_levelGrid.GetWidth() * 2, (float)_levelGrid.GetHeight() * 2);
            sensor.AddObservation(normalizedFoodPosition);

            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    sensor.AddObservation(IsObstacleInDirection(new Vector2Int(dx, dy)));
                }
            }
        }

        private bool IsObstacleInDirection(Vector2Int direction)
        {
            Vector2Int nextPosition = _gridPosition + direction;

            // wall
            if (nextPosition.x < -_levelGrid.GetWidth() || nextPosition.y < -_levelGrid.GetHeight() || nextPosition.x > _levelGrid.GetWidth() || nextPosition.y > _levelGrid.GetHeight())
            {
                return true;
            }

            // snake bodies
            return _levelGrid.IsPositionInSnakesBody(nextPosition);
        }

        private void FixedUpdate()
        {
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

        private void UpdateMovement() {
            float oldDistance = Vector2.Distance(_gridPosition, _levelGrid.GetFoodPosition());

            _gridPosition += _gridMoveDirection;

            float newDistance = Vector2.Distance(_gridPosition, _levelGrid.GetFoodPosition());
            if (newDistance < oldDistance)
            {
                AddReward(0.1f);
            }
            else
            {
                AddReward(-0.1f);
            }

            if (_gridPosition.x < -_levelGrid.GetWidth() || _gridPosition.x > _levelGrid.GetWidth() ||
                _gridPosition.y < -_levelGrid.GetHeight() || _gridPosition.y > _levelGrid.GetHeight())
            {
                AddReward(-1f);
                EndEpisode();
                return;
            }

            if (_levelGrid.IsPositionInSnakesBody(_gridPosition))
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
            snakeBodyGameObject.transform.SetParent(this.gameObject.transform);
            _snakeBodyTransformList.Insert(0, snakeBodyGameObject.transform);
            snakeBodyGameObject.transform.position = new Vector3(_gridPosition.x, _gridPosition.y);
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
}