using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveManager
{
    private static readonly string folderPath = Path.Combine(Application.dataPath, "DNA_Saver");
    private static readonly string filePath = Path.Combine(folderPath, "winners.json");


    // Ghi DNA thủ công
    public static void SaveWinners(DNA winner, DNA secWinner, int generation)
    {
        if (winner == null || secWinner == null)
        {
            Debug.LogWarning("SaveManager: winner or secWinner is null, skip saving.");
            return;
        }

        string json = BuildDNAJson(winner, secWinner, generation);

        try
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);


            File.WriteAllText(filePath, json, Encoding.UTF8);
            Debug.Log($"SaveManager: Winners saved successfully at: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: Failed to write file: {e.Message}");
        }
    }

    private static string BuildDNAJson(DNA w, DNA s, int generation)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"generation\":");
        sb.Append(generation);
        sb.Append(",{\"winner\":");
        sb.Append(DnaToJson(w));
        sb.Append(",\"secWinner\":");
        sb.Append(DnaToJson(s));
        sb.Append("}");
        return sb.ToString();
    }

    private static string DnaToJson(DNA dna)
    {
        var layers = dna.getDNA();
        StringBuilder sb = new StringBuilder();
        sb.Append("[");

        for (int i = 0; i < layers.Count; i++)
        {
            sb.Append("[");
            var layer = layers[i];
            for (int j = 0; j < layer.Length; j++)
            {
                sb.Append("[");
                sb.Append(string.Join(",", layer[j]));
                sb.Append("]");
                if (j < layer.Length - 1) sb.Append(",");
            }
            sb.Append("]");
            if (i < layers.Count - 1) sb.Append(",");
        }
        sb.Append("]");
        return sb.ToString();
    }

    // Đọc JSON thủ công
    public static bool LoadWinners(out DNA winner, out DNA secWinner, out int generation)
    {
        winner = null;
        secWinner = null;
        generation = 0;

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"SaveManager: No saved file found at {filePath}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            ParseWinners(json, out winner, out secWinner, out generation);
            Debug.Log($"SaveManager: Loaded winners successfully from {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: Failed to load file: {e.Message}");
            return false;
        }
    }

    private static void ParseWinners(string json, out DNA winner, out DNA secWinner, out int generation)
    {
        winner = null;
        secWinner = null;
        generation = 0;

        try
        {
            // Lấy giá trị generation
            int genIndex = json.IndexOf("\"generation\":");
            if (genIndex >= 0)
            {
                int genStart = genIndex + "\"generation\":".Length;
                int genEnd = json.IndexOf(",", genStart);
                string genStr = json.Substring(genStart, genEnd - genStart);
                int.TryParse(genStr.Trim(), out generation);
            }

            // Tách phần winner / secWinner như cũ
            int startW = json.IndexOf("[[");
            int mid = json.IndexOf("],\"secWinner\":");
            int startS = json.IndexOf("[[", mid);
            int end = json.LastIndexOf("]");

            if (startW == -1 || startS == -1)
            {
                Debug.LogError("SaveManager: Invalid file structure.");
                return;
            }

            string jsonWinner = json.Substring(startW, mid - startW);
            string jsonSec = json.Substring(startS, end - startS);

            winner = new DNA(ParseLayers(jsonWinner));
            secWinner = new DNA(ParseLayers(jsonSec));
        }
        catch
        {
            Debug.LogError("SaveManager: Failed to parse generation or DNA.");
        }
    }


    private static List<float[][]> ParseLayers(string text)
    {
        // Cấu trúc JSON thực tế là một danh sách các neuron, không phải danh sách các lớp.
        // Mỗi neuron là một mảng trọng số. Ta phải gom chúng theo kích thước lớp thực tế.
        text = text.Trim('[', ']');

        // Bóc tách từng neuron riêng biệt
        List<float[]> neurons = new List<float[]>();
        string[] neuronGroups = text.Split(new string[] { "],[" }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string neuronStr in neuronGroups)
        {
            string clean = neuronStr.Replace("[", "").Replace("]", "");
            string[] nums = clean.Split(',');
            List<float> weights = new List<float>();

            foreach (string n in nums)
            {
                if (float.TryParse(n, out float val))
                    weights.Add(val);
            }

            if (weights.Count > 0)
                neurons.Add(weights.ToArray());
        }

        // 🔧 Giờ ta nhóm neuron thành từng lớp dựa theo cấu trúc mạng hiện tại
        // Mạng hiện tại có 5 input, 10 hidden, 2 output => 2 lớp trọng số: 5x10 và 10x2
        int inputs = NeuralNetwork.inputs;
        int hidden = NeuralNetwork.size_hidden_layers;
        int outputs = NeuralNetwork.outputs;
        int hiddenLayers = NeuralNetwork.hiddenLayers;

        List<float[][]> layers = new List<float[][]>();

        // Lớp 1: input → hidden (5x10)
        if (neurons.Count >= inputs)
        {
            float[][] layer1 = new float[inputs][];
            for (int i = 0; i < inputs; i++)
            {
                // neuron[i] chứa 10 giá trị
                layer1[i] = neurons[i];
            }
            layers.Add(layer1);
        }

        // Lớp 2: hidden → output (10x2)
        if (neurons.Count >= inputs + hidden)
        {
            float[][] layer2 = new float[hidden][];
            for (int i = 0; i < hidden; i++)
            {
                layer2[i] = neurons[inputs + i];
            }
            layers.Add(layer2);
        }

        return layers;
    }

}
