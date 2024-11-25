using System.Collections;
using Unity.MLAgents;
using UnityEngine;

public class TrainStatus : MonoBehaviour
{

    void Start() {

        // Avvia la coroutine
        //StartCoroutine(CurriculumLearnLesson());
    }

    IEnumerator CurriculumLearnLesson() {



        Debug.Log("curriculumLearnStep: " + (int)Academy.Instance.EnvironmentParameters.GetWithDefault("my_environment_parameter", 0));

        // Aspetta 10 secondi
        yield return new WaitForSeconds(10f);

        // Chiama sé stesso per continuare la ricorsione
        StartCoroutine(CurriculumLearnLesson());
    }

}
