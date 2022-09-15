using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using CollieMollie.UI;
using CollieMollie.Helper;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEditor.Progress;

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
        [SerializeField] private Transform _topSpawnAnchor = null;
        [SerializeField] private Transform _bottomSpawnAnchor = null;
        [SerializeField] private int[] _activeItemIds = {0, 0, 0};

        [Header("UI")]
        [SerializeField] private UIButton _upButton = null;
        [SerializeField] private UIButton _downButton = null;
        [SerializeField] private TextMeshProUGUI _itemNameText = null;

        private FlowItem[] _createdItems = null;
        private int ItemsLastIndex
        {
            get => _createdItems.Length - 1;
        }

        private int TopId
        {
            get => _activeItemIds[0];
            set => _activeItemIds[0] = value;
        }

        private int CenterId
        {
            get => _activeItemIds[1];
            set => _activeItemIds[1] = value;
        }

        private int BottomId
        {
            get => _activeItemIds[2];
            set => _activeItemIds[2] = value;
        }
        #endregion

        private void Awake()
        {
            _upButton.OnPressed += (args) => UpdateItemPositions(args, 1);
            _downButton.OnPressed += (args) => UpdateItemPositions(args, -1);
        }

        private void Start()
        {
            StartCoroutine(CreateItems(() =>
            {
                TopId = CenterId + 1 > _createdItems.Length - 1 ? 0 : CenterId + 1;
                SetItemToAnchor(TopId, _topAnchor);

                SetItemToAnchor(_activeItemIds[1], _centerAnchor, true);

                BottomId = CenterId - 1 < 0 ? _createdItems.Length - 1 : CenterId - 1;
                SetItemToAnchor(BottomId, _bottomAnchor);
            }));
        }

        #region Subscribers
        public void UpdateItemPositions(UIEventArgs args, int dir)
        {
            bool upButtonClicked = dir > 0;
            if (upButtonClicked)
                _createdItems[BottomId].MoveToAnchor(_bottomSpawnAnchor, true);
            else
                _createdItems[TopId].MoveToAnchor(_topSpawnAnchor, true);

            CenterId = LoopId(CenterId, dir);

            if (upButtonClicked)
            {
                int topReadyId = LoopId(TopId, +1);
                _createdItems[topReadyId].MoveToAnchorReady(_topSpawnAnchor);
            }
            else
            {
                int bottomReadyId = LoopId(BottomId, -1);
                _createdItems[bottomReadyId].MoveToAnchorReady(_bottomSpawnAnchor);
            }

            TopId = LoopId(CenterId, +1);
            _createdItems[TopId].MoveToAnchor(_topAnchor);

            _createdItems[CenterId].MoveToAnchor(_centerAnchor);
            _itemNameText.text = _createdItems[CenterId].Name;

            BottomId = LoopId(CenterId, -1);
            _createdItems[BottomId].MoveToAnchor(_bottomAnchor);

            int LoopId(int baseNum, int value)
            {
                int nextValue = baseNum + value;
                return nextValue > ItemsLastIndex ? 0 :
                    nextValue < 0 ? ItemsLastIndex : nextValue;
            }
        }
        #endregion

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

            _createdItems = createdItems.OrderBy(x => x.Id).ToArray();
            done?.Invoke();

            void OnLoadInstantiateItem(AsyncOperationHandle<GameObject> obj, int index)
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject item = Instantiate(obj.Result, _itemHolder);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.LookAt(Vector3.forward, Vector3.up);
                    item.SetActive(false);

                    if (item.TryGetComponent<FlowItem>(out FlowItem flowItem))
                    {
                        flowItem.Name = _itemPresets[index].Name;
                        flowItem.Id = _itemPresets[index].Id;
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
            FlowItem item = _createdItems.First(x => x.Id == index);
            if (item == null) return;

            item.transform.parent = anchor;
            item.transform.localPosition = Vector3.zero;

            item.gameObject.SetActive(true);
            if (isCenter)
                _itemNameText.text = item.Name;
        }
        #endregion
    }
}
