using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PlaceSearch : MonoBehaviour
{
    private Button _search;
    private InputField _searchField;
    public string apiKey = "AIzaSyCRKHlpmFQ1SRnIX465zxw9gslA6T7UiM8";
    public string apiUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json?";

    private void Awake()
    {
        _search = transform.Find("").GetComponent<Button>();
        _searchField = transform.Find("").GetComponent<InputField>();
    }


    void Start()
    {
        _search.onClick.AddListener(OnClickSearch);
    }

    private void OnClickSearch()
    {
        StartCoroutine(SearchPlace(_searchField.text));
    }

    IEnumerator SearchPlace(string keyword)
    {
        string url = $"{apiUrl}query={keyword}&key={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            Vector2 ParseDestination(string json)
            {
                JObject jsonResponse = JObject.Parse(json);
                JToken location = jsonResponse["results"][0]["geometry"]["location"];
                float lat = location["lat"].Value<float>();
                float lng = location["lng"].Value<float>();
                return new Vector2(lat, lng);
            }
        }
    }
}
