using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDNA
{
    public List<List<List<float>>> weights = new List<List<List<float>>>();

    public SerializableDNA() { }

    public SerializableDNA(DNA dna)
    {
        var dnaLayers = dna.getDNA(); // List<float[][]>

        foreach (var layer in dnaLayers)
        {
            var layerList = new List<List<float>>();
            for (int i = 0; i < layer.Length; i++)
            {
                var neuronWeights = new List<float>();
                for (int j = 0; j < layer[i].Length; j++)
                {
                    neuronWeights.Add(layer[i][j]);
                }
                layerList.Add(neuronWeights);
            }
            weights.Add(layerList);
        }
    }

    public DNA ToDNA()
    {
        var newLayers = new List<float[][]>();

        foreach (var layerList in weights)
        {
            var layerArray = new float[layerList.Count][];
            for (int i = 0; i < layerList.Count; i++)
            {
                layerArray[i] = layerList[i].ToArray();
            }
            newLayers.Add(layerArray);
        }

        return new DNA(newLayers);
    }
}
