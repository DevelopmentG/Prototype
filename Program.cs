using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class NaiveBayesDiseasePredictor
{
    private Dictionary<string, int> diseaseCounts = new Dictionary<string, int>();
    private Dictionary<string, Dictionary<string, int>> symptomCounts = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, string> diseaseTreatments = new Dictionary<string, string>();
    private int totalCases = 0;
    private const double LaplaceSmoothing = 1.0;

    public void Train(string csvFilePath)
    {
        var lines = File.ReadAllLines(csvFilePath).Skip(1); // Skip header

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length < 4) continue;

            string disease = parts[1].Trim().ToLower();  // Disease Name
            string[] symptoms = parts[2].ToLower().Split(';').Select(s => s.Trim()).ToArray(); // Symptoms split by semicolon
            string treatment = parts[3].Trim(); // Treatment information

            if (!diseaseCounts.ContainsKey(disease))
                diseaseCounts[disease] = 0;
            diseaseCounts[disease]++;

            if (!diseaseTreatments.ContainsKey(disease))
                diseaseTreatments[disease] = treatment;

            foreach (var symptom in symptoms)
            {
                if (!symptomCounts.ContainsKey(symptom))
                    symptomCounts[symptom] = new Dictionary<string, int>();

                if (!symptomCounts[symptom].ContainsKey(disease))
                    symptomCounts[symptom][disease] = 0;

                symptomCounts[symptom][disease]++;
            }

            totalCases++;
        }
    }

    public void Predict(string[] inputSymptoms)
    {
        Dictionary<string, double> probabilities = new Dictionary<string, double>();

        foreach (var disease in diseaseCounts.Keys)
        {
            double prior = (double)(diseaseCounts[disease] + LaplaceSmoothing) / (totalCases + diseaseCounts.Count * LaplaceSmoothing);
            double likelihood = 1.0;

            foreach (var symptom in inputSymptoms)
            {
                int symptomCount = symptomCounts.ContainsKey(symptom) && symptomCounts[symptom].ContainsKey(disease)
                    ? symptomCounts[symptom][disease]
                    : 0;

                double symptomProbability = (symptomCount + LaplaceSmoothing) / (diseaseCounts[disease] + 2 * LaplaceSmoothing);

                likelihood *= symptomProbability;
            }

            probabilities[disease] = prior * likelihood;
        }

        var sortedPredictions = probabilities.OrderByDescending(p => p.Value).Take(5).ToList(); // Take only top 5

        Console.WriteLine("\nTop 5 Predicted Diseases:");
        foreach (var prediction in sortedPredictions)
        {
            string treatment = diseaseTreatments.ContainsKey(prediction.Key) ? diseaseTreatments[prediction.Key] : "No treatment data available";

            double percentage = prediction.Value * 100;

             Console.WriteLine($"{prediction.Key}: {percentage:F2}% | Treatment: {treatment}");   
             
                  }
    }
}

class Program
{
    static void Main()
    {
        string filePath = "Diseases_Symptoms_Updated.csv"; // Ensure correct CSV path
        var predictor = new NaiveBayesDiseasePredictor();
        predictor.Train(filePath);

        Console.Write("\nEnter symptoms (comma-separated, e.g., palpitations, sweating): ");
        string[] inputSymptoms = Console.ReadLine().ToLower().Split(',').Select(s => s.Trim()).ToArray();

        predictor.Predict(inputSymptoms);
    }
}
