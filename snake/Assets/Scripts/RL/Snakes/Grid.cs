using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.RL.Snakes
{
    public class Grid
    {
        private Vector2Int _foodGridPosition;
        private GameObject _foodGameObject;
        private readonly int _width;
        private readonly int _height;
        private List<Snake> _snakeList;
        private bool _foodSpawned;

        public Grid(int width, int height)
        {
            _width = width;
            _height = height;
            _snakeList = new List<Snake>();
        }

        public void Setup(List<Snake> snakes)
        {
            _snakeList = snakes;
            CreateGridBorder();
            SpawnFood();
        }

        public void SpawnFood()
        {
            if (_foodSpawned)
            {
                return;
            }

            do
            {
                _foodGridPosition = new Vector2Int(Random.Range(-_width / 2, _width / 2), Random.Range(-_height / 2, _height / 2));
            }
            while (IsFoodOnSnake(_foodGridPosition));

            if (_foodGameObject != null)
            {
                Object.Destroy(_foodGameObject);
            }

            _foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
            _foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.instance.foodSprite;
            _foodGameObject.transform.position = new Vector3(_foodGridPosition.x, _foodGridPosition.y);
            _foodSpawned = true;
        }

        private bool IsFoodOnSnake(Vector2Int position)
        {
            foreach (var snake in _snakeList)
            {
                if (snake.GetFullSnakePositionList().Contains(position))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
        {
            if (snakeGridPosition == _foodGridPosition)
            {
                _foodSpawned = false;
                SpawnFood();
                return true;
            }
            return false;
        }

        public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
        {
            if (gridPosition.x < -_width / 2)
            {
                gridPosition.x = _width / 2;
            }
            else if (gridPosition.x > _width / 2)
            {
                gridPosition.x = -_width / 2;
            }

            if (gridPosition.y < -_height / 2)
            {
                gridPosition.y = _height / 2;
            }
            else if (gridPosition.y > _height / 2)
            {
                gridPosition.y = -_height / 2;
            }

            return gridPosition;
        }

        public bool IsPositionInSnakesBody(Vector2Int gridPosition)
        {
            foreach (var snake in _snakeList)
            {
                if (snake.GetFullSnakePositionList().Contains(gridPosition))
                {
                    return true;
                }
            }
            return false;
        }

        public int GetWidth()
        {
            return _width;
        }

        public int GetHeight()
        {
            return _height;
        }

        public void CreateGridBorder()
        {
            // top border
            CreateBorder(new Vector2(0, _height + 2f), new Vector2(_width, 1));
            // bottom border
            CreateBorder(new Vector2(0, -_height - 2f), new Vector2(_width, 1));
            // left border
            CreateBorder(new Vector2(-_width - 2f, 0), new Vector2(1, _height));
            // right border
            CreateBorder(new Vector2(_width + 2f, 0), new Vector2(1, _height));
        }

        private void CreateBorder(Vector2 position, Vector2 size)
        {
            GameObject border = new GameObject("Border", typeof(SpriteRenderer), typeof(BoxCollider2D));
            border.transform.position = position;
            border.transform.localScale = size;

            Texture2D texture = new Texture2D(256, 256);
            texture.SetPixel(0, 0, Color.yellow);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            border.GetComponent<SpriteRenderer>().sprite = sprite;

            border.GetComponent<BoxCollider2D>().isTrigger = true;
        }

        public Vector2Int GetFoodPosition()
        {
            return _foodGridPosition;
        }
    }
}