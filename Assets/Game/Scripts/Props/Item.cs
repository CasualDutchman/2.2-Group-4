using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    public bool interactable = true;

    public virtual void Interact(Player player) { }

    public virtual string Message() { return ""; }
}
