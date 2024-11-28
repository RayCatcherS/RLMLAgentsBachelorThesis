using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;


public class AgentScript : Agent
{
    [Header("Environment Settings")]
    [SerializeField] public EnvironmentController gameEnvironmentController;



    [Header("Agent Settings")]
    [SerializeField] private float agentMoveSpeed;
    [SerializeField] private float agentRotationSpeed;
    [SerializeField] private int maxAgentHealth = 10;
    public int GetMaxAgentHealth() {
        return maxAgentHealth;
    }
    public int GetMaxAgentAmmo() {
        int lesson = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("my_environment_parameter", 0);
        int maxAgentAmmo = 0;

        if (lesson == 0) {
            maxAgentAmmo = 150;
        } else if (lesson == 1) {
            maxAgentAmmo = 90;
        } else if (lesson == 2) {
            maxAgentAmmo = 50;
        } else if (lesson == 3) {
            maxAgentAmmo = 25;
        }

        return maxAgentAmmo;
    }
    private int agentHealth;
    // get vita agente
    public int GetAgentHealth() {
        return agentHealth;
    }

    private int agentAmmo;
    
    

    [Header("Agent Components")]
    [SerializeField] private AgentCannon agentCannon;
    [SerializeField] private RayPerceptionSensorComponent3D environmentRaySensor;

    [Header("Opponent agent components")]
    [SerializeField] private AgentScript opponentAgent;

    [Header("Agent UI")]
    [SerializeField] private Text ammoText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text winMessage;
    [SerializeField] private Text loseMessage;

    //private float distanceReward = 0;


    private BehaviorType behaviorType;

    public override void OnEpisodeBegin() {

        //resetta ammo
        agentAmmo = GetMaxAgentAmmo();
        ammoText.text = agentAmmo.ToString();

        // resetta la vita dell'agente
        agentHealth = maxAgentHealth;
        healthSlider.value = 1;

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

        //Posizione iniziale dell'agente(casuale)
        transform.position = gameEnvironmentController.GetRandPosSampleFromActualEnv() + new Vector3(0, 0.5f, 0);
        // rotazione casuale agente
        transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);


        // azzera la velocità dell'agente
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        // disattiva tutti i colpi di cannone vaganti in scena dell'agente
        agentCannon.DisableAllCannonBall();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(agentCannon.CanShoot());
        sensor.AddObservation(agentHealth / maxAgentHealth); // salute normalizzata
        sensor.AddObservation(agentAmmo / GetMaxAgentAmmo()); // ammo normalizzata

        // rileva direzione agente avversario
        OppositeAgentInformation oppositeAgentInformation = DetectedEnemyAgent();

        // posizione dell'agente 
        sensor.AddObservation(gameObject.transform.localPosition);

        // posizione agente avversario
        sensor.AddObservation(oppositeAgentInformation.agentLocalPosition);


        // direzione dell'agente
        sensor.AddObservation(gameObject.transform.localEulerAngles.y / 360.0f); // rotazione normalizzata

        // direzione dell'agente avversario
        sensor.AddObservation(oppositeAgentInformation.agentDirection / 360.0f);// rotazione normalizzata


