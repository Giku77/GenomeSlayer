using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }
    void Interact(Player player);
}
