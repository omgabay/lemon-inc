using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPink : MonoBehaviour
{
    [SerializeField]
    private GameObject pinkLemonadePrefab;

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Pink Lemonade Syrup"))
        {
            CustomerOrder orderStatus = GetComponent<OrderHolder>().GetOrderStatus();
            orderStatus.SetDrinkType(DrinkType.PINK_LEMONADE);
            Destroy(transform.gameObject);
            GameObject newDrink = Instantiate(pinkLemonadePrefab, transform.position, transform.rotation);
            newDrink.GetComponent<OrderHolder>().SetOrderStatus(orderStatus);
        }
    }
}
