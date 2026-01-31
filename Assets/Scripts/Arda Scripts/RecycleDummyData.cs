using UnityEngine;

[CreateAssetMenu(menuName = "Recycle/Dummy Data")]
public class RecycleDummyData : ScriptableObject
{
    public string dummyName;
    public Sprite dummySprite;
    public float maxHealth;
    public GameObject dummyPrefab;
}