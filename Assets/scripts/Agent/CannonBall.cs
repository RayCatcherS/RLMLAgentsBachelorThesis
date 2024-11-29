using Unity.MLAgents.Policies;
using UnityEngine;

public class CannonBall : MonoBehaviour {
    [SerializeField] public AgentScript agentScript; // Riferimento all'agente che ha sparato il proiettile
    [SerializeField] private int speed = 10; // Velocità del proiettile
    [SerializeField] private int lifetime = 15; // Durata massima del proiettile

    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable() {
        // Cancella eventuali timer pendenti per evitare disattivazioni premature
        CancelInvoke(nameof(DisableProjectile));

        // Avvia un nuovo timer per disattivare il proiettile
        Invoke(nameof(DisableProjectile), lifetime);
    }

    void OnDisable() {
        // Cancella il timer quando il proiettile viene disabilitato
        CancelInvoke(nameof(DisableProjectile));
    }

    void FixedUpdate() {
        // Movimento costante del proiettile
        Vector3 moveDirection = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    void OnTriggerEnter(Collider other) {
        //Debug.Log($"Proiettile colpito: {other.gameObject.name}");

        // Se il proiettile colpisce un agente
        if(other.CompareTag("agent")) {
            AgentScript hittedAgent = other.GetComponent<AgentScript>();

            if(other.GetComponent<BehaviorParameters>().TeamId == agentScript.gameObject.GetComponent<BehaviorParameters>().TeamId) {
                // Se il proiettile colpisce l'agente che lo ha sparato, non fare nulla

            } else {
                // Altrimenti, applica una penalità all'agente colpito
                hittedAgent.DamageAgent(agentScript.gameObject.GetComponent<BehaviorParameters>().TeamId);

                // Applica una ricompensa all'agente che ha sparato il proiettile
                agentScript.AddReward(10);

                Debug.Log("colpito");
                // Disattiva il proiettile
                gameObject.SetActive(false);


                // se l'agente è stato colpito termina episodio
                agentScript.gameEnvironmentController.EndEnvironemntEpisode();
            }

        } else if(other.CompareTag("cannonBall")) {

            if(other.GetComponent<CannonBall>().agentScript.gameObject.GetComponent<BehaviorParameters>().TeamId == 
                agentScript.gameObject.GetComponent<BehaviorParameters>().TeamId) {

                // Se il proiettile colpisce un proiettile dello stesso agente, non fare nulla
            } else {
                // Disattiva il proiettile
                gameObject.SetActive(false);
            }
        } else {

            // applica penalità per colpo errato
            agentScript.AddReward(-10/ agentScript.GetMaxAgentAmmo());
            

            // Disattiva il proiettile
            gameObject.SetActive(false);
        }
    }

    private void DisableProjectile() {
        gameObject.SetActive(false); // Disattiva il proiettile
    }
}
