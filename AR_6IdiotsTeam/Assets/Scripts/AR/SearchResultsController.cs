using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchResultsController : MonoBehaviour
{
    [SerializeField] private GameObject _searchResultButtonPrefab;
    [SerializeField] private Transform _searchResultsParent;
    [SerializeField] private ARMapController _arMapController;

    private List<GameObject> _searchResultObjects = new List<GameObject>();


    private void Awake()
    {
        _searchResultsParent = transform.Find("Map/Scroll View/Viewport/Content").transform;
    }

    public void DisplaySearchResults(List<SearchResult> results)
    {
        ClearSearchResults();
        foreach (var result in results)
        {
            GameObject resultObject = Instantiate(_searchResultButtonPrefab, _searchResultsParent);
            resultObject.GetComponentInChildren<TMP_Text>().text = result.Name;
            resultObject.GetComponent<Button>().onClick.AddListener(() => OnSearchResultClicked(result.Location));
            _searchResultObjects.Add(resultObject);
        }
    }

    void ClearSearchResults()
    {
        foreach (var resultObject in _searchResultObjects)
        {
            Destroy(resultObject);
        }
        _searchResultObjects.Clear();
    }

    void OnSearchResultClicked(Vector2 location)
    {
        _arMapController.DisplayRoute(location);
    }
}
