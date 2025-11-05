using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Car : MonoBehaviour
{
    // === Fields ===
    private DNA dna;
    private NeuralNetwork network;

    private Vector3 initialPoint;
    private Vector3 lastPos;

    private float distance;
    private float timeAlive;
    private float fitness;

    public int roundsPassed;
    private bool initialized;

    // === Unity Methods ===
    private void Start() { }

    private void Update()
    {
        if (!initialized) return;

        UpdateStats();
        UpdateMovement();
    }

    private void OnTriggerEnter(Collider col)
    {
        switch (col.gameObject.tag)
        {
            case "finish":
                roundsPassed++;
                fitness += 100; // Bonus khi hoàn thành vòng
                if(roundsPassed >= 50)
                {
                    SaveCarFinishRound();
                    break;
                }
                break;

            case "barie":
                HandleCollision();
                break;
        }
    }

    // === Initialization ===
    public void Initialize()
    {
        network = new NeuralNetwork();
        dna = new DNA(network.getWeights());
        InitializeCommon();
    }

    public void Initialize(DNA dna)
    {
        network = new NeuralNetwork(dna);
        this.dna = dna;
        InitializeCommon();
    }

    private void InitializeCommon()
    {
        initialPoint = transform.position;
        initialized = true;
        ResetStats();
    }

    private void ResetStats()
    {
        distance = 0f;
        timeAlive = 0f;
        lastPos = transform.position;
        fitness = 0f;
        roundsPassed = 0;
    }

    // === Core Logic ===
    private void UpdateStats()
    {
        timeAlive += Time.deltaTime;
        distance += Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;
    }

    private void UpdateMovement()
    {
        var lasers = GetComponent<Lasers>();
        var carMov = GetComponent<CarMov>();

        float[] laserInputs = lasers.getDistances();
        float normSpeed = carMov.getNormalizedSpeed();
        float normRot = carMov.getNormalizedRotation();

        // Gộp tất cả input lại
        float[] inputs = new float[laserInputs.Length + 2];
        laserInputs.CopyTo(inputs, 0);
        inputs[^2] = normSpeed;
        inputs[^1] = normRot;

        // Feed-forward và điều khiển
        network.feedForward(inputs);
        List<float> outputs = network.getOutputs();
        carMov.updateMovement(outputs);

        // Cập nhật lại khoảng cách từ điểm xuất phát
        distance = Vector3.Distance(transform.position, initialPoint);
    }

    // === Fitness ===
    public float GetFitnessScore()
    {
        fitness = distance * 1.5f + timeAlive * 1.2f;
        return fitness;
    }

    public DNA GetDNA() => dna;

    // === Camera & Population Logic ===
    private void SaveCarFinishRound()
    {
        var controller = GameObject.Find("CarController").GetComponent<CarControllerAI>();
        float score = GetFitnessScore();

        controller.TryUpdateGlobalBest(dna, score);

        // 🔥 Lưu ngay cá thể này
        SaveManager.SaveWinners(dna, controller.secWinner ?? dna, controller.generation);
        Destroy(gameObject);
    }
    private void HandleCollision()
    {
        var controller = GameObject.Find("CarController").GetComponent<CarControllerAI>();
        List<GameObject> cars = controller.getCars();

        float score = GetFitnessScore();
        if (cars.Count > 2) score *= 0.2f; // penalty nếu không trong top 2

        controller.TryUpdateGlobalBest(dna, score);

        if (cars.Count == 2)
        {
            controller.winner = cars[0].GetComponent<Car>().GetDNA();
            controller.secWinner = cars[1].GetComponent<Car>().GetDNA();
        }

        if (cars.Count == 1)
        {
            EnsureWinnersExist(controller, cars);
            SaveManager.SaveWinners(controller.winner, controller.secWinner, controller.generation);
            Debug.Log("Car: Winners saved successfully before creating new population.");

            // Đảm bảo winner đúng thứ tự
            if (!controller.winner.Equals(cars[0].GetComponent<Car>().GetDNA()))
            {
                (controller.winner, controller.secWinner) = (controller.secWinner, controller.winner);
            }

            cars.Remove(gameObject);
            controller.newPopulation(true);
            Destroy(gameObject);
            return;
        }

        // Đổi camera theo random xe khác
        FollowRandomCar(controller, cars);
    }

    private static void EnsureWinnersExist(CarControllerAI controller, List<GameObject> cars)
    {
        if (controller.winner == null || controller.secWinner == null)
        {
            if (cars.Count >= 2)
            {
                controller.winner = cars[0].GetComponent<Car>().GetDNA();
                controller.secWinner = cars[1].GetComponent<Car>().GetDNA();
            }
            else
            {
                controller.winner = cars[0].GetComponent<Car>().GetDNA();
                controller.secWinner = new DNA(controller.winner.getDNA());
            }
        }
    }

    private void FollowRandomCar(CarControllerAI controller, List<GameObject> cars)
    {
        int rand = Random.Range(0, cars.Count);
        GameObject cameraObj = GameObject.Find("Camera");
        CameraMovement camera = cameraObj.GetComponent<CameraMovement>();

        if (cars[rand] == gameObject)
        {
            HandleCollision(); // thử lại nếu trùng chính xe hiện tại
            return;
        }

        if (gameObject == camera.getFollowing())
        {
            camera.Follow(cars[rand]);
        }

        cars.Remove(gameObject);
        Destroy(gameObject);
    }
}
