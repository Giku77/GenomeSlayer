using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [Range(0.02f, 0.3f)] public float flashTime = 0.08f;
    [ColorUsage(true, true)] public Color flashColor = Color.red;
    public float intensity = 1.4f;

    readonly List<Renderer> _renderers = new();
    readonly List<MaterialPropertyBlock> _mpbs = new();
    readonly int _baseColorId = Shader.PropertyToID("_BaseColor");   // URP
    readonly int _colorId = Shader.PropertyToID("_Color");       // Built-in/Legacy
    readonly int _emissId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        GetComponentsInChildren(true, _renderers);
        foreach (var r in _renderers) _mpbs.Add(new MaterialPropertyBlock());
    }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            var r = _renderers[i];
            var mpb = _mpbs[i];
            r.GetPropertyBlock(mpb);

            if (HasProp(r, _baseColorId)) mpb.SetColor(_baseColorId, flashColor);
            else if (HasProp(r, _colorId)) mpb.SetColor(_colorId, flashColor);

            if (HasProp(r, _emissId)) mpb.SetColor(_emissId, flashColor * intensity);

            r.SetPropertyBlock(mpb);
        }

        yield return new WaitForSeconds(flashTime);

        for (int i = 0; i < _renderers.Count; i++)
        {
            _mpbs[i].Clear();
            _renderers[i].SetPropertyBlock(_mpbs[i]);
        }
    }

    static bool HasProp(Renderer r, int id)
    {
        var mats = r.sharedMaterials;
        if (mats == null || mats.Length == 0 || mats[0] == null) return false;
        return mats[0].HasProperty(id);
    }
}
