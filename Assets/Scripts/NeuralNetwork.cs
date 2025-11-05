using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {

    // Số lượng tầng ẩn (hidden layers) trong mạng
    // 1 nghĩa là chỉ có 1 hidden layer giữa input và output
    public static int hiddenLayers = 1;

    // Số neuron trong mỗi tầng ẩn
    // Ví dụ: nếu =10 thì mỗi hidden layer có 10 neuron
    public  static int size_hidden_layers = 10;

    // Số lượng đầu ra của mạng
    // Ví dụ: 2 nếu điều khiển xe thì có thể là [xoay vô-lăng, tăng tốc]
    public static int outputs = 2;

    // Số lượng đầu vào của mạng
    // 8 raycast + Velocity(car) + rotation(car)
    public static int inputs = 10;

    // Giá trị cực đại khi khởi tạo trọng số ngẫu nhiên
    // Mỗi weight sẽ nằm trong khoảng [-maxInitialValue, +maxInitialValue]
    public static float maxInitialValue = 5f;

    // Dùng cho hàm kích hoạt sigmoid: 1 / (1 + e^(-x))
    private const float EULER_NUMBER = 2.71828f;

    // Danh sách các neuron theo tầng
    // neurons[layer][i] = giá trị kích hoạt của neuron thứ i trong layer
    private List<List<float>> neurons;

    // Danh sách các trọng số kết nối giữa các tầng
    // weights[layer][i][j] = trọng số nối từ neuron i (layer hiện tại)
    //                        tới neuron j (layer kế tiếp)
    private List<float[][]> weights;

    // Tổng số tầng của mạng (input + hidden + output)
    // = hiddenLayers + 2
    private int totalLayers = 0;

    public NeuralNetwork()
    {
        totalLayers = hiddenLayers + 2;// hidden layers + inputslayer+outputlayer
        //Initialize weights and the neurons array
        weights = new List<float[][]>();
        neurons = new List<List<float>>();

        //Fill neurons and weights
        for(int i = 0; i < totalLayers; i++)
        {
            float[][] layerWeights;
            List<float> layer = new List<float>();
            int sizeLayer = getSizeLayer(i);
            if (i != 1 + hiddenLayers)
            {
                layerWeights = new float[sizeLayer][];
                int nextSizeLayer = getSizeLayer(i + 1);
                for(int j = 0; j < sizeLayer; j++)//current
                {
                    layerWeights[j] = new float[nextSizeLayer];
                    for(int k = 0; k < nextSizeLayer; k++)// next layer
                    {
                        layerWeights[j][k] = genRandomValue();
                    }
                }
                weights.Add(layerWeights);
            }
            for(int j = 0; j < sizeLayer; j++)
            {
                layer.Add(0);
            }
            neurons.Add(layer);
        }

    }
    public NeuralNetwork(DNA dna)
    {
        List<float[][]> weightsDNA = dna.getDNA();
        totalLayers = hiddenLayers + 2;// hidden layers + inputslayer + outputlayer
        //Initialize weights and the neurons array
        weights = new List<float[][]>();
        neurons = new List<List<float>>();

        //Fill neurons and weights
        for (int i = 0; i < totalLayers; i++)
        {
         
            float[][] layerWeights;
            float[][] weightsDNALayer;
            List<float> layer = new List<float>();
            int sizeLayer = getSizeLayer(i);
            if (i != 1 + hiddenLayers)
            {
                weightsDNALayer = weightsDNA[i];
                layerWeights = new float[sizeLayer][];
                int nextSizeLayer = getSizeLayer(i + 1);
                for (int j = 0; j < sizeLayer; j++)//current
                {
                    layerWeights[j] = new float[nextSizeLayer];
                    for (int k = 0; k < nextSizeLayer; k++)// next layer
                    {
                        layerWeights[j][k] = weightsDNALayer[j][k];
                    }
                }
                weights.Add(layerWeights);
            }
            for (int j = 0; j < sizeLayer; j++)
            {
                layer.Add(0);
            }
            neurons.Add(layer);
        }
    }
    public void feedForward(float[]inputs)
    {
    
        //Set inputs in input layer
        List<float> inputLayer = neurons[0];
        for(int i = 0; i < inputs.Length; i++)
        {
            inputLayer[i] = inputs[i];
        }
        //Update neurons from the input Layer to the output Layer
        for(int layer =0;layer< neurons.Count-1; layer++) {
            
            float[][] weightsLayer = weights[layer];
            int nextLayer = layer + 1;
            List<float> neuronsLayer = neurons[layer];
            List<float> neuronsNextLayer = neurons[nextLayer];
            for(int i = 0; i < neuronsNextLayer.Count; i++) //Next layer
            {
                float sum = 0;
                for(int j = 0; j < neuronsLayer.Count; j++)
                {
                    sum += weightsLayer[j][i] * neuronsLayer[j];//Feed-forward multiplication
                }
                neuronsNextLayer[i] = sigmoid(sum);

            }
        }
    
    }
    public int getSizeLayer(int i)
    {
        //Layer position i
        int sizeLayer = 0;
        //Depending on the layer it will give a different size
        if (i == 0)
        {
            sizeLayer = inputs;

        }else if(i == hiddenLayers + 1)
        {
            sizeLayer = outputs;
        }
        else
        {
            sizeLayer = size_hidden_layers;
        }
        return sizeLayer;
    }
    public List<float> getOutputs()
    {
        return neurons[neurons.Count - 1];
    }
    public float sigmoid(float x)
    {
        return 1 / (float)(1 + Mathf.Pow(EULER_NUMBER, -x));
    }
    public float genRandomValue()
    {
        return Random.Range(-maxInitialValue, maxInitialValue);
    }
    public List<List<float>> getNeurons()
    {
        return neurons;
    }
    public List<float[][]> getWeights()
    {
        return weights;
    }
}
