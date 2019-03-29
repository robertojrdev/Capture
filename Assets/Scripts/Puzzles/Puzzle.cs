using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class Puzzle : MonoBehaviour
{
    [SerializeField] private bool useReceiver = false;
    [SerializeField] protected UnityEvent onSolve;
    public bool IsSolved {get; protected set;}

    public UnityEvent OnSolve
    {
        get
        {
            return onSolve;
        }
    }

    public bool UseReceiver
    {
        get
        {
            return useReceiver;
        }

        protected set
        {
            useReceiver = value;
        }
    }

    private void Awake()
    {
        GameManager.puzzles.Add(this);
    }

    public abstract bool IsInPuzzle(Transform player);

    public abstract bool TryToSolve();

    public abstract void PutInReceiver();

    private void OnDisable()
    {
        GameManager.puzzles.Remove(this);
    }
}
