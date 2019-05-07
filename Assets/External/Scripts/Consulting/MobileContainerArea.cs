using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    [RequireComponent(typeof(ConsultingSetup))]
    public class MobileContainerArea : MonoBehaviour
    {
        // Start is called before the first frame update
        public bool OpponentArea;
        IEnumerator WaitUntilMainInit()
        {
            yield return new WaitUntil(() => MainSceneVariables.boardTransforms != null);
            GetComponent<ConsultingSetup>().enabled = true;
        }
        void Start()
        {
            if (!Settings.Mobile) gameObject.SetActive(false);
            else
            {
                float yPos = OpponentArea ? Screen.height : 0;
                gameObject.transform.position = Camera.main.ScreenToWorldPoint(
                  new Vector3(Screen.width, yPos, 20));
                StartCoroutine(WaitUntilMainInit());
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}