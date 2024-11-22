using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TestAgentScript : Agent
{
    [SerializeField] private Transform objectiveTransform;
    [SerializeField] private float agentMoveSpeed;
    [SerializeField] private float agentRotationSpeed;
    

    [SerializeField] private Material winMateriall;
    [SerializeField] private Material loseMateriall;
    [SerializeField] private Material defaultMateriall;
    [SerializeField] private Renderer floorRenderer;

    private float distanceReward = 0;
    public override void OnEpisodeBegin() {
        distanceReward = 0;
        //Posizione iniziale dell'agente(casuale)
        transform.localPosition = new Vector3(
            Random.Range(-24f, 24f),
            0,
            Random.Range(-24f, 24f)
            );
        //Posizione iniziale dell goal(casuale)
        objectiveTransform.localPosition = new Vector3(
            Random.Range(-24f, 24f),
            0,
            Random.Range(-24f, 24f)
            );
    }

    public override void CollectObservations(VectorSensor sensor) {
        //sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(objectiveTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        // Ottieni gli input
        float rotate = actions.ContinuousActions[0]; // Rotazione
        float moveY = actions.ContinuousActions[1]; // Movimento avanti/indietro

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


        float timePenalty = -1f / MaxStep;
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
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        
        continousActions[0] = Input.GetAxisRaw("Horizontal");
        continousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other) {
        
        if(other.gameObject.tag == "goal") {
            AddReward(10f); //Incrementa il reward
            StartCoroutine(PrintMessageRepeatedly(false));
            EndEpisode();
        } else if(other.gameObject.tag == "wall") {
            AddReward(-10f); //Incrementa il reward
            
            StartCoroutine(PrintMessageRepeatedly(true));
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

    IEnumerator PrintMessageRepeatedly(bool wrongEnd) {
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
