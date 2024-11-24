using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CurriculumLearning 
{
    static List<Vector2Int> intervalEnvironment = new List<Vector2Int>();

    static private List<float> percentagesOfEnvironment = new List<float>();

    public static void InitCurriculumLearining(
            List<float> percentages,
            int maxStepHyperparameter
        ) {

        // Calcola la somma delle percentuali
        float total = 0f;
        foreach(var percentage in percentages) {
            total += percentage;
        }
        // Tolleranza per arrotondamento
        float tolerance = 0.0001f;
        // Assert: verifica se la somma è (quasi) uguale a 1
        if(Mathf.Abs(total - 1f) >= tolerance) {
            Debug.LogError($"La somma delle percentuali ({total}) non è uguale a 1!");
            Debug.Break(); // Mette in pausa l'esecuzione
        }

        int pivot = 0;
        
        for(int i = 0; i < percentages.Count; i++) {

            if(i == 0) {
                intervalEnvironment.Add(
                    new Vector2Int(pivot, (int)(maxStepHyperparameter * percentages[i]))
                );
                
            } else {
                intervalEnvironment.Add(
                    new Vector2Int(pivot + 1, pivot + (int)(maxStepHyperparameter * percentages[i]))
                );
            }

            pivot = pivot + (int)(maxStepHyperparameter * percentages[i]);
        }

    }
    public static int EnvironmentToLoad(int step) {

        int result = -1;
        for(int i = 0; i < intervalEnvironment.Count; i++) {

            if(Enumerable.Range(intervalEnvironment[i].x, intervalEnvironment[i].y).Contains(step)) {
                result = i; 
            }
        }

        if(result == -1) {
            Debug.LogError("Errore: nessun ambiente trovato per lo step: " +  step); 
        }

        return result;
    }
}
