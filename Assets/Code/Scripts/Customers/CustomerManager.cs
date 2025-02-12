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
    private List<string> customerAvailableMaleNames = new List<string>();
    [SerializeField]
    private List<string> customerAvailableFemaleNames = new List<string>();

    public void Start()
    {
        customersWaitingInLine = new GameObject[customerLinePositions.Length];
        customersWaitingAtTable = new GameObject[customerOrderContactPositions.Length];
        StartCoroutine("CustomerSpawnManager"); // 1 second
    }

    IEnumerator CustomerSpawnManager()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            if (customersInLineCount < customerLinePositions.Length)
            {
                spawnCustomer();
            }
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

        Customer customerData = spawned.GetComponentInChildren<Customer>();
        Customer.Gender gender = customerData.CustomerGender;
        string name;
        if (gender == Customer.Gender.Male)
        {
            int index = Random.Range(0, customerAvailableMaleNames.Count);
            name = customerAvailableMaleNames[index];
            customerAvailableMaleNames.RemoveAt(index);
        }
        else
        {
            int index = Random.Range(0, customerAvailableFemaleNames.Count);
            name = customerAvailableFemaleNames[index];
            customerAvailableFemaleNames.RemoveAt(index);
        }
        customerData.CustomerName = name;

        CustomerMessages customerMessages = spawned.GetComponentInChildren<CustomerMessages>();
        customerMessages.SetMessages(customerData.GetMessages());

        spawned.GetComponent<NavMeshAgent>().destination = customerLinePositions[customersInLineCount];
        if (customersAtTableCount < customerOrderContactPositions.Length)
        {
            customersWaitingAtTable[customersAtTableCount] = spawned;
            Debug.Log(string.Format("{0} joined the table :)", customerData.CustomerName));
            spawned.GetComponent<CustomerNavMesh>().OrderContactPosition = customerOrderContactPositions[customersAtTableCount];
            spawned.GetComponent<CustomerNavMesh>().IsUpNext = true;
            customerMessages.ResetMessages();
            customersAtTableCount++;
        }
        else
        {
            customersWaitingInLine[customersInLineCount] = spawned;
            customersInLineCount++;
        }
    }

    public void AcceptOrderByCustomerName(string customerName)
    {
        Debug.Log("Trying to send customer away - " + customerName);

        foreach (GameObject customer in customersWaitingAtTable)
        {
            Customer customerObject = customer.GetComponentInChildren<Customer>(); 
            if (customerObject.CustomerName == customerName)
            {
                //TODO Should uncomment this line - sending customer away only when order is present. 
                customerObject.AttemptToTakeOrder();
                Debug.Log("Sending customer away - " + customerName);
                break;
            }
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

                Customer customerData = customer.GetComponentInChildren<Customer>();
            
                if (customerData.CustomerGender == Customer.Gender.Male)
                {
                    customerAvailableMaleNames.Add(customerData.CustomerName);
                }
                else
                {
                    customerAvailableFemaleNames.Add(customerData.CustomerName);
                }

                break;
            }
        }
    }

}
