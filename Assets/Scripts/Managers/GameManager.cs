using UnityEngine;
using System.Collections;
using Unity.MLAgents;

public class GameManager : MonoBehaviour 
{

	public Maze mazePrefab;

	private Maze mazeInstance;
	
	public void Awake()
	{
		Academy.Instance.OnEnvironmentReset -= EnvironmentReset;
		Academy.Instance.OnEnvironmentReset += EnvironmentReset;
	}

	private void EnvironmentReset()
	{
		RestartGame();
	}

	private void Start () 
	{
		BeginGame();
	}
	
	private void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			RestartGame();
		}
	}

	private void BeginGame () 
	{
		mazeInstance = Instantiate(mazePrefab) as Maze;
		mazeInstance.Generate();
	}

	private void RestartGame () 
	{
		StopAllCoroutines();
		Destroy(mazeInstance.gameObject);
		BeginGame();
	}
}