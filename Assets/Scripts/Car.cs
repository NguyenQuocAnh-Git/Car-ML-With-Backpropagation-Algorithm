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

    public float currentProgress;
    public float stuckDistanceThreshold = 5f;  // nếu trong 4.5s mà di chuyển < 5m → coi là kẹt/quay vòng
    public float lastSignificantProgress = 0f;
    public float timeSinceSignificantProgress = 0f;
    public float stuckTimeLimit = 4.5f;        // 4.5 giây không đi xa hơn 5 đơn vị → chết
    private float totalDistanceTraveled = 0f;
    private CarMov carMov;
    // === Unity Methods ===
    private void Start()
    {
        carMov = GetComponent<CarMov>();
        InitializeCommon();
    }

    private void Update()
    {
        if (!initialized) return;

        UpdateStats();
        UpdateMovement();

        // CheckIfStuck();
    }
    private void CheckIfStuck()
    {
        // float currentProgress;

        // Cách 1: Dùng khoảng cách từ spawn (nếu track không có checkpoint)
        // Nếu track chạy theo trục Z thì dùng cái này tốt hơn:
        // float currentProgress = transform.position.z;

        // Nếu xe đã di chuyển được thêm ít nhất X đơn vị so với lần kiểm tra trước → có tiến bộ
        if (totalDistanceTraveled > lastSignificantProgress + 6f)  // đi thêm được 6m
        {
            lastSignificantProgress = totalDistanceTraveled;
            timeSinceSignificantProgress = 0f;
        }
        else
        {
            timeSinceSignificantProgress += Time.deltaTime;
        }

        if (timeSinceSignificantProgress > 4.5f)
        {
            Debug.Log("Die ");
            HandleCollision(); // xe quay vòng hoặc kẹt thật → chết
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        switch (col.gameObject.tag)
        {
            case "finish":
                roundsPassed++;
                fitness += 100; // Bonus khi hoàn thành vòng
                if(roundsPassed >= 5)
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
        totalDistanceTraveled += carMov.getCurrentSpeed() * Time.deltaTime;
    }

    private void UpdateMovement()
    {
        var lasers = GetComponent<Lasers>();

        float[] laserInputs = lasers.getDistances();
        float leftSide = (laserInputs[0] + laserInputs[1]) / 2f;   // 2 laser trái nhất
        float rightSide = (laserInputs[15] + laserInputs[16]) / 2f; // 2 laser phải nhất
        float centerError = (leftSide - rightSide) / 10f;  // [-1, +1]: âm=lệch trái, dương=lệch phải
        float normSpeed = carMov.getNormalizedSpeed();
        float normRot = carMov.getNormalizedRotation();

        // Gộp tất cả input lại
        float[] inputs = new float[21];

        laserInputs.CopyTo(inputs, 0);
        inputs[18] = centerError;     // input mới – cực kỳ quan trọng
        inputs[19] = normSpeed;
        inputs[20] = normRot;

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
        float[] laserDistances = GetComponent<Lasers>().getDistances();
        float centerBonus = 0f;

        // 🔥 TÍNH BONUS ĐI GIỮA: so sánh laser trái vs phải
        float leftAvg = (laserDistances[0] + laserDistances[1] + laserDistances[15] + laserDistances[16]) / 4f;
        float rightAvg = (laserDistances[2] + laserDistances[3] + laserDistances[14]) / 3f;  // tùy index

        float centerliness = 1f - Mathf.Abs(leftAvg - rightAvg) / Mathf.Max(leftAvg + rightAvg, 1f);
        centerBonus = centerliness * 15f * Time.deltaTime;  // reward cao!

        fitness += centerBonus;  // thêm vào fitness chính

        float baseFitness = totalDistanceTraveled * 350f 
                        + roundsPassed * 3000f 
                        + carMov.getCurrentSpeed() * 5f
                        + timeAlive * 0.05f; // rất thấp

        // Penalty quay vòng tròn
        if (carMov.getCurrentSpeed() < 3f && Mathf.Abs(carMov.vyRot) > 30f)
            baseFitness -= 10f;

        return baseFitness;
    }

    public DNA GetDNA() => dna;

    // === Camera & Population Logic ===
    private void SaveCarFinishRound()
    {
        var controller = GameObject.Find("CarController").GetComponent<CarControllerAI>();
        float score = GetFitnessScore();
         List<GameObject> cars = controller.getCars();

        controller.TryUpdateGlobalBest(dna, score);

        // 🔥 Lưu ngay cá thể này
        SaveManager.SaveWinners(dna, controller.secWinner ?? dna, controller.generation);
        cars.Remove(gameObject);
        Destroy(gameObject);
        if(cars.Count == 0)
        {
            controller.restartGeneration();
        }
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
