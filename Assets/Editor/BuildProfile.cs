using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "BuildProfile", menuName = "Scriptable Objects/BuildProfile")]
public class BuildProfile : ScriptableObject
{
    public string outputPath = "Builds/WebGL";
    public BuildTarget targetPlatform = BuildTarget.WebGL;
    public WebGLCompressionFormat compressionFormat = WebGLCompressionFormat.Disabled;
}