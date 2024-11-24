using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Environment : MonoBehaviour {
    [SerializeField] private NavMeshSurface navMeshSurface1;
    [SerializeField] private float areaRadius = 10f; // Raggio per trovare posizioni casuali
    [SerializeField] private float markerLifetime = 2f; // Durata del marker in secondi

    private Vector3? markerPosition; // Posizione del marker corrente

    /// <summary>
    /// Restituisce una posizione casuale valida sulla NavMesh.
    /// </summary>
    public Vector3 GetRandomNavMeshPosition() {
        // Genera un punto casuale all'interno di una sfera
        Vector3 randomDirection = Random.insideUnitSphere * areaRadius;
        randomDirection += navMeshSurface1.gameObject.transform.position; // Usa la posizione della navmeshsurface come centro

        // Cerca una posizione valida sulla NavMesh
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomDirection, out hit, areaRadius, NavMesh.AllAreas)) {
            return hit.position; // Posizione valida trovata
        }

        // Se nessuna posizione valida è trovata, restituisci la posizione del centro
        return transform.position;
    }

    /// <summary>
    /// Mostra un marker temporaneo alla posizione campionata.
    /// </summary>
    public void ShowRandomPoint() {
        Vector3 randomPosition = GetRandomNavMeshPosition();
        StartCoroutine(DisplayMarker(randomPosition));
    }

    /// <summary>
    /// Coroutine per mostrare il marker temporaneamente.
    /// </summary>
    private IEnumerator DisplayMarker(Vector3 position) {
        markerPosition = position; // Imposta la posizione del marker
        yield return new WaitForSeconds(markerLifetime); // Aspetta la durata specificata
        markerPosition = null; // Rimuovi il marker
    }

    /// <summary>
    /// Disegna il marker e l'area della sfera nell'Editor.
    /// </summary>
    private void OnDrawGizmosSelected() {
        // Disegna la sfera verde che rappresenta l'area
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(navMeshSurface1.gameObject.transform.position, areaRadius);

        // Disegna il marker se esiste
        if(markerPosition.HasValue) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(markerPosition.Value, 0.5f); // Marker di raggio 0.5
        }
    }
}
