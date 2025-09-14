using UnityEngine;

public class Fruit : MonoBehaviour, IInteractable
{
    public TreeEntity ownerTree;

    public string Prompt => "[F] Harvest";
    public void Interact(Player player)
    {
        ownerTree?.OnFruitHarvested();
        Destroy(gameObject);
    }
}
