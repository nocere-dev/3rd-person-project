using System;
using UnityEngine;

public class SFXManager : MonoBehaviour {
    public static SFXManager instance;

    private void Awake() {
        if (instance == null) instance = this;
    }
}
