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


        if(behaviorType == BehaviorType.InferenceOnly || behaviorType == BehaviorType.HeuristicOnly || gameEnvironmentController.curriculumLearningDisabled) {
            maxAgentAmmo = 15;
        } else {
            if(lesson == 0) {
                maxAgentAmmo = 3;
            } else if(lesson == 1) {
                maxAgentAmmo = 6;
            } else if(lesson == 2) {
                maxAgentAmmo = 10;
            } else if(lesson == 3) {
                maxAgentAmmo = 15;
            }
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
    private Vector2 lastOppositeAgentPosition = new Vector2(0, 0);


    private BehaviorType behaviorType;

    public override void OnEpisodeBegin() {
        // carica environment in base al behaviour type
        behaviorType = GetComponent<BehaviorParameters>().BehaviorType;

        // resetta osservazioni
        lastOppositeAgentPosition = new Vector2(0, 0);


        //resetta ammo
        agentAmmo = GetMaxAgentAmmo();
        ammoText.text = agentAmmo.ToString();

        // resetta la vita dell'agente
        agentHealth = maxAgentHealth;
        healthSlider.value = 1;


        if(behaviorType == BehaviorType.InferenceOnly || behaviorType == BehaviorType.HeuristicOnly || gameEnvironmentController.curriculumLearningDisabled) {
            gameEnvironmentController.LoadEnvironment(3);

        } else {
            // carica l'ambiente in base allo stato del curriculum learning
            gameEnvironmentController.LoadEnvironment(
                (int)Academy.Instance.EnvironmentParameters.GetWithDefault("my_environment_parameter", 0)
            );
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
        //sensor.AddObservation(agentCannon.CanShoot());
        //sensor.AddObservation(agentHealth / maxAgentHealth); // salute normalizzata
        sensor.AddObservation(agentAmmo / GetMaxAgentAmmo()); // ammo agente normalizzata



        // se si reintroducono le posizioni !RICORDARSI DI RESETTARLE NELLA FUNZIONE! OnEpisodeBegin()


        // rileva direzione agente avversario
        OppositeAgentInformation oppositeAgentInformation = DetectedEnemyAgent();


        // posizione degli agenti
        sensor.AddObservation(new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.z));
        sensor.AddObservation(oppositeAgentInformation.agentLocalPosition);


        
        sensor.AddObservation(gameObject.transform.localEulerAngles.y / 360.0f); // direzione agente normalizzata
        sensor.AddObservation(oppositeAgentInformation.agentDirection / 360.0f); // ultima direzione nemico normalizzata


        // ultima velocity dell'agente avversario
        sensor.AddObservation(oppositeAgentInformation.agentVelocity);

        // velocity dell'agente
        sensor.AddObservation(gameObject.GetComponent<Rigidbody>().velocity);


        // salue dell'agente avversario normalizzata
        //sensor.AddObservation(opponentAgent.GetAgentHealth() / maxAgentHealth); 
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
        OppositeAgentInformation oppositeAgentInformation = DetectedEnemyAgent();
        if(oppositeAgentInformation.agentDirection != -1) {
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







        if(gameObject.GetComponent<BehaviorParameters>().TeamId == 0) {
            if(Input.GetKey(KeyCode.W)) {
                discreteActions[1] = 1;
            } else if(Input.GetKey(KeyCode.S)) {
                discreteActions[1] = 2;
            } else {
                discreteActions[1] = 0;
            }

            if(Input.GetKey(KeyCode.D)) {
                discreteActions[0] = 1;
            } else if(Input.GetKey(KeyCode.A)) {
                discreteActions[0] = 2;
            } else {
                discreteActions[0] = 0;
            }

            if(Input.GetKey(KeyCode.Space)) {
                discreteActions[2] = 1;
            } else {
                discreteActions[2] = 0;
            }
        }


        if(gameObject.GetComponent<BehaviorParameters>().TeamId == 1) {
            if(Input.GetKey(KeyCode.UpArrow)) {
                discreteActions[1] = 1;
            } else if(Input.GetKey(KeyCode.DownArrow)) {
                discreteActions[1] = 2;
            } else {
                discreteActions[1] = 0;
            }

            if(Input.GetKey(KeyCode.RightArrow)) {
                discreteActions[0] = 1;
            } else if(Input.GetKey(KeyCode.LeftArrow)) {
                discreteActions[0] = 2;
            } else {
                discreteActions[0] = 0;
            }

            if(Input.GetKey(KeyCode.LeftShift)) {
                discreteActions[2] = 1;
            } else {
                discreteActions[2] = 0;
            }
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

            // penalità per la collisione con un muro
            if(!gameEnvironmentController.wallMatchFailDisabled) {
                gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
            gameObject.GetComponent<BehaviorParameters>().TeamId,
            -10f
                );
            }

        } else if(collision.gameObject.tag == "agent") {
            // penalità per la collisione con un altro agente
            gameEnvironmentController.EndEnvironmentEpisodeWithOneLose(
                gameObject.GetComponent<BehaviorParameters>().TeamId,
                -10f
                );
        }
    }

    // restituisce la distanza dal tag rilevato, -1 se non rilevato
    /*private float DistanceDetectedTag(string tagToDetect) {
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
    }*/


    // se c'è un agente nel campo visivo, ritorna la sua direzione(restituisce [0, 360] gradi, -1 se non rilevato)
    private OppositeAgentInformation DetectedEnemyAgent() {

        var rayOutputs = RayPerceptionSensor.Perceive(environmentRaySensor.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;

        for(int i = 0; i < lengthOfRayOutputs; i++) {
            GameObject agentHitted = rayOutputs[i].HitGameObject;
            if(agentHitted != null) {

                if(agentHitted.gameObject.tag == "agent") {

                    lastOppositeAgentPosition = new Vector2(agentHitted.gameObject.transform.localPosition.x, agentHitted.gameObject.transform.localPosition.z);
                    return new OppositeAgentInformation(
                        agentHitted.gameObject.transform.eulerAngles.y,
                        new Vector2(agentHitted.gameObject.transform.localPosition.x, agentHitted.gameObject.transform.localPosition.z),
                        new Vector2(agentHitted.GetComponent<Rigidbody>().velocity.x, agentHitted.GetComponent<Rigidbody>().velocity.z)
                    );
                }
            }
        }

        return new OppositeAgentInformation(
            -1,
            lastOppositeAgentPosition,
            Vector2.zero
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
    public Vector2 agentLocalPosition = Vector2.zero;
    public Vector2 agentVelocity = Vector2.zero;


    public OppositeAgentInformation(float agentLocalDir, Vector2 agentLocalPos, Vector2 agentVel) {
        this.agentDirection = agentLocalDir;
        this.agentLocalPosition = agentLocalPos;
        this.agentVelocity = agentVel;
    }
}
