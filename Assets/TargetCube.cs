using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCube : MonoBehaviour
{
    public GameObject cube;
    public InteractiveSelect indicator;
    public Renderer cubeRender;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColor(Color color)
    {
        // changes the color of the cube
        cubeRender.material.color = color;
    }

    public void ChangeSize(bool isConfirmed)
    {
        float baseScale = 0.06f;
        float scale = isConfirmed ? baseScale * 1.2f : baseScale;
        // the current selected cube is 1.2 times the size of the normal cube as a visual cue.
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void StartWobbing(Vector3 targetCube)
    {
        // passed in the target cube which is the cube that's supposed to be the next target.
        // this is so that the wobbing direction is not set to only "left and right" but also takes in account of the vertical displacement
        // works better when one cube might be put further away from the other cubes
        // instead of making the whole gameobject move, only the cube will move, which is a son of this gameobject
        // this is so that the gameobject can always stay in the relative same position.
        Vector3 norm = Vector3.Normalize(targetCube - transform.position); 
    }

    public void TextSelected(bool isConfirmed)
    {
        if (isConfirmed)
        {
            indicator.Follow(true, true);
        }
        else
        {
            indicator.Follow(true);
        }
    }

    public void TextDeselected()
    {
        indicator.NoTarget();
    }

    public bool TextExist()
    {
        return indicator != null;
    }

    IEnumerator Bobbing(Vector3 dir)
    {
        Vector3 startPos = cube.transform.localPosition;
        float cycle = 2.5f;

        while (true)
        {
            float t = (Mathf.Sin(Time.time * Mathf.PI * 2f / cycle) + 1f) / 2f; // oscillates
            cube.transform.localPosition = startPos + dir * 0.08f * t;
            yield return null;
        }
    }
}
