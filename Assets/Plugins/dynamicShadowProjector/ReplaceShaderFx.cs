using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceShaderFx : MonoBehaviour
{
	public Camera targetCamera;
	public Shader newShader;

    // Start is called before the first frame update
    void Start()
    {
        targetCamera.SetReplacementShader(newShader, null);
    }

    
}
