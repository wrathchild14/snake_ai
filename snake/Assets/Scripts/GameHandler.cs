using UnityEngine;

namespace Assets.Scripts
{
    public class GameHandler : MonoBehaviour
    {
        [SerializeField] private Snake _snakePrefab;
        private Snake _snake;
        private LevelGrid _levelGrid;

        public GameHandler(Snake snakePrefab)
        {
            _snakePrefab = snakePrefab;
        }

        [SerializeField] private const int GridSizeX = 20;
        [SerializeField] private const int GridSizeY = 20;

        void Start()
        {
            _levelGrid = new LevelGrid(GridSizeX, GridSizeY);
            _snake = Instantiate(_snakePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _snake.Setup(_levelGrid);
            _levelGrid.Setup(_snake);
        }

    }
}