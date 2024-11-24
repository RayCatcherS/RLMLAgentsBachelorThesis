using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField] List<GameObject> environments = new List<GameObject>();
    private int currentEnvironmentIndex = 0;
    

    public void Start() {
        for(int i = 0; i < environments.Count; i++) {
            environments[i].SetActive(false);
        }
    }
    public void LoadEnvironment(int index) {
        for(int i = 0; i < environments.Count; i++) {
            environments[i].SetActive(false);
        }

        environments[index].SetActive(true);
        currentEnvironmentIndex = index;
    }


    public Vector3 GetRandPosSampleFromActualEnv() {
        
        return environments[currentEnvironmentIndex].GetComponent<Environment>().GetRandomNavMeshPosition();
    }
}
