using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BreathMeter : MonoBehaviour {
    public float breath;

    private RectTransform meter;
    private RectTransform bg;
    private float maxWidth;

    private bool isVisible = false;

    // Use this for initialization
    void Start () {
        meter = transform.GetChild(1).GetComponent<RectTransform>();
        bg = transform.GetChild(0).GetComponent<RectTransform>();

        meter.GetComponent<CanvasRenderer>().SetAlpha(0);
        bg.GetComponent<CanvasRenderer>().SetAlpha(0);
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log(breath);


        if (!isVisible && breath < 0.99f) {
            meter.GetComponent<CanvasRenderer>().SetAlpha(1);
            bg.GetComponent<CanvasRenderer>().SetAlpha(1);
            isVisible = true;
        } else if (isVisible && breath > 0.99f) {
            StartCoroutine(hideMeter());
        }

        meter.anchorMax = new Vector2(breath, 1);


        if (Input.GetKey(KeyCode.O))
            breath -= Time.deltaTime;
        if (Input.GetKey(KeyCode.P))
            breath += Time.deltaTime;
    }

    private IEnumerator hideMeter(){
        yield return new WaitForSeconds(1);
        if (breath < 0.99)
            yield break;

        meter.GetComponent<CanvasRenderer>().SetAlpha(0);
        bg.GetComponent<CanvasRenderer>().SetAlpha(0);
        isVisible = false;
    }
}
