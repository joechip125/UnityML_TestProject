using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MazeMaker mazePrefab;

    private MazeMaker mazeInstance;
    
    void Start()
    {
        BeginGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            RestartGame();
    }

    private void BeginGame()
    {
        mazeInstance = Instantiate(mazePrefab) as MazeMaker;
        mazeInstance.Generate();
    }

    private void RestartGame()
    {
        Destroy(mazeInstance.gameObject);
        BeginGame();
    }
}
