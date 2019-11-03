using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AssetDownloader : MonoBehaviour {
    public string shapesURL;
    public string materialsURL;
    private GameObject ShapeGO;
    public Text loadingText;
    public Transform spawnPos;

    IEnumerator LoadShapeBundle(string shapeName) {
        while (!Caching.ready){
            yield return null;
        }

        //Begin download
        WWW www = WWW.LoadFromCacheOrDownload (shapesURL, 0);
        yield return www;

        //Load the downloaded bundle
        AssetBundle bundle = www.assetBundle;

        //Load an asset from the loaded bundle
        AssetBundleRequest bundleRequest = bundle.LoadAssetAsync (shapeName, typeof(GameObject));
        yield return bundleRequest;

        //get object
        GameObject obj = bundleRequest.asset as GameObject;

        ShapeGO = Instantiate(obj, spawnPos.position, Quaternion.identity) as GameObject;
        loadingText.text = "";

        bundle.Unload(false);
        www.Dispose();
    }

    IEnumerator LoadMaterialBundle(string materialName) {
        while (!Caching.ready){
            yield return null;
        }

        //Begin download
        WWW www = WWW.LoadFromCacheOrDownload (materialsURL, 0);
        yield return www;

        //Load the downloaded bundle
        AssetBundle bundle = www.assetBundle;

        //Load an asset from the loaded bundle
        AssetBundleRequest bundleRequest = bundle.LoadAssetAsync (materialName, typeof(Material));
        yield return bundleRequest;

        //get object
        Material newMat = bundleRequest.asset as Material;

        ShapeGO.gameObject.GetComponent<MeshRenderer>().material = newMat;

        bundle.Unload(false);
        www.Dispose();
    }

    public void LoadAsset(string shapeName){
        if(ShapeGO){
            Destroy(ShapeGO);
        }

        loadingText.text = "Loading...";
        StartCoroutine(LoadShapeBundle(shapeName));        
    }

    public void LoadMaterial(string materialName){
        if(ShapeGO.GetComponent<MeshRenderer>().material.name != materialName){
            StartCoroutine(LoadMaterialBundle(materialName)); 
        }
    }
}
