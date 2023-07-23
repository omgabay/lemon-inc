using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomerManager : MonoBehaviour
{
    [SerializeField]
    private Vector3[] customerLinePositions;
    [SerializeField]
    private Vector3[] customerOrderContactPositions;
    [SerializeField]
    private int customersInLineCount = 0;
    private int customersAtTableCount = 0;
    [SerializeField]
    private GameObject[] customerPrefabs;
    [SerializeField]
    private Vector3[] spawnPositions;
    [SerializeField]
    private Quaternion[] spawnRotations;
    private GameObject[] customersWaitingInLine;
    private GameObject[] customersWaitingAtTable;

    [SerializeField]
    private bool debugSendCustomerAway = false;
    [SerializeField]
    private GameObject debugSendCustomerAwayGO;

    public void Start()
    {
        customersWaitingInLine = new GameObject[customerLinePositions.Length];
        customersWaitingAtTable = new GameObject[customerOrderContactPositions.Length];
    }

    public void Update()
    {
        if (customersInLineCount < customerLinePositions.Length)
        {
            spawnCustomer();
        }

        if (debugSendCustomerAway)
        {
            SendCustomerAway(debugSendCustomerAwayGO);
        }
    }

    public void OnCustomerSpawnRequest(string[] values)
    {
        if (customersInLineCount < customerLinePositions.Length && values[0] == "Next customer")
        {
            spawnCustomer();
        }
    }

    private void spawnCustomer()
    {
        int customerPrefabIndex = Random.Range(0, customerPrefabs.Length);
        int spawnPointIndex = Random.Range(0, spawnPositions.Length);
        GameObject spawned = Instantiate(customerPrefabs[customerPrefabIndex], spawnPositions[spawnPointIndex], spawnRotations[spawnPointIndex]);
        spawned.GetComponent<NavMeshAgent>().destination = customerLinePositions[customersInLineCount];
        if (customersAtTableCount < customerOrderContactPositions.Length)
        {
            customersWaitingAtTable[customersAtTableCount] = spawned;
            spawned.GetComponent<CustomerNavMesh>().OrderContactPosition = customerOrderContactPositions[customersAtTableCount];
            spawned.GetComponent<CustomerNavMesh>().IsUpNext = true;
            customersAtTableCount++;
        }
        else
        {
            customersWaitingInLine[customersInLineCount] = spawned;
            customersInLineCount++;
        }
    }

    public void SendCustomerAway(GameObject customer)
    {
        for (int i = 0; i < customersWaitingAtTable.Length; i++)
        {
            if (customersWaitingAtTable[i] == customer)
            {
                customer.GetComponent<CustomerNavMesh>().IsDone = true;

                customersWaitingAtTable[i] = customersWaitingInLine[0];
                customersWaitingAtTable[i].GetComponent<CustomerNavMesh>().OrderContactPosition = customerOrderContactPositions[i];
                customersWaitingAtTable[i].GetComponent<CustomerNavMesh>().IsUpNext = true;

                for (int j = 1; j < customersInLineCount; j++)
                {
                    customersWaitingInLine[j].GetComponent<NavMeshAgent>().destination = customerLinePositions[j - 1];
                    customersWaitingInLine[j - 1] = customersWaitingInLine[j];
                }

                customersInLineCount--;

                spawnCustomer();
                debugSendCustomerAway = false;

                break;
            }
        }
    }
}