        // vita dell'agente avversario
        sensor.AddObservation(opponentAgent.GetAgentHealth() / maxAgentHealth); // vita normalizzata
    }

    public override void OnActionReceived(ActionBuffers actions) {
        // Ottieni gli input
        int moveYRaw = actions.DiscreteActions[1]; // Movimento avanti/indietro
        int moveY = 0;
        int rotateRaw = actions.DiscreteActions[0]; // Rotazione
        int rotate = 0;
        int shootRaw = actions.DiscreteActions[2]; // Sparo



        // Mappa i valori dell'input
        //Debug.Log("shootRaw: " + shootRaw);
        if(shootRaw == 0) {
            
        } else if(shootRaw == 1 && agentCannon.CanShoot()) {

            if(agentAmmo > 0) {
                agentCannon.Shoot(gameObject.GetComponent<BehaviorParameters>().TeamId, this);

                agentAmmo--;
                ammoText.text = agentAmmo.ToString();

                // Penalizza per ogni sparo
                //AddReward(-10 / maxAgentAmmo);

                if(agentAmmo == 0) {
                    gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
                    gameObject.GetComponent<BehaviorParameters>().TeamId,
                    -1f
                    );
                }


            } else {
                gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
                gameObject.GetComponent<BehaviorParameters>().TeamId,
                -1f
                );
            }

        }


        //Debug.Log("move: " + moveYRaw);
        //Debug.Log("rotate: " + rotateRaw);
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


        // Penalizza per il tempo trascorso
        float timePenalty = -1f / MaxStep;
        AddReward(timePenalty);


        // reward rilevamento agente con raycast
        float value = DistanceDetectedTag("goal");
        if(value > 0) {

            //float distRew = (1f - value) / MaxStep;
            float detectingReward = 2f / MaxStep;
            AddReward(detectingReward);

            //distanceReward = distanceReward + distRew;
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        // Maschera l'azione "sparare" (azione 1) se il timer non lo consente
        actionMask.SetActionEnabled(2, 1, agentCannon.CanShoot());
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

        if(Input.GetKey(KeyCode.Space)) {
            discreteActions[2] = 1; 
        } else {
            discreteActions[2] = 0;
        }
        
    }


    // applica danno all'agente, [damageBy] rappresenta l'agente che ha inflitto il danno
    public void DamageAgent(int damagedBy) {
        AddReward(-1f); // penalità per il danno subito
        agentHealth = agentHealth - 1;


        // update ui
        healthSlider.value = ((float)agentHealth / (float)maxAgentHealth);

        // l'agente ha perso
        if(agentHealth == 0) {
            gameEnvironmentController.EndEnvironmentEpisodeWithOneWin(damagedBy);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "wall") {
            gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
            gameObject.GetComponent<BehaviorParameters>().TeamId,
            -1f
            );
            //gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
            //    gameObject.GetComponent<BehaviorParameters>().TeamId
            //);

        } else if(collision.gameObject.tag == "agent") {
            gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
                gameObject.GetComponent<BehaviorParameters>().TeamId,
                -1f
                );
            //AddReward(-1f); // penalità per la collisione con un altro agente
        }
    }

    // restituisce la distanza dal tag rilevato, -1 se non rilevato
    private float DistanceDetectedTag(string tagToDetect) {
        // Accedi al componente Ray Perception Sensor
 
        
        var rayOutputs = RayPerceptionSensor.Perceive(environmentRaySensor.GetRayPerceptionInput()).RayOutputs;
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


    // se c'è un agente nel campo visivo, ritorna la sua direzione(restituisce [0, 360] gradi, -1 se non rilevato)
    private Vector3 lastOppositeAgentPosition = new Vector3(0, 0, 0);
    private float lastOppositeAgentRotation = 0;
    private OppositeAgentInformation DetectedEnemyAgent() {

        var rayOutputs = RayPerceptionSensor.Perceive(environmentRaySensor.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;

        for(int i = 0; i < lengthOfRayOutputs; i++) {
            GameObject agentHitted = rayOutputs[i].HitGameObject;
            if(agentHitted != null) {

                if(agentHitted.gameObject.tag == "agent") {

                    lastOppositeAgentPosition = agentHitted.gameObject.transform.localPosition;
                    lastOppositeAgentRotation = agentHitted.gameObject.transform.localEulerAngles.y;
                    return new OppositeAgentInformation(
                        agentHitted.gameObject.transform.eulerAngles.y,
                        agentHitted.gameObject.transform.position
                    );
                }
            }
        }

        return new OppositeAgentInformation(
            lastOppositeAgentRotation,
            lastOppositeAgentPosition
        );
    }

    public void ShowOutcomeMessage(bool winner) {

        if(winner) {
            winMessage.gameObject.SetActive(true);
        } else {
            loseMessage.gameObject.SetActive(true);
        }
    }
    public void DisableOutcomeMessage() {
        winMessage.gameObject.SetActive(false);
        loseMessage.gameObject.SetActive(false);
    }
}

public class OppositeAgentInformation {
    public float agentDirection = 0;
    public Vector3 agentLocalPosition = Vector3.zero;

    public OppositeAgentInformation(float agentLocalDirection, Vector3 agentLocalPosition) {
        this.agentDirection = agentLocalDirection;
        this.agentLocalPosition = agentLocalPosition;
    }
}
