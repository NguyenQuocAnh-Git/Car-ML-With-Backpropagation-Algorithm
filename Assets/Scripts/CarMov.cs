using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMov : MonoBehaviour
{
    public float vyRot = 0;                 // Tốc độ quay hiện tại (theo trục Y)
    public float speedLinear = 10f;         // Tốc độ tuyến tính cơ bản
    public float speedRotation = 80f;       // Tốc độ quay cơ bản
    private float vz;                        // Vận tốc tịnh tiến hiện tại
    [SerializeField] private float acceleration;              // Gia tốc hiện tại
    private float increaseAcc = 5;           // Hệ số tăng gia tốc
    public bool activeAcceleration = true;   // Bật/tắt chế độ tăng tốc
    public float maxSpeed = 15f;             // Vận tốc tối đa (để chuẩn hóa)
    public float minSpeed = 1f;
    public float maxRotation = 90f;          // Tốc độ quay tối đa (độ/giây)

    [SerializeField] private float currentSpeed;              // Dùng để lưu tốc độ thực tế
    [SerializeField] private float currentRotation;           // Góc quay hiện tại (0–360)

    void Start()
    {
        vz = speedLinear;
    }

    void Update()
    {
        float time = Time.deltaTime;
    
        // Cập nhật vận tốc từ gia tốc
        if (vz <= minSpeed)
            acceleration = Mathf.Lerp(acceleration, increaseAcc * 0.5f, Time.deltaTime * 3f);

        vz += acceleration * time; 
        vz = Mathf.Clamp(vz, minSpeed, maxSpeed); // ko cho đi lùi

        // Cập nhật vị trí theo vận tốc mới
        transform.position += transform.forward * vz * time;

        float epsilon = 0.1f;
        if (Mathf.Abs(vz) < epsilon)
            vyRot = 0;

        // Cập nhật góc quay
        transform.Rotate(new Vector3(0, vyRot * time, 0));

        // Cập nhật tốc độ hiện tại
        currentSpeed = Mathf.Abs(vz);

        // Cập nhật góc quay hiện tại
        currentRotation = transform.eulerAngles.y;
    }

    public float getAcceleration()
    {
        return acceleration;
    }

    public void updateMovement(List<float> outputs)
    {
        // Output[0]: steering
        // if (outputs[0] * 2 > 1f)
        //     vyRot = (outputs[0] * 2 - 1) * speedRotation;
        // else
        //     vyRot = -(outputs[0] * 2) * speedRotation;

        // // Output[1]: acceleration
        // if (outputs[1] * 2 > 1f)
        //     acceleration = (outputs[1] * 2 - 1) * increaseAcc;
        // else
        //     acceleration = -outputs[1] * 2 * increaseAcc;

        // Output[0]: steering [-1,1]
        float steerInput = outputs[0] * 2f - 1f; // map 0–1 -> -1–1
        vyRot = steerInput * speedRotation; // speedRotation là độ/giây

        // Output[1]: acceleration [-1,1]
        float accInput = outputs[1] * 2f - 1f;
        float minForcedAcc = 0.8f; // luôn có ít nhất 0.8 gia tốc
        acceleration = Mathf.Max(accInput * increaseAcc * 1.5f, minForcedAcc);
        // acceleration = accInput * increaseAcc;
    }

    // ✅ Hàm lấy tốc độ thực tế (có thể dùng cho input NN)
    public float getCurrentSpeed()
    {
        return Mathf.Abs(currentSpeed); // Luôn dương
    }

    // ✅ Chuẩn hóa tốc độ về [0, 1]
    public float getNormalizedSpeed()
    {
        return Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
    }

    // ✅ Lấy góc quay hiện tại (0–360)
    public float getCurrentRotation()
    {
        return currentRotation;
    }

    // ✅ Chuẩn hóa góc quay về [0, 1]
    public float getNormalizedRotation()
    {
        return currentRotation / 360f;
    }
}
