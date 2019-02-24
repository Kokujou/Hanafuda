using Hanafuda;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Klasse für Runden- und/oder Spielende
/// </summary>
public class Finish : MonoBehaviour
{
    public GameObject MobilePrefab;
    public GameObject PCPrefab;

    private void Start()
    {
        if (Settings.Mobile)
            Instantiate(MobilePrefab);
        else
            Instantiate(PCPrefab);
    }

}