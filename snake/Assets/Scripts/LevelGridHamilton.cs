using UnityEngine;

namespace Assets.Scripts
{
    public class LevelGridHamilton
    {
        private Vector2Int _foodGridPosition;
        private GameObject _foodGameObject;
        private readonly int _width;
        private readonly int _height;
        private HamiltonSnake _snake;

        public LevelGridHamilton(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Setup(HamiltonSnake snake)
        {
            _snake = snake;
            snake.Setup(this);
            SpawnFood();
            CreateGridBorder();
        }

        private void SpawnFood()
        {
            do
            {
                _foodGridPosition = new Vector2Int(Random.Range(-_width / 2, _width / 2), Random.Range(-_height / 2, _height / 2));
            }
            while (_snake.GetFullSnakePositionList().Contains(_foodGridPosition));

            if (_foodGameObject != null)
            {
                Object.Destroy(_foodGameObject);
            }

            _foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
            _foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.instance.foodSprite;
            _foodGameObject.transform.position = new Vector3(_foodGridPosition.x, _foodGridPosition.y);
        }

        public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
        {
            if (snakeGridPosition == _foodGridPosition)
            {
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
    }
}