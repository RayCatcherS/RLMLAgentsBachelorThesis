using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField] List<GameObject> environments = new List<GameObject>();
    

    public void Start() {
        for(int i = 0; i < environments.Count; i++) {
            environments[i].SetActive(false);
        }
    }
    public void LoadEnvironment(int index) {
        environments[index].SetActive(true);
    }


    public Vector3 GetRandomPositionSample() {
        return new Vector3();
    }
}
