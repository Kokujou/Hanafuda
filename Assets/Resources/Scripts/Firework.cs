using System.Collections;
using UnityEngine;

public class Firework : MonoBehaviour
{
    // Use this for initialization
    public GameObject FWExplosion;

    private IEnumerator PlayFirework()
    {
        var FWPrefab = FWExplosion;
        while (true)
        {
            var col = Color.HSVToRGB(Random.Range(0.1f, 1f), 1, .75f);
            var go = Instantiate(FWPrefab, transform);
            go.transform.localPosition = new Vector3(Random.Range(-50, 50), Random.Range(-30, 30), 0);
            var main = go.GetComponent<ParticleSystem>().main;
            main.startColor = col;
            yield return new WaitForSeconds(Random.Range(0f, .8f));
        }
    }

    private void Start()
    {
        StartCoroutine(PlayFirework());
    }

    // Update is called once per frame
    private void Update()
    {
    }
}