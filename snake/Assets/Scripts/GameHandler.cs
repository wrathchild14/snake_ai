using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private Snake snake;
    private LevelGrid levelGrid;
    // Start is called before the first frame update
    void Start()
    {
        levelGrid = new LevelGrid(47, 47);
        snake.Setup(levelGrid);
        levelGrid.Setup(snake);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
