using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        if (!isVisible && breath < 0.99f) {
            meter.GetComponent<CanvasRenderer>().SetAlpha(1);
            bg.GetComponent<CanvasRenderer>().SetAlpha(1);
            isVisible = true;
        } else if (isVisible && breath > 0.99f) {
            StartCoroutine(hideMeter(1));
        }

        meter.anchorMax = new Vector2(breath, 1);
    }

    // Hide the meter with a X second delay
    private IEnumerator hideMeter(float delay){
        yield return new WaitForSeconds(delay);
        if (breath < 0.99)
            yield break;

        meter.GetComponent<CanvasRenderer>().SetAlpha(0);
        bg.GetComponent<CanvasRenderer>().SetAlpha(0);
        isVisible = false;
    }
}
