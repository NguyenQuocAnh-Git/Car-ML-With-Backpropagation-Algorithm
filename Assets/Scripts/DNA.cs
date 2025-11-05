using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNA {

    private List<float[][]> dna;
    private float mutationProb = 0.09f;
    private float maxVariation = 1f;
    private float maxMutation = 5f;

    public DNA()
    {
        var network = new NeuralNetwork();
        dna = network.getWeights();
    }

    public DNA(List<float[][]> weights)
    {
        this.dna = weights;
    }
    public List<float[][]> getDNA()
    {
        return dna;
    }
    public DNA mutate()
    {
        List<float[][]> newDna = new List<float[][]>();

        for (int i = 0; i < dna.Count; i++)
        {
            float[][] oldLayer = dna[i];
            float[][] newLayer = new float[oldLayer.Length][];

            for (int j = 0; j < oldLayer.Length; j++)
            {
                newLayer[j] = new float[oldLayer[j].Length];
                for (int k = 0; k < oldLayer[j].Length; k++)
                {
                    // copy gốc
                    float value = oldLayer[j][k];

                    // xác suất đột biến
                    if (Random.value < mutationProb)
                    {
                        value += Random.Range(-maxVariation, maxVariation);
                        // clamp để tránh giá trị quá lớn
                        value = Mathf.Clamp(value, -maxVariation, maxVariation);
                    }

                    newLayer[j][k] = value;
                }
            }

            newDna.Add(newLayer);
        }

        return new DNA(newDna);
    }
    //DNA of the class (parent) + DNA parameter (parent)
    public DNA crossover(DNA otherParent)
    {
        List<float[][]> child = new List<float[][]>();
        for(int i = 0; i < dna.Count; i++)
        {
            float[][] otherParentLayer = otherParent.getDNA()[i];
            float[][] parentLayer = dna[i];
            for(int j = 0; j < parentLayer.Length; j++)
            {
                for(int k = 0; k < parentLayer[j].Length; k++)
                {
                    float rand = Random.Range(0f, 1f);
                    if (rand < 0.5f)
                    {
                        //Second parent
                        parentLayer[j][k] = otherParentLayer[j][k];
                    }
                    else
                    {
                        //Same
                        //First parent
                        parentLayer[j][k] = parentLayer[j][k];
                    }

                }
            }
            child.Add(parentLayer);

        }
        return new DNA(child);
    }
}
