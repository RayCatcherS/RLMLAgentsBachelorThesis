using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class AgentScript : Agent
{

    [SerializeField] private EnvironmentController gameEnvironmentController;
    

    [SerializeField] private Transform objectiveTransform;
    [SerializeField] private float agentMoveSpeed;
    [SerializeField] private float agentRotationSpeed;
    

    [SerializeField] private Material winMateriall;
    [SerializeField] private Material loseMateriall;
    [SerializeField] private Material defaultMateriall;
    [SerializeField] private Renderer floorRenderer;

    private float distanceReward = 0;


    private BehaviorType behaviorType;

    public override void OnEpisodeBegin() {

        // carica environment in base al behaviour type
        behaviorType = GetComponent<BehaviorParameters>().BehaviorType;
        
        switch(behaviorType) {
            case BehaviorType.InferenceOnly:
                gameEnvironmentController.LoadEnvironment(
                    3
                );
            break;

            case BehaviorType.HeuristicOnly:
                gameEnvironmentController.LoadEnvironment(
                    3
                );
            break;

            case BehaviorType.Default:
                // carica l'ambiente in base allo stato del curriculum learning
                gameEnvironmentController.LoadEnvironment(
                    (int)Academy.Instance.EnvironmentParameters.GetWithDefault("my_environment_parameter", 0)
                );
            break;
        }

       

        //reset distance
        distanceReward = 0;

        //Posizione iniziale dell'agente(casuale)
        transform.position = gameEnvironmentController.GetRandPosSampleFromActualEnv() + new Vector3(0, 0.5f, 0);
        // rotazione casuale agente
        transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);

        //Posizione iniziale dell goal(casuale)
        objectiveTransform.position = gameEnvironmentController.GetRandPosSampleFromActualEnv() + new Vector3(0, 0.5f, 0); ;
        // rotazione casuale goal
        objectiveTransform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);


        // azzera la velocità dell'agente
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


    }

    public override void CollectObservations(VectorSensor sensor) {
        //sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(objectiveTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        // Ottieni gli input
        int moveYRaw = actions.DiscreteActions[1]; // Movimento avanti/indietro
        int moveY = 0;
        int rotateRaw = actions.DiscreteActions[0]; // Rotazione
        int rotate = 0;

        if(rotateRaw == 0) {
            rotate = 0;
        } else if(rotateRaw == 1) {
            rotate = 1;
        } else if(rotateRaw == 2) {
            rotate = -1;
        }

        if(moveYRaw == 0) {
            moveY = 0;
        } else if(moveYRaw == 1) {
            moveY = 1;
        } else if(moveYRaw == 2) {
            moveY = -1;
        }

        // Calcola la direzione del movimento e della rotazione
        Vector3 dirToGo = transform.forward * moveY;
        Vector3 rotateDir = transform.up * rotate;

        // Applica la rotazione
        if(moveY > 0 || moveY == 0) {
            transform.Rotate(rotateDir, Time.deltaTime * agentRotationSpeed);
        } else { // in retromarcia viene applicata una rotazione inversa
            transform.Rotate(rotateDir, Time.deltaTime * agentRotationSpeed * -1);
        }
        

        // Applica il movimento basato su AddForce
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.AddForce(dirToGo * agentMoveSpeed, ForceMode.VelocityChange);


        float timePenalty = -4f / MaxStep;
        AddReward(timePenalty);


        // Rilevamento del goal con raycast
        float distance = DistanceDetectedTag("goal");
        if(distance > 0) {

            float distRew = (1f - distance) / MaxStep;
            AddReward(distRew);

            distanceReward = distanceReward + distRew;
        }
    }


    // Funzione per testare il movimento dell'agente, sovrascrivendo le azioni del modello con quelle genrate da un input(tastiera)
    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        if((int)Input.GetAxisRaw("Horizontal") == 0) {
            discreteActions[0] = 0;
        } else if((int)Input.GetAxisRaw("Horizontal") == 1) {
            discreteActions[0] = 1;
        } else if((int)Input.GetAxisRaw("Horizontal") == -1) {
            discreteActions[0] = 2;
        }

        if((int)Input.GetAxisRaw("Vertical") == 0) {
            discreteActions[1] = 0;
        } else if((int)Input.GetAxisRaw("Vertical") == 1) {
            discreteActions[1] = 1;
        } else if ((int)Input.GetAxisRaw("Vertical") == -1) {
            discreteActions[1] = 2;
        }
    }

    private void OnTriggerEnter(Collider other) {
        
        if(other.gameObject.tag == "goal") {
            AddReward(10f); //Incrementa il reward
            StartCoroutine(PrintEpisodeOutcome(false));
            EndEpisode();
        } else if(other.gameObject.tag == "wall") {
            AddReward(-10f); //Incrementa il reward
            
            StartCoroutine(PrintEpisodeOutcome(true));
            EndEpisode();
        } else {
            Debug.Log("Error, collision with:" + other.gameObject.tag.ToString());
        }
        
        
    }

    private float DistanceDetectedTag(string tagToDetect) {
        // Accedi al componente Ray Perception Sensor
        RayPerceptionSensorComponent3D sensor = GetComponentInChildren<RayPerceptionSensorComponent3D>();
 
        
        var rayOutputs = RayPerceptionSensor.Perceive(sensor.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;

        for(int i = 0; i < lengthOfRayOutputs; i++) {
            GameObject goHit = rayOutputs[i].HitGameObject;
            if(goHit != null) {

                if(rayOutputs[i].HitGameObject.gameObject.tag == tagToDetect) {
                    return rayOutputs[i].HitFraction;
                }
            }
        }

        return -1;
    }

    IEnumerator PrintEpisodeOutcome(bool wrongEnd) {
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
}
