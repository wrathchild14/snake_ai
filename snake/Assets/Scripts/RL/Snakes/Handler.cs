using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.RL.Snakes
{
    public class Handler : MonoBehaviour
    {
        [SerializeField] private List<Snake> _snakes;
        private Grid _levelGrid;

        [SerializeField] private int GridSizeX = 20;
        [SerializeField] private int GridSizeY = 20;

        void Start()
        {
            _levelGrid = new Grid(GridSizeX, GridSizeY);
            //_snake = Instantiate(_snakePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _levelGrid.Setup(_snakes);
            foreach (var snake in _snakes)
            {
                snake.Setup(_levelGrid);
            }
        }
    }
}