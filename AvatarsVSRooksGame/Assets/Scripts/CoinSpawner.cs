using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Configuración de Monedas")]
    [SerializeField] private GameObject coin25Prefab;
    [SerializeField] private GameObject coin50Prefab;
    [SerializeField] private GameObject coin100Prefab;
    
    [Header("Configuración de Spawn")]
    [SerializeField] private float spawnInterval = 5f; // Cada 5 segundos
    [SerializeField] private Transform containerMatrixParent; // Asignar "ContainerMatrix" desde el inspector
    
    private List<RookContainer> allContainers;
    private List<RookContainer> occupiedByCoins;
    
    void Start()
    {
        InitializeContainers();
        StartCoroutine(SpawnCoinsRoutine());
    }
    
    void InitializeContainers()
    {
        allContainers = new List<RookContainer>();
        occupiedByCoins = new List<RookContainer>();
        
        // Obtener todos los RookContainers hijos de ContainerMatrix
        if (containerMatrixParent != null)
        {
            RookContainer[] containers = containerMatrixParent.GetComponentsInChildren<RookContainer>();
            allContainers.AddRange(containers);
            //Debug.Log($"CoinSpawner: Encontrados {allContainers.Count} RookContainers");
        }
        else
        {
            Debug.LogError("CoinSpawner: No se asignó containerMatrixParent! Asigna 'ContainerMatrix' en el Inspector");
        }
    }
    
    IEnumerator SpawnCoinsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnRandomCoin();
        }
    }
    
    void SpawnRandomCoin()
    {
        // Filtrar solo casillas vacías (sin torres Y sin monedas)
        List<RookContainer> availableContainers = new List<RookContainer>();
        
        foreach (RookContainer container in allContainers)
        {
            // Verificar que no tenga torre Y que no tenga moneda
            if (!container.filled && !occupiedByCoins.Contains(container))
            {
                availableContainers.Add(container);
            }
        }
        
        if (availableContainers.Count == 0)
        {
            Debug.LogWarning("CoinSpawner: No hay casillas disponibles para spawn");
            return;
        }
        
        // Seleccionar casilla aleatoria
        int randomIndex = Random.Range(0, availableContainers.Count);
        RookContainer selectedContainer = availableContainers[randomIndex];
        
        // Seleccionar tipo de moneda (probabilidad 1/3 cada una)
        GameObject coinPrefab = SelectRandomCoinType();
        
        if (coinPrefab == null)
        {
            Debug.LogError("CoinSpawner: No se asignaron los prefabs de monedas!");
            return;
        }
        
        // Instanciar la moneda como hijo del contenedor
        GameObject coin = Instantiate(coinPrefab, selectedContainer.transform);
        coin.transform.localPosition = new Vector3(0, 0, -1);
        
        // Configurar el comportamiento de la moneda
        CoinBehavior coinBehavior = coin.GetComponent<CoinBehavior>();
        if (coinBehavior != null)
        {
            coinBehavior.SetContainer(selectedContainer, this);
        }
        else
        {
            Debug.LogError("CoinSpawner: El prefab de moneda no tiene el componente CoinBehavior!");
        }
        
        // Marcar contenedor como ocupado por moneda
        occupiedByCoins.Add(selectedContainer);
    }
    
    GameObject SelectRandomCoinType()
    {
        // Probabilidad exacta de 1/3 para cada tipo
        int randomValue = Random.Range(0, 3);
        
        switch (randomValue)
        {
            case 0:
                return coin25Prefab;
            case 1:
                return coin50Prefab;
            case 2:
                return coin100Prefab;
            default:
                return coin25Prefab;
        }
    }
    
    // Método público para que CoinBehavior libere el contenedor
    public void FreeContainer(RookContainer container)
    {
        if (occupiedByCoins.Contains(container))
        {
            occupiedByCoins.Remove(container);
        }
    }
    
    // Métodos públicos para controlar el spawner
    public void PauseSpawning()
    {
        StopAllCoroutines();
    }
    
    public void ResumeSpawning()
    {
        StartCoroutine(SpawnCoinsRoutine());
    }
    
    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }
}