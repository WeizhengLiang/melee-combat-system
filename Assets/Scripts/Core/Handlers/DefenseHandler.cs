using System;
using UnityEngine;

public class DefenseHandler : MonoBehaviour {
    public event Action OnDefendStart;
    public event Action OnDefendEnd;

    public bool IsDefending { get; private set; }

    public void StartDefend() {
        IsDefending = true;
        OnDefendStart?.Invoke();
    }

    public void EndDefend() {
        IsDefending = false;
        OnDefendEnd?.Invoke();
    }
}