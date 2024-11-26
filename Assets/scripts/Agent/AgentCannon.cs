using UnityEngine;

public class AgentCannon : MonoBehaviour {
    [SerializeField] private GameObject canonBallPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int poolSize = 10; // Numero massimo di proiettili nella pool
    [SerializeField] private float shootInterval = 1f; // Intervallo minimo tra i colpi
    private float lastShootTime; // Tempo dell'ultimo sparo
    

    private GameObject[] cannonBallPool; // La pool di proiettili
    private int currentIndex = 0; // Indice per gestire la coda circolare

    void Start() {
        // Inizializza la pool di proiettili
        cannonBallPool = new GameObject[poolSize];
        for(int i = 0; i < poolSize; i++) {
            cannonBallPool[i] = Instantiate(canonBallPrefab);
            cannonBallPool[i].SetActive(false); // Disattiva il proiettile inizialmente
        }
    }


    public void Shoot(int agentId, AgentScript agent) {

        // Aggiorna il tempo dell'ultimo sparo
        lastShootTime = Time.time;

        // Seleziona il proiettile corrente dalla pool
        GameObject projectile = cannonBallPool[currentIndex];

        // Aggiorna l'indice in modo circolare
        currentIndex = (currentIndex + 1) % poolSize;

        // Configura il proiettile
        projectile.GetComponent<CannonBall>().agentScript = agent;
        projectile.transform.position = spawnPoint.position;
        projectile.transform.rotation = spawnPoint.rotation;
        projectile.SetActive(true);
    }

    public void DisableAllCannonBall() {
        // Disattiva tutti i proiettili nella pool
        for(int i = 0; i < poolSize; i++) {
            cannonBallPool[i].SetActive(false);
        }
    }

    public bool CanShoot() {
        return Time.time - lastShootTime >= shootInterval;
    }
}
