using UnityEngine;

namespace Assets.Scripts.RL
{
    public class SmartGameHandler : MonoBehaviour
    {
        [SerializeField] private SmartSnake _snakePrefab;
        private SmartSnake _snake;
        private SmartLevelGrid _levelGrid;

        public SmartGameHandler(SmartSnake snakePrefab)
        {
            _snakePrefab = snakePrefab;
        }

        [SerializeField] private const int GridSizeX = 20;
        [SerializeField] private const int GridSizeY = 20;

        void Start()
        {
            _levelGrid = new SmartLevelGrid(GridSizeX, GridSizeY);
            _snake = Instantiate(_snakePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _snake.Setup(_levelGrid);
            _levelGrid.Setup(_snake);
        }

    }
}