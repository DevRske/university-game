using System;
using UnityEngine;

public class RoundStart : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roundManager.StartRound();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
