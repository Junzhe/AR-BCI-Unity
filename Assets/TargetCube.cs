using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCube : MonoBehaviour
{
    public GameObject cube;
    public InteractiveSelect indicator;
    public Renderer cubeRender;
    private Vector3 dir; // this direction is the direction which the cube wobbs
    private float t;
    int testdir; // debug variable

    // Start is called before the first frame update
    void Start()
    {
        dir = Vector3.zero;
        testdir = 0; // debug
    }

    // Update is called once per frame
    void Update()
    {
        t = (Mathf.Sin(Time.time * Mathf.PI * 2f / 0.8f) + 1f) / 2f; // oscillates
        cube.transform.localPosition = dir * 0.15f * t; //actuall line
        debug();
    }

    void debug()
    {
        if (Input.GetMouseButtonDown(0))
        {
            testdir = Random.Range(-1, 2);
        }
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
        // 当开始收到切换的信号时，引用这个函数，可以持续引用，targetcube为下一个要切换的对象的position
        // passed in the target cube which is the cube that's supposed to be the next target.
        // this is so that the wobbing direction is not set to only "left and right" but also takes in account of the vertical displacement
        // works better when one cube might be put further away from the other cubes
        // instead of making the whole gameobject move, only the cube will move, which is a son of this gameobject
        // this is so that the gameobject can always stay in the relative same position.
        dir = Vector3.Normalize(targetCube - transform.position); 
    }

    public void StopWobbing()
    {
        // 当切换的信号持续不足一段时间，导致要取消的时候，引用这个函数，停止方块的摆动
        // 当切换的信号持续了一段时间，完成了切换时，也引用这个函数，停止方块的摆动
        dir = Vector3.zero;
        cube.transform.localPosition = Vector3.zero;
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
}
