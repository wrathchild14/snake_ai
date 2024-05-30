
using UnityEngine;

namespace Assets.Scripts
{
    public class HamiltonGameHandler : MonoBehaviour
    {
        [SerializeField] private HamiltonSnake _snakePrefab;
        private HamiltonSnake _snake;
        private LevelGridHamilton _levelGrid;

        public HamiltonGameHandler(HamiltonSnake snakePrefab)
        {
            _snakePrefab = snakePrefab;
        }

        [SerializeField] private const int GridSizeX = 20;
        [SerializeField] private const int GridSizeY = 20;

        void Start()
        {
            _levelGrid = new LevelGridHamilton(GridSizeX, GridSizeY);
            _snake = Instantiate(_snakePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _snake.Setup(_levelGrid);
            _levelGrid.Setup(_snake);
        }

    }
}