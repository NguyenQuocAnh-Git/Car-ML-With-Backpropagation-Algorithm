using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMov : MonoBehaviour
{
    private float vyRot = 0;                 // Tốc độ quay hiện tại (theo trục Y)
    public float speedLinear = 0.2f;         // Tốc độ tuyến tính cơ bản
    public float speedRotation = 0.5f;       // Tốc độ quay cơ bản
    private float vz;                        // Vận tốc tịnh tiến hiện tại
    private float acceleration;              // Gia tốc hiện tại
    private float increaseAcc = 5;           // Hệ số tăng gia tốc
    public bool activeAcceleration = true;   // Bật/tắt chế độ tăng tốc
    public float maxSpeed = 10f;             // Vận tốc tối đa (để chuẩn hóa)
    public float maxRotation = 90f;          // Tốc độ quay tối đa (độ/giây)

    private float currentSpeed;              // Dùng để lưu tốc độ thực tế
    private float currentRotation;           // Góc quay hiện tại (0–360)

    void Start()
    {
        vz = speedLinear;
    }

    void Update()
    {
        float time = Time.deltaTime;

        // Cập nhật vị trí theo hướng hiện tại
        transform.position += transform.forward * (vz * time + 0.5f * acceleration * time * time);

        // Cập nhật góc quay
        transform.Rotate(new Vector3(0, vyRot, 0));

        // Tính toán tốc độ hiện tại
        currentSpeed = vz + acceleration * time;
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

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
        if (outputs[0] * 2 > 1f)
            vyRot = (outputs[0] * 2 - 1) * speedRotation * Time.deltaTime;
        else
            vyRot = -(outputs[0] * 2) * speedRotation * Time.deltaTime;

        // Output[1]: acceleration
        if (outputs[1] * 2 > 1f)
            acceleration = (outputs[1] * 2 - 1) * increaseAcc;
        else
            acceleration = -outputs[1] * 2 * increaseAcc;
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
