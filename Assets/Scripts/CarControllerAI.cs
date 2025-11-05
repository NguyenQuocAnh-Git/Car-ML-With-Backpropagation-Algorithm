using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerAI : MonoBehaviour
{
    public List<GameObject> cars;
    public int population = 100;
    public int generation = 0;
    public GameObject car;
    public GameObject bestCar;
    [HideInInspector]
    public DNA winner;
    public DNA secWinner;
    private int carsCreated = 0;


    public DNA globalBest;
    public float globalBestScore = float.MinValue;


    // Use this for initialization
    void Start()
    {
        // At start, load saved winners if available
        bool ok = SaveManager.LoadWinners(out DNA first,out DNA second, out int generationOut);
        if (ok)
        {
            this.winner = first;
            this.secWinner = second;
            this.generation = generationOut;
            Debug.Log($"Generation : {generationOut}");
            globalBest = winner; // save later
            newPopulationAtStart();
            Debug.Log("CarControllerAI: Loaded saved winners at start.");
        }else
        {
            newPopulation();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void TryUpdateGlobalBest(DNA candidate, float candidateScore)
    {
        if (candidateScore > globalBestScore)
        {
            globalBestScore = candidateScore;
            globalBest = new DNA(candidate.getDNA());
        }
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
    public void newPopulationAtStart()
    {
        cars = new List<GameObject>();
        for (int i = 0; i < population; i++)
        {
            DNA newDna;
            // 5% cơ hội sinh cá thể hoàn toàn mới
            if (Random.value < 0.05f)
            {
                newDna = new DNA();
            }
            else // 95% còn lại lấy từ crossover giữa 2 bố mẹ
            {
                newDna = winner.crossover(secWinner);
                newDna = newDna.mutate();
            }
            GameObject carObj = Instantiate(car);
            cars.Add(carObj);
            carObj.GetComponent<Car>().Initialize(newDna);
        }
        generation++;
        carsCreated = 0;
        if (cars != null && cars.Count > 0)
        {
            GameObject.Find("Camera").GetComponent<CameraMovement>().Follow(cars[0]);
        }
    }
    // Called to create new generation using crossover of winner & secWinner
    public void newPopulation(bool geneticManipulation)
    {
        if (geneticManipulation && globalBest != null)
        {
            GameObject _bestCar = Instantiate(bestCar);
            _bestCar.GetComponent<Car>().Initialize(globalBest);
            cars.Add(_bestCar);

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
                DNA newDna;
                // 5% cơ hội sinh cá thể hoàn toàn mới
                if (Random.value < 0.05f)
                {
                    newDna = new DNA();
                }
                else // 95% còn lại lấy từ crossover giữa 2 bố mẹ
                {
                    newDna = winner.crossover(secWinner);
                    newDna = newDna.mutate();
                }
                GameObject carObj = Instantiate(car);
                cars.Add(carObj);
                carObj.GetComponent<Car>().Initialize(newDna);
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
        int generationOut;
        bool ok = SaveManager.LoadWinners(out loadedWinner, out loadedSecWinner, out generationOut);
        if (!ok)
        {
            Debug.LogWarning("CarControllerAI: No saved winners to load.");
            return;
        }

        // Set controller winners and spawn new population using them
        this.winner = loadedWinner;
        this.secWinner = loadedSecWinner;
        this.generation = generationOut;
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

        SaveManager.SaveWinners(winner, secWinner, generation);
        Debug.Log("✅ CarControllerAI: Winners saved successfully by user.");
    }

}
