using UnityEngine;

public class AssignTextures : MonoBehaviour
{
    public ComputeShader shader;
    public int texResolution = 256;

    Renderer rend;
    RenderTexture outputTexture;

    int KernelHandle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
