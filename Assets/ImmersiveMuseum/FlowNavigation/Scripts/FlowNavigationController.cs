using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gallery.FlowNavigation
{
    public class FlowNavigationController : MonoBehaviour
    {
        #region Variable Field
        [SerializeField] private ItemPreset[] _itemPresets = null;
        [SerializeField] private Transform _itemHolder = null;
        [SerializeField] private Transform _topAnchor = null;
        [SerializeField] private Transform _centerAnchor = null;
        [SerializeField] private Transform _bottomAnchor = null;
        [SerializeField] private TextMeshProUGUI _itemNameText = null;

        private FlowItem[] _activeItems = null;
        private int _selectedItemId = 0;

        #endregion

        private void Start()
        {
            StartCoroutine(CreateItems(() =>
            {
                int topItemId = _selectedItemId + 1 > _activeItems.Length - 1 ? 0 : _selectedItemId + 1;
                SetItemToAnchor(topItemId, _topAnchor);

                SetItemToAnchor(_selectedItemId, _centerAnchor, true);

                int bottomItemId = _selectedItemId - 1 < 0 ? _activeItems.Length - 1 : _selectedItemId - 1;
                SetItemToAnchor(bottomItemId, _bottomAnchor);
            }));
        }

        #region Flow Nav Features
        private IEnumerator CreateItems(Action done)
        {
            int createCount = 0;
            List<FlowItem> createdItems = new List<FlowItem>();
            for (int i = 0; i < _itemPresets.Length; i++)
            {
                int index = i;
                AsyncOperationHandle<GameObject> handle = _itemPresets[i].ItemRef.LoadAssetAsync<GameObject>();
                handle.Completed += (obj) => OnLoadInstantiateItem(obj, index);
            }

            while (createCount < _itemPresets.Length - 1)
                yield return null;

            _activeItems = createdItems.ToArray();
            done?.Invoke();

            void OnLoadInstantiateItem(AsyncOperationHandle<GameObject> obj, int index)
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject item = Instantiate(obj.Result, _itemHolder);
                    item.SetActive(false);
                    if (item.TryGetComponent<FlowItem>(out FlowItem flowItem))
                    {
                        flowItem.Name = _itemPresets[index].Name;
                        flowItem.Id = index;
                        createdItems.Add(flowItem);
                    }
                    createCount++;
                }
                else
                {
                    Debug.LogError($"{obj.Result.name} assetReference failed to load.");
                }
            }
        }

        private void SetItemToAnchor(int index, Transform anchor, bool isCenter = false)
        {
            FlowItem item = _activeItems.First(x => x.Id == index);
            if (item == null) return;

            item.transform.parent = anchor;
            item.transform.localPosition = Vector3.zero;
            item.transform.LookAt(Camera.main.transform, Vector3.up);
            item.gameObject.SetActive(true);
            if (isCenter)
                _itemNameText.text = item.Name;
        }
        #endregion
    }
}
