using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerAI : MonoBehaviour
{
    public List<GameObject> cars;
    public int population = 20;
    public int generation = 0;
    public GameObject car;
    [HideInInspector]
    public DNA winner;
    public DNA secWinner;
    private int carsCreated = 0;

    // Use this for initialization
    void Start()
    {
        // At start, just create new population normally
        newPopulation();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<GameObject> getCars()
    {
        return cars;
    }

    public void newPopulation()
    {
        cars = new List<GameObject>();
        for (int i = 0; i < population; i++)
        {
            GameObject carObj = (Instantiate(car));
            cars.Add(carObj);
            carObj.GetComponent<Car>().Initialize();
        }
        generation++;
        Debug.Log(generation);
    }

    // Called to create new generation using crossover of winner & secWinner
    public void newPopulation(bool geneticManipulation)
    {
        if (geneticManipulation)
        {
            if (winner == null || secWinner == null)
            {
                Debug.LogWarning("CarControllerAI: winner or secWinner is null; cannot perform genetic manipulation. Creating random population instead.");
                newPopulation();
                return;
            }

            Debug.Log("CarControllerAI: Creating new population with genetic manipulation.");
            cars = new List<GameObject>();
            for (int i = 0; i < population; i++)
            {
                DNA dna = winner.crossover(secWinner);
                DNA mutated = dna.mutate();
                GameObject carObj = Instantiate(car);
                cars.Add(carObj);
                carObj.GetComponent<Car>().Initialize(mutated);
            }
        }
        generation++;
        carsCreated = 0;
        if (cars != null && cars.Count > 0)
        {
            GameObject.Find("Camera").GetComponent<CameraMovement>().Follow(cars[0]);
        }
    }

    public void restartGeneration()
    {
        cars.Clear();
        newPopulation();
    }

    // PUBLIC method to load winners from file and spawn a population using them
    public void LoadSavedWinnersAndCreatePopulation()
    {
        DNA loadedWinner, loadedSecWinner;
        bool ok = SaveManager.LoadWinners(out loadedWinner, out loadedSecWinner);
        if (!ok)
        {
            Debug.LogWarning("CarControllerAI: No saved winners to load.");
            return;
        }

        // Set controller winners and spawn new population using them
        this.winner = loadedWinner;
        this.secWinner = loadedSecWinner;
        Debug.Log("CarControllerAI: Winners loaded. Creating population from saved winners.");
        newPopulation(true);
    }

    // Save winners manually when button is clicked
    public void SaveCurrentWinners()
    {
        if (winner == null || secWinner == null)
        {
            Debug.LogWarning("⚠️ CarControllerAI: No winners available yet. Please wait until a generation completes.");
            return;
        }

        SaveManager.SaveWinners(winner, secWinner);
        Debug.Log("✅ CarControllerAI: Winners saved successfully by user.");
    }

}
