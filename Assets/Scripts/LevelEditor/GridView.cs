using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridView : MonoBehaviour
{
    private Camera _camera;
    private Material _material;

    public Grid grid;

    private void Awake()
    {
    }

    private void Update()
    {
        if (!grid) return;

        GetCamera();
        GetShader();
        SetShaderValues();

        var z = 10;

        transform.position = _camera.transform.position + Vector3.forward * z;
        transform.LookAt(_camera.transform);
        transform.Rotate(Vector3.right * 180, Space.Self);

        var screen = _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, z));

        var distance = screen - transform.position;

        transform.localScale = new Vector3(distance.x * 2, distance.y * 2, 1);
    }

    protected void GetCamera()
    {
        if (_camera != null) return;

        _camera = Camera.main;
    }

    private void GetShader()
    {
        if (_material != null) return;

        _material = GetComponent<Renderer>().sharedMaterial;
    }

    private void SetShaderValues()
    {
        _material.SetVector("_GridSize",grid.cellSize);
    }
}