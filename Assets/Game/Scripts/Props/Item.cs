using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class Item : MonoBehaviour {

    //Parent of interactable classes
    public bool interactable = true;

    //Callback for when the player presses the interaction button 
    public virtual void Interact(Player player) { }

    //Message to display when the player looks at the object
    public virtual string Message() { return ""; }
}
