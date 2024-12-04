using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

public class EnvironmentController : MonoBehaviour {

    [Header("Environment override")]
    public bool curriculumLearningDisabled = false;
    public bool wallMatchFailDisabled = false;

    [Header("Environments")]
    [SerializeField] List<GameObject> environments = new List<GameObject>();
    [SerializeField] private AgentScript [] agents; 

    private int currentEnvironmentIndex = 0;

    [Header("Outcome Materials")]
    [SerializeField] private Material winMateriall;
    [SerializeField] private Material loseMateriall;
    [SerializeField] private Material defaultMateriall;
    [SerializeField] private Renderer floorRenderer;

    public void Start() {
        for(int i = 0; i < environments.Count; i++) {
            environments[i].SetActive(false);
        }

        // vettore agenti nell'environment
        agents = GetComponentsInChildren<AgentScript>();
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

    public void EndEnvironmentEpisodeWithOneWin(int winningAgent) {

        // premia l'agente vincitore e penalizza gli altri(perdenti)
        for(int i = 0; i < agents.Length; i++) {

            if(agents[i].gameObject.GetComponent<BehaviorParameters>().TeamId == winningAgent) {
                agents[i].AddReward(10);
            } //else {
              //  agents[i].AddReward(-10f);
            //}
        }


        // termina episodio per tutti gli agenti
        for(int i = 0; i < agents.Length; i++) {
            agents[i].EndEpisode();
        }



        // Stampa l'esito dell'episodio
        ViewEpisodeOutcome(false);
        ViewWinningAgent(winningAgent);
    }

    public void EndEnvironmentEpisodeWithOneLose(int loserAgent, float penalty) {

        // penalizza solo l'agente perdente
        for(int i = 0; i < agents.Length; i++) {

            if(agents[i].gameObject.GetComponent<BehaviorParameters>().TeamId == loserAgent) {
                agents[i].AddReward(-10);
            }
        }

        // termina episodio per tutti gli agenti
        for(int i = 0; i < agents.Length; i++) {
            agents[i].EndEpisode();
        }

        // Stampa l'esito dell'episodio
        ViewEpisodeOutcome(true);
    }

    public void EndEnvironemntEpisode() {
        // termina episodio per tutti gli agenti
        for(int i = 0; i < agents.Length; i++) {
            agents[i].EndEpisode();
        }

        // Stampa l'esito dell'episodio
        ViewEpisodeOutcome(false);
    }

    // visualizza esito episodio
    public void ViewEpisodeOutcome(bool wrongEnd) {
        StartCoroutine(EpisodeOutcome(wrongEnd));
    }
    private IEnumerator EpisodeOutcome(bool wrongEnd) {
        if(wrongEnd) {
            floorRenderer.material = loseMateriall;
            yield return new WaitForSeconds(1f);
            floorRenderer.material = defaultMateriall;
        } else {
            floorRenderer.material = winMateriall;
            yield return new WaitForSeconds(1f);
            floorRenderer.material = defaultMateriall;
        }

    }



    // visualizza agente vincitore
    public void ViewWinningAgent(int winningAgent) {
        StartCoroutine(WinningAgent(winningAgent));
    }
    private IEnumerator WinningAgent(int winningAgent) {

        for(int i = 0; i < agents.Length;i++) {
            if(agents[i].gameObject.GetComponent<BehaviorParameters>().TeamId == winningAgent) {

                agents[i].ShowOutcomeMessage(true);
            } else {
                agents[i].ShowOutcomeMessage(false);
            }
            
        }

        yield return new WaitForSeconds(1f);

        for(int i = 0; i < agents.Length; i++) {
            agents[i].DisableOutcomeMessage();
        }
        
    }
}
