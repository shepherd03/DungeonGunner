
using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class Player:MonoBehaviour
{
    [HideInInspector]public PlayerDetailsSO playerDetails;
    [HideInInspector]public Health health;
    [HideInInspector]public SpriteRenderer spriteRenderer;
    [HideInInspector]public Animator animator;

    private void Awake()
    {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;
        SetHealth();
    }

    private void SetHealth()
    {
        health.StartHealth = playerDetails.playerHealth;
    }
}